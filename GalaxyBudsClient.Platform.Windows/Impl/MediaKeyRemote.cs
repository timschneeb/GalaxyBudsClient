using System;
using System.Diagnostics.CodeAnalysis;
using GalaxyBudsClient.Bluetooth.Windows;
using GalaxyBudsClient.Platform.Interfaces;
using Serilog;
using WindowsInput;
using WindowsInput.Native;

namespace GalaxyBudsClient.Platform.Windows.Impl;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class MediaKeyRemote : IMediaKeyRemote
{
    public void Play()
    {
        if (!AudioPlaybackDetection.IsWindowsPlayingSound())
        {
            PlayPause();
            Log.Debug("Windows.MediaKeyRemote: All criteria are met; emitting play/pause keypress");
        }
        else
        {
            Log.Debug("Windows.MediaKeyRemote: Windows appears to playback sound; do not emit a play/pause keypress");
        }
    }

    public void Pause()
    {
        if (AudioPlaybackDetection.IsWindowsPlayingSound())
        {
            PlayPause();
            Log.Debug("Windows.MediaKeyRemote: All criteria are met; emitting play/pause keypress");
        }
        else
        {
            Log.Debug("Windows.MediaKeyRemote: Windows appears to playback no sound; do not emit a play/pause keypress");
        }
    }

    public void PlayPause()
    {
        try
        {
            new InputSimulator().Keyboard.KeyPress(VirtualKeyCode.MEDIA_PLAY_PAUSE);
        }
        catch (Exception ex)
        {
            Log.Error("Windows.MediaKeyRemote: Exception while sending keystroke: {ExMessage}", ex.Message);
        }
    }
}