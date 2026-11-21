using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace GalaxyBudsClient.Platform.SpatialAudio;

public sealed unsafe class LinuxSpatialAudioSink : ISpatialAudioSink
{
    private const string LibPulseSimple = "libpulse-simple.so.0";

    [StructLayout(LayoutKind.Sequential)]
    private struct PaSampleSpec
    {
        public int format;     // PA_SAMPLE_FLOAT32LE = 5
        public uint rate;
        public byte channels;
    }

    private const int PA_STREAM_PLAYBACK = 1;
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

    [DllImport(LibPulseSimple, EntryPoint = "pa_simple_write")]
    private static extern int PaSimpleWrite(IntPtr s, void* data, nuint bytes, out int error);

    [DllImport(LibPulseSimple, EntryPoint = "pa_simple_free")]
    private static extern void PaSimpleFree(IntPtr s);

    private IntPtr _pulseHandle;
    private bool _isRunning;
    private CancellationTokenSource? _cts;
    private readonly object _lock = new();

    public bool IsRunning => _isRunning;
    public int SampleRate { get; }

    public LinuxSpatialAudioSink(int sampleRate = 48000)
    {
        SampleRate = sampleRate;
    }

    public void Start(Func<Span<float>, int> readStereoSamplesCallback)
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

            _pulseHandle = PaSimpleNew(null, "GalaxyBudsClient", PA_STREAM_PLAYBACK, null, "Spatial Audio Output", ref ss, IntPtr.Zero, IntPtr.Zero, out var error);
            if (_pulseHandle == IntPtr.Zero)
            {
                Log.Warning("LinuxSpatialAudioSink: Failed to open pulse playback stream, error code {Error}", error);
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
                        var read = readStereoSamplesCallback(buffer.AsSpan());
                        if (read <= 0)
                        {
                            Thread.Sleep(5);
                            continue;
                        }

                        fixed (float* ptr = buffer)
                        {
                            if (PaSimpleWrite(_pulseHandle, ptr, (nuint)(read * sizeof(float)), out var writeErr) < 0)
                            {
                                Log.Warning("LinuxSpatialAudioSink: Write failed, error {Error}", writeErr);
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "LinuxSpatialAudioSink: Playback loop encountered an error");
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
