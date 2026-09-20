using System;

namespace GalaxyBudsClient.Platform.SpatialAudio;

public interface ISpatialAudioSink : IDisposable
{
    void Start(Func<Span<float>, int> readStereoSamplesCallback);
    void Stop();
    bool IsRunning { get; }
    int SampleRate { get; }
}
