using System;
using System.Runtime.InteropServices;
using ReactiveUI;
using Serilog;

namespace GalaxyBudsClient.Platform.SpatialAudio;

/// <summary>
/// Routes system-wide audio (YouTube, Spotify, Movies, Games) from BlackHole on macOS or WASAPI on Windows
/// through the SpatialAudioDspEngine to render full real-time 360 spatial audio to Galaxy Buds.
/// </summary>
public sealed class SpatialSystemAudioStreamer : ReactiveObject, IDisposable
{
    private static readonly object Padlock = new();
    private static SpatialSystemAudioStreamer? _instance;
    public static SpatialSystemAudioStreamer Instance
    {
        get
        {
            lock (Padlock)
            {
                return _instance ??= new SpatialSystemAudioStreamer();
            }
        }
    }

    private const int SampleRate = 44100;
    private readonly SpatialAudioDspEngine _dspEngine = new(SampleRate);
    private MacOsSpatialAudioCapture? _capture;
    private ISpatialAudioSink? _sink;
    private const int RingBufferSize = 32768;
    private readonly float[] _ringBuffer = new float[RingBufferSize];
    private float[] _spatialBuffer = new float[4096];
    private int _ringWritePos;
    private int _ringReadPos;
    private int _availableSamples;
    private readonly object _bufferLock = new();

    private bool _isActive;
    public bool IsActive
    {
        get => _isActive;
        private set => this.RaiseAndSetIfChanged(ref _isActive, value);
    }

    public float VirtualSpeakerAngle
    {
        get => _dspEngine.SpeakerAzimuthDeg;
        set => _dspEngine.SpeakerAzimuthDeg = value;
    }

    public float AmbienceAmount
    {
        get => _dspEngine.AmbienceAmount;
        set => _dspEngine.AmbienceAmount = value;
    }

    private SpatialSystemAudioStreamer()
    {
        SpatialAudioService.Instance.OrientationUpdated += (s, e) =>
        {
            _dspEngine.SetOrientation(e.Yaw, e.Pitch, e.Roll);
        };
    }

    public void Start()
    {
        if (IsActive) return;

        try
        {
            Log.Information("SpatialSystemAudioStreamer: Starting system-wide 360 audio loopback");
            _dspEngine.Reset();
            _dspEngine.SetOrientation(SpatialAudioService.Instance.CurrentYaw,
                                      SpatialAudioService.Instance.CurrentPitch,
                                      SpatialAudioService.Instance.CurrentRoll);

            lock (_bufferLock)
            {
                _ringWritePos = 0;
                _ringReadPos = 0;
                _availableSamples = 0;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                _sink = SpatialAudioSinkFactory.Create(SampleRate);
                _sink.Start(OnProvideAudioSamples);

                _capture = new MacOsSpatialAudioCapture(SampleRate);
                _capture.Start(OnAudioCaptured);
            }

            IsActive = true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "SpatialSystemAudioStreamer: Failed to start system audio loopback");
            Stop();
        }
    }

    private void OnAudioCaptured(Span<float> inputSpan)
    {
        lock (_bufferLock)
        {
            if (_spatialBuffer.Length < inputSpan.Length)
            {
                _spatialBuffer = new float[inputSpan.Length * 2];
            }

            // Spatial process input directly with DSP engine using live head orientation
            _dspEngine.Process(inputSpan, _spatialBuffer.AsSpan(0, inputSpan.Length));

            // Write spatialized samples into ring buffer
            var count = inputSpan.Length;
            for (var i = 0; i < count; i++)
            {
                _ringBuffer[_ringWritePos] = _spatialBuffer[i];
                _ringWritePos = (_ringWritePos + 1) % RingBufferSize;
            }
            _availableSamples = Math.Min(RingBufferSize, _availableSamples + count);
        }
    }

    private int OnProvideAudioSamples(Span<float> outputBuffer)
    {
        lock (_bufferLock)
        {
            var count = outputBuffer.Length;
            if (_availableSamples >= count)
            {
                for (var i = 0; i < count; i++)
                {
                    outputBuffer[i] = _ringBuffer[_ringReadPos];
                    _ringReadPos = (_ringReadPos + 1) % RingBufferSize;
                }
                _availableSamples -= count;
                return count;
            }

            if (_availableSamples > 0)
            {
                var avail = _availableSamples;
                for (var i = 0; i < avail; i++)
                {
                    outputBuffer[i] = _ringBuffer[_ringReadPos];
                    _ringReadPos = (_ringReadPos + 1) % RingBufferSize;
                }
                outputBuffer.Slice(avail).Clear();
                _availableSamples = 0;
                return count;
            }

            outputBuffer.Clear();
            return count;
        }
    }

    public void Stop()
    {
        if (!IsActive) return;
        Log.Information("SpatialSystemAudioStreamer: Stopping system-wide 360 loopback");

        try
        {
            _capture?.Stop();
            _capture?.Dispose();
            _capture = null;

            _sink?.Stop();
            _sink?.Dispose();
            _sink = null;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "SpatialSystemAudioStreamer: Error during stop");
        }
        finally
        {
            IsActive = false;
        }
    }

    public void Toggle()
    {
        if (IsActive) Stop();
        else Start();
    }

    public void Dispose()
    {
        Stop();
    }
}
