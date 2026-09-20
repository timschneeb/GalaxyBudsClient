using System;

namespace GalaxyBudsClient.Platform.SpatialAudio;

public class NullSpatialAudioSink : ISpatialAudioSink
{
    public bool IsRunning { get; private set; }
    public int SampleRate { get; }

    public NullSpatialAudioSink(int sampleRate = 48000)
    {
        SampleRate = sampleRate;
    }

    public void Start(Func<Span<float>, int> readStereoSamplesCallback)
    {
        IsRunning = true;
    }

    public void Stop()
    {
        IsRunning = false;
    }

    public void Dispose()
    {
        Stop();
    }
}

public static class SpatialAudioSinkFactory
{
    public static ISpatialAudioSink Create(int sampleRate = 48000)
    {
        if (OperatingSystem.IsMacOS())
        {
            return new MacOsSpatialAudioSink(sampleRate);
        }

        // Windows and Linux fallbacks
        return new NullSpatialAudioSink(sampleRate);
    }
}
