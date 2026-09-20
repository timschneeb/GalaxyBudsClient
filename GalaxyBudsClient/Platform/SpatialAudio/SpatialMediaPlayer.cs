using System;
using System.Threading;
using System.Threading.Tasks;
using ReactiveUI;
using Serilog;

namespace GalaxyBudsClient.Platform.SpatialAudio;

/// <summary>
/// Real-time 360 spatial audio player and stream engine.
/// Continuously generates and/or streams stereo audio through the SpatialAudioDspEngine
/// rotated in real time by head orientation from the Galaxy Buds 2 Pro motion sensors,
/// outputting low-latency binaural 3D sound to the active audio device.
/// </summary>
public sealed class SpatialMediaPlayer : ReactiveObject, IDisposable
{
    private static readonly object Padlock = new();
    private static SpatialMediaPlayer? _instance;
    public static SpatialMediaPlayer Instance
    {
        get
        {
            lock (Padlock)
            {
                return _instance ??= new SpatialMediaPlayer();
            }
        }
    }

    private const int SampleRate = 44100;
    private readonly SpatialAudioDspEngine _dspEngine = new(SampleRate);
    private ISpatialAudioSink? _audioSink;

    private bool _isPlaying;
    public bool IsPlaying
    {
        get => _isPlaying;
        private set => this.RaiseAndSetIfChanged(ref _isPlaying, value);
    }

    public float VirtualSpeakerAngle
    {
        get => _dspEngine.SpeakerAzimuthDeg;
        set
        {
            _dspEngine.SpeakerAzimuthDeg = Math.Clamp(value, 15f, 75f);
            this.RaisePropertyChanged();
        }
    }

    public float AmbienceAmount
    {
        get => _dspEngine.AmbienceAmount;
        set
        {
            _dspEngine.AmbienceAmount = Math.Clamp(value, 0f, 0.4f);
            this.RaisePropertyChanged();
        }
    }

    // Generator state for harmonic 360 music/ambience demo
    private double _sampleTime;
    private float[] _rawBuffer = new float[16384];

    private SpatialMediaPlayer()
    {
        SpatialAudioService.Instance.OrientationUpdated += OnOrientationUpdated;
    }

    private void OnOrientationUpdated(object? sender, SpatialOrientationEventArgs e)
    {
        _dspEngine.SetOrientation(e.Yaw, e.Pitch, e.Roll);
    }

    public void Play()
    {
        if (IsPlaying) return;

        try
        {
            Log.Information("SpatialMediaPlayer: Starting continuous 360 audio stream");
            _dspEngine.Reset();
            _dspEngine.SetOrientation(SpatialAudioService.Instance.CurrentYaw,
                                      SpatialAudioService.Instance.CurrentPitch,
                                      SpatialAudioService.Instance.CurrentRoll);
            _audioSink = SpatialAudioSinkFactory.Create(SampleRate);
            _audioSink.Start(OnProvideAudioSamples);
            IsPlaying = true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "SpatialMediaPlayer: Failed to start audio playback");
            Stop();
        }
    }

    public void Stop()
    {
        if (!IsPlaying && _audioSink == null) return;

        Log.Information("SpatialMediaPlayer: Stopping 360 audio stream");
        try
        {
            _audioSink?.Stop();
            _audioSink?.Dispose();
            _audioSink = null;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "SpatialMediaPlayer: Error stopping audio sink");
        }
        finally
        {
            IsPlaying = false;
        }
    }

    public void Toggle()
    {
        if (IsPlaying)
            Stop();
        else
            Play();
    }

    private int OnProvideAudioSamples(Span<float> outputBuffer)
    {
        var sampleCount = outputBuffer.Length;
        if (_rawBuffer.Length < sampleCount)
        {
            _rawBuffer = new float[sampleCount * 2];
        }

        var frameCount = sampleCount / 2;
        var rawSpan = _rawBuffer.AsSpan(0, sampleCount);

        // Synthesize a rich, soothing stereo ambient soundscape (anchored at physical screen):
        // Chord progression: Cmaj9 -> Am9 -> Fmaj7 -> Gsus4
        var chords = new (double c1, double c2, double c3, double c4)[]
        {
            (261.63, 329.63, 392.00, 493.88), // Cmaj9
            (220.00, 261.63, 329.63, 392.00), // Am9
            (174.61, 220.00, 261.63, 329.63), // Fmaj7
            (196.00, 261.63, 293.66, 392.00)  // Gsus4
        };

        const double secondsPerChord = 4.0;
        var dt = 1.0 / SampleRate;

        for (var i = 0; i < frameCount; i++)
        {
            var t = _sampleTime;
            _sampleTime += dt;

            var chordIdx = (int)((t / secondsPerChord) % chords.Length);
            var chord = chords[chordIdx];

            // Harmonic pad synth
            var pad1 = Math.Sin(2.0 * Math.PI * chord.c1 * t);
            var pad2 = Math.Sin(2.0 * Math.PI * chord.c2 * t);
            var pad3 = Math.Sin(2.0 * Math.PI * chord.c3 * t);
            var pad4 = Math.Sin(2.0 * Math.PI * chord.c4 * t);

            // Sub bass (centered anchor)
            var bass = Math.Sin(2.0 * Math.PI * (chord.c1 * 0.5) * t) * 0.4;

            // Stereo wide acoustic soundstage
            var leftChannel = (pad1 * 0.6 + pad3 * 0.5 + bass) * 0.22;
            var rightChannel = (pad2 * 0.6 + pad4 * 0.5 + bass) * 0.22;

            // Crisp 16th-note arpeggiator / chime providing high-frequency transients for instant 3D localization
            const double arpSpeed = 0.25; // 4 notes per second
            var arpNoteIdx = (int)(t / arpSpeed) % 4;
            var arpFreq = arpNoteIdx switch
            {
                0 => chord.c1 * 2.0,
                1 => chord.c2 * 2.0,
                2 => chord.c3 * 2.0,
                _ => chord.c4 * 2.0
            };

            var arpPhase = (t % arpSpeed) / arpSpeed;
            var arpEnv = Math.Exp(-5.0 * arpPhase);
            var arpWave = (Math.Sin(2.0 * Math.PI * arpFreq * t) +
                           0.35 * Math.Sin(4.0 * Math.PI * arpFreq * t)) * arpEnv * 0.18;

            // Alternate arpeggio notes across left and right virtual speakers
            if (arpNoteIdx % 2 == 0)
            {
                leftChannel += arpWave * 0.85;
                rightChannel += arpWave * 0.15;
            }
            else
            {
                leftChannel += arpWave * 0.15;
                rightChannel += arpWave * 0.85;
            }

            // Subtle spatial ping every 2 seconds
            var pingPhase = t % 2.0;
            if (pingPhase < 0.6)
            {
                var pingEnv = Math.Exp(-6.0 * pingPhase);
                var pingFreq = chord.c3 * 3.0; // ~1200 Hz
                var ping = Math.Sin(2.0 * Math.PI * pingFreq * pingPhase) * pingEnv * 0.12;
                leftChannel += ping * 0.5;
                rightChannel += ping * 0.5;
            }

            rawSpan[i * 2] = (float)leftChannel;
            rawSpan[i * 2 + 1] = (float)rightChannel;
        }

        // Apply real-time 3D Binaural DSP engine rotated by head orientation
        _dspEngine.Process(rawSpan, outputBuffer);

        return sampleCount;
    }

    public void Dispose()
    {
        Stop();
        SpatialAudioService.Instance.OrientationUpdated -= OnOrientationUpdated;
    }
}
