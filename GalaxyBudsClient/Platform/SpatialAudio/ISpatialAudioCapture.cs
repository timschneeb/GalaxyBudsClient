using System;

namespace GalaxyBudsClient.Platform.SpatialAudio;

public interface ISpatialAudioCapture : IDisposable
{
    void Start(Action<Span<float>> onAudioCaptured);
    void Stop();
    bool IsRunning { get; }
    int SampleRate { get; }
}
