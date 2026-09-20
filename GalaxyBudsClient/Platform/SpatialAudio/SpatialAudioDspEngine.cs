using System;

namespace GalaxyBudsClient.Platform.SpatialAudio;

/// <summary>
/// High-performance, zero-allocation real-time DSP engine for 360 binaural spatial audio.
/// Takes standard stereo audio (e.g. system sound, YouTube, Spotify, movies) and renders
/// virtual speakers anchored in 3D space in front of the listener using Woodworth ITD,
/// frequency-dependent ILD head shadow biquad filters, and subtle early room reflections.
/// </summary>
public sealed class SpatialAudioDspEngine
{
    private readonly int _sampleRate;
    private readonly float _invSampleRate;

    // Head physical model parameters
    private const float HeadRadiusMeters = 0.0875f; // ~8.75 cm
    private const float SpeedOfSound = 343.0f;     // m/s
    private readonly float _maxDelaySeconds;
    private readonly float _maxDelaySamples;

    // Fractional delay lines for each ear: [0] = Left Ear, [1] = Right Ear
    // Circular buffer length of 1024 is plenty for ~32 samples max delay + early reflections
    private const int DelayBufferSize = 1024;
    private const int DelayMask = DelayBufferSize - 1;
    private readonly float[] _delayBufferLeftIn = new float[DelayBufferSize];
    private readonly float[] _delayBufferRightIn = new float[DelayBufferSize];
    private int _writeIndex;

    // Current smoothed orientation (in radians)
    private float _targetYaw;
    private float _targetPitch;
    private float _targetRoll;

    private float _smoothedYaw;
    private float _smoothedPitch;
    private float _smoothedRoll;

    // Filter states for Head Shadow (Biquad low-shelf / low-pass for each channel and ear)
    // Left input -> Left Ear, Left input -> Right Ear, Right input -> Left Ear, Right input -> Right Ear
    private struct BiquadState
    {
        public float X1, X2;
        public float Y1, Y2;

        public void Reset()
        {
            X1 = X2 = Y1 = Y2 = 0f;
        }
    }

    private BiquadState _filterLL;
    private BiquadState _filterLR;
    private BiquadState _filterRL;
    private BiquadState _filterRR;

    // Configuration
    public float SpeakerAzimuthDeg { get; set; } = 30.0f; // Virtual stereo speakers at ±30°
    public float AmbienceAmount { get; set; } = 0.12f;    // Subtle cross-reflection to externalize sound

    public SpatialAudioDspEngine(int sampleRate = 48000)
    {
        _sampleRate = Math.Max(22050, Math.Min(192000, sampleRate));
        _invSampleRate = 1.0f / _sampleRate;
        _maxDelaySeconds = (HeadRadiusMeters / SpeedOfSound) * ((MathF.PI / 2.0f) + 1.0f);
        _maxDelaySamples = _maxDelaySeconds * _sampleRate;
    }

    /// <summary>
    /// Update the listener's head orientation in degrees.
    /// </summary>
    public void SetOrientation(float yawDegrees, float pitchDegrees, float rollDegrees)
    {
        _targetYaw = yawDegrees * (MathF.PI / 180.0f);
        _targetPitch = pitchDegrees * (MathF.PI / 180.0f);
        _targetRoll = rollDegrees * (MathF.PI / 180.0f);
    }

    /// <summary>
    /// Process interleaved 32-bit floating point stereo PCM audio frames.
    /// </summary>
    public void Process(ReadOnlySpan<float> input, Span<float> output)
    {
        var frameCount = Math.Min(input.Length / 2, output.Length / 2);
        if (frameCount <= 0) return;

        // Smoothing factor for parameter transitions (per sample)
        const float smoothAlpha = 0.005f;

        var speakerAngleRad = SpeakerAzimuthDeg * (MathF.PI / 180.0f);

        for (var i = 0; i < frameCount; i++)
        {
            var inIdx = i * 2;
            var inL = input[inIdx];
            var inR = input[inIdx + 1];

            // Smooth orientation
            _smoothedYaw += (_targetYaw - _smoothedYaw) * smoothAlpha;
            _smoothedPitch += (_targetPitch - _smoothedPitch) * smoothAlpha;
            _smoothedRoll += (_targetRoll - _smoothedRoll) * smoothAlpha;

            // Write input samples to circular delay lines
            _delayBufferLeftIn[_writeIndex] = inL;
            _delayBufferRightIn[_writeIndex] = inR;

            // Compute virtual speaker relative angles
            // Left virtual speaker is at -speakerAngleRad in world space
            // Relative azimuth to head: relAngle = worldAngle - yaw
            var relAngleL = -speakerAngleRad - _smoothedYaw;
            var relAngleR = speakerAngleRad - _smoothedYaw;

            // Wrap relative angles to [-PI, PI]
            relAngleL = NormalizeAngle(relAngleL);
            relAngleR = NormalizeAngle(relAngleR);

            // Calculate ITD (delays in samples) and ILD (gain & filter cutoff) for:
            // 1. Left virtual speaker to Left and Right ears
            CalculateEarResponse(relAngleL, out var delayL_to_L, out var gainL_to_L, out var cutoffL_to_L,
                                            out var delayL_to_R, out var gainL_to_R, out var cutoffL_to_R);

            // 2. Right virtual speaker to Left and Right ears
            CalculateEarResponse(relAngleR, out var delayR_to_L, out var gainR_to_L, out var cutoffR_to_L,
                                            out var delayR_to_R, out var gainR_to_R, out var cutoffR_to_R);

            // Read delayed samples with linear fractional interpolation
            var sampleLL = ReadFractionalDelay(_delayBufferLeftIn, _writeIndex, delayL_to_L);
            var sampleLR = ReadFractionalDelay(_delayBufferLeftIn, _writeIndex, delayL_to_R);
            var sampleRL = ReadFractionalDelay(_delayBufferRightIn, _writeIndex, delayR_to_L);
            var sampleRR = ReadFractionalDelay(_delayBufferRightIn, _writeIndex, delayR_to_R);

            // Apply ILD head shadow filters (frequency-dependent attenuation)
            sampleLL = ApplyHeadShadowFilter(sampleLL, cutoffL_to_L, ref _filterLL) * gainL_to_L;
            sampleLR = ApplyHeadShadowFilter(sampleLR, cutoffL_to_R, ref _filterLR) * gainL_to_R;
            sampleRL = ApplyHeadShadowFilter(sampleRL, cutoffR_to_L, ref _filterRL) * gainR_to_L;
            sampleRR = ApplyHeadShadowFilter(sampleRR, cutoffR_to_R, ref _filterRR) * gainR_to_R;

            // Multi-tap room early reflection cluster simulating acoustic walls & floor:
            // Tap 1: Cross-wall early bounce (~3.6 ms, 160 samples)
            // Tap 2: Rear wall reflection (~8.2 ms, 360 samples)
            // Tap 3: Floor / ceiling reflection (~15 ms, 660 samples)
            var refl1_L = ReadFractionalDelay(_delayBufferRightIn, _writeIndex, 160.0f);
            var refl1_R = ReadFractionalDelay(_delayBufferLeftIn, _writeIndex, 160.0f);

            var refl2_L = ReadFractionalDelay(_delayBufferLeftIn, _writeIndex, 360.0f);
            var refl2_R = ReadFractionalDelay(_delayBufferRightIn, _writeIndex, 360.0f);

            var refl3_L = ReadFractionalDelay(_delayBufferRightIn, _writeIndex, 660.0f);
            var refl3_R = ReadFractionalDelay(_delayBufferLeftIn, _writeIndex, 660.0f);

            var roomL = ((refl1_L * 0.5f) + (refl2_L * 0.35f) + (refl3_L * 0.25f)) * (AmbienceAmount * 2.5f);
            var roomR = ((refl1_R * 0.5f) + (refl2_R * 0.35f) + (refl3_R * 0.25f)) * (AmbienceAmount * 2.5f);

            // Sum outputs for Left Ear and Right Ear with natural acoustic wet/dry balance
            var directMix = 1.0f - (AmbienceAmount * 0.35f);
            var outL = ((sampleLL + sampleRL) * directMix) + roomL;
            var outR = ((sampleLR + sampleRR) * directMix) + roomR;

            // Soft clipper to prevent any digital distortion
            output[inIdx] = SoftClip(outL);
            output[inIdx + 1] = SoftClip(outR);

            _writeIndex = (_writeIndex + 1) & DelayMask;
        }
    }

    /// <summary>
    /// Process interleaved 16-bit signed PCM audio bytes.
    /// </summary>
    public void Process16Bit(ReadOnlySpan<byte> inputBytes, Span<byte> outputBytes)
    {
        var sampleCount = Math.Min(inputBytes.Length / 2, outputBytes.Length / 2);
        var frameCount = sampleCount / 2;
        if (frameCount <= 0) return;

        // Process in chunks of up to 256 frames on stack to avoid heap allocation
        Span<float> floatIn = stackalloc float[512];
        Span<float> floatOut = stackalloc float[512];

        var processedFrames = 0;
        while (processedFrames < frameCount)
        {
            var chunkFrames = Math.Min(256, frameCount - processedFrames);
            var chunkSamples = chunkFrames * 2;

            var inByteOffset = processedFrames * 4;
            for (var s = 0; s < chunkSamples; s++)
            {
                var byteIdx = inByteOffset + (s * 2);
                var sampleInt16 = (short)(inputBytes[byteIdx] | (inputBytes[byteIdx + 1] << 8));
                floatIn[s] = sampleInt16 / 32768.0f;
            }

            Process(floatIn[..chunkSamples], floatOut[..chunkSamples]);

            var outByteOffset = processedFrames * 4;
            for (var s = 0; s < chunkSamples; s++)
            {
                var val = (int)(floatOut[s] * 32767.0f);
                val = Math.Clamp(val, -32768, 32767);
                var byteIdx = outByteOffset + (s * 2);
                outputBytes[byteIdx] = (byte)(val & 0xFF);
                outputBytes[byteIdx + 1] = (byte)((val >> 8) & 0xFF);
            }

            processedFrames += chunkFrames;
        }
    }

    private void CalculateEarResponse(float relAngleRad,
        out float delayL, out float gainL, out float cutoffL,
        out float delayR, out float gainR, out float cutoffR)
    {
        // Woodworth spherical head model for Left Ear (located at -PI/2) and Right Ear (+PI/2)
        // Angle to Left Ear: thetaL = relAngle + PI/2
        // Angle to Right Ear: thetaR = relAngle - PI/2
        var thetaL = NormalizeAngle(relAngleRad + (MathF.PI / 2.0f));
        var thetaR = NormalizeAngle(relAngleRad - (MathF.PI / 2.0f));

        // Interaural Time Difference (ITD) delay in seconds
        var delaySecL = WoodworthDelay(thetaL);
        var delaySecR = WoodworthDelay(thetaR);

        delayL = delaySecL * _sampleRate;
        delayR = delaySecR * _sampleRate;

        // Interaural Level Difference (ILD)
        // Standard pan: -1 when source is to listener's left, +1 when source is to listener's right
        var pan = Math.Clamp(MathF.Sin(relAngleRad), -1.0f, 1.0f);

        // Constant power panning law
        // pan = -1 (hard left): gainL = 1.0, gainR = 0.0
        // pan =  0 (center):    gainL = 0.707, gainR = 0.707
        // pan = +1 (hard right):gainL = 0.0, gainR = 1.0
        var panAngle = (pan + 1.0f) * (MathF.PI / 4.0f);
        gainL = MathF.Cos(panAngle);
        gainR = MathF.Sin(panAngle);

        // Head shadow cutoff frequency:
        // Ipsilateral ear gets direct high frequencies (cutoff up to 20kHz)
        // Contralateral ear is shadowed by head (cutoff down to ~1.8kHz)
        // Left ear is shadowed only when sound is to listener's right (pan > 0)
        // Right ear is shadowed only when sound is to listener's left (pan < 0)
        var shadowDepthL = Math.Clamp(pan, 0f, 1f);
        var shadowDepthR = Math.Clamp(-pan, 0f, 1f);

        cutoffL = MathF.Exp(MathF.Log(1800.0f) * shadowDepthL + MathF.Log(20000.0f) * (1.0f - shadowDepthL));
        cutoffR = MathF.Exp(MathF.Log(1800.0f) * shadowDepthR + MathF.Log(20000.0f) * (1.0f - shadowDepthR));
    }

    private static float WoodworthDelay(float theta)
    {
        var absTheta = MathF.Abs(theta);
        float delay;
        if (absTheta <= MathF.PI / 2.0f)
        {
            delay = (HeadRadiusMeters / SpeedOfSound) * (1.0f - MathF.Cos(absTheta));
        }
        else
        {
            delay = (HeadRadiusMeters / SpeedOfSound) * (1.0f + (absTheta - (MathF.PI / 2.0f)));
        }
        return Math.Max(0.0f, delay);
    }

    private static float ReadFractionalDelay(float[] buffer, int writeIdx, float delaySamples)
    {
        var readPos = writeIdx - delaySamples;
        while (readPos < 0) readPos += DelayBufferSize;

        var index0 = (int)readPos;
        var frac = readPos - index0;
        var index1 = (index0 + 1) & DelayMask;

        index0 &= DelayMask;

        // Linear interpolation
        return (buffer[index0] * (1.0f - frac)) + (buffer[index1] * frac);
    }

    private float ApplyHeadShadowFilter(float input, float cutoffHz, ref BiquadState state)
    {
        // 1st order low-pass IIR filter: y[n] = (1 - alpha) * x[n] + alpha * y[n-1]
        var w = 2.0f * MathF.PI * cutoffHz * _invSampleRate;
        var alpha = Math.Clamp(MathF.Exp(-w), 0.0f, 0.98f);

        var output = ((1.0f - alpha) * input) + (alpha * state.Y1);
        state.Y1 = output;
        return output;
    }

    private static float SoftClip(float x)
    {
        if (x > 1.0f) return 1.0f - MathF.Exp(-x);
        if (x < -1.0f) return -1.0f + MathF.Exp(x);
        return x;
    }

    private static float NormalizeAngle(float angle)
    {
        while (angle > MathF.PI) angle -= 2.0f * MathF.PI;
        while (angle < -MathF.PI) angle += 2.0f * MathF.PI;
        return angle;
    }

    public void Reset()
    {
        Array.Clear(_delayBufferLeftIn);
        Array.Clear(_delayBufferRightIn);
        _filterLL.Reset();
        _filterLR.Reset();
        _filterRL.Reset();
        _filterRR.Reset();
        _writeIndex = 0;
    }
}
