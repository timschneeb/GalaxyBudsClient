using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace GalaxyBudsClient.Platform.SpatialAudio;

public sealed unsafe class LinuxSpatialAudioCapture : ISpatialAudioCapture
{
    private const string LibPulseSimple = "libpulse-simple.so.0";

    [StructLayout(LayoutKind.Sequential)]
    private struct PaSampleSpec
    {
        public int format;     // PA_SAMPLE_FLOAT32LE = 5
        public uint rate;
        public byte channels;
    }

    private const int PA_STREAM_RECORD = 2;
    private const int PA_SAMPLE_FLOAT32LE = 5;

    [DllImport(LibPulseSimple, EntryPoint = "pa_simple_new")]
    private static extern IntPtr PaSimpleNew(
        string? server,
        string name,
        int dir,
        string? dev,
        string streamName,
        ref PaSampleSpec ss,
        IntPtr map,
        IntPtr attr,
        out int error);

    [DllImport(LibPulseSimple, EntryPoint = "pa_simple_read")]
    private static extern int PaSimpleRead(IntPtr s, void* data, nuint bytes, out int error);

    [DllImport(LibPulseSimple, EntryPoint = "pa_simple_free")]
    private static extern void PaSimpleFree(IntPtr s);

    private IntPtr _pulseHandle;
    private bool _isRunning;
    private CancellationTokenSource? _cts;
    private readonly object _lock = new();

    public bool IsRunning => _isRunning;
    public int SampleRate { get; }

    public LinuxSpatialAudioCapture(int sampleRate = 44100)
    {
        SampleRate = sampleRate;
    }

    public void Start(Action<Span<float>> onAudioCaptured)
    {
        lock (_lock)
        {
            if (_isRunning) return;

            var ss = new PaSampleSpec
            {
                format = PA_SAMPLE_FLOAT32LE,
                rate = (uint)SampleRate,
                channels = 2
            };

            // Capture from default monitor (system playback loopback)
            _pulseHandle = PaSimpleNew(null, "GalaxyBudsClient", PA_STREAM_RECORD, "@DEFAULT_MONITOR@", "Spatial Audio Capture", ref ss, IntPtr.Zero, IntPtr.Zero, out var error);
            if (_pulseHandle == IntPtr.Zero)
            {
                // Fallback to default source
                _pulseHandle = PaSimpleNew(null, "GalaxyBudsClient", PA_STREAM_RECORD, null, "Spatial Audio Capture", ref ss, IntPtr.Zero, IntPtr.Zero, out error);
            }

            if (_pulseHandle == IntPtr.Zero)
            {
                Log.Warning("LinuxSpatialAudioCapture: Failed to open pulse capture stream, error {Error}", error);
                return;
            }

            _isRunning = true;
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            Task.Run(() =>
            {
                const int bufferFrames = 512;
                var buffer = new float[bufferFrames * 2];

                try
                {
                    while (!token.IsCancellationRequested && _isRunning)
                    {
                        fixed (float* ptr = buffer)
                        {
                            if (PaSimpleRead(_pulseHandle, ptr, (nuint)(buffer.Length * sizeof(float)), out var readErr) < 0)
                            {
                                Log.Warning("LinuxSpatialAudioCapture: Read error {Error}", readErr);
                                break;
                            }
                        }

                        onAudioCaptured(buffer.AsSpan());
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "LinuxSpatialAudioCapture: Capture loop encountered an error");
                }
            }, token);
        }
    }

    public void Stop()
    {
        lock (_lock)
        {
            if (!_isRunning) return;
            _isRunning = false;
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            if (_pulseHandle != IntPtr.Zero)
            {
                PaSimpleFree(_pulseHandle);
                _pulseHandle = IntPtr.Zero;
            }
        }
    }

    public void Dispose()
    {
        Stop();
    }
}
