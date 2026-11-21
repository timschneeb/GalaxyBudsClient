using System;

namespace GalaxyBudsClient.Platform.SpatialAudio;

public static class SpatialAudioCaptureFactory
{
    public static ISpatialAudioCapture? Create(int sampleRate = 44100)
    {
        if (OperatingSystem.IsMacOS())
        {
            return new MacOsSpatialAudioCapture(sampleRate);
        }

        if (OperatingSystem.IsLinux())
        {
            return new LinuxSpatialAudioCapture(sampleRate);
        }

        return null;
    }
}
