using System;
using WindowsInput;
using WindowsInput.Native;
using GalaxyBudsClient.Platform.Interfaces;
using Serilog;
using ThePBone.Interop.Win32;

namespace GalaxyBudsClient.Platform.Windows
{
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
                Log.Error($"Windows.MediaKeyRemote: Exception while sending keystroke: {ex.Message}");
            }
        }
    }
}
