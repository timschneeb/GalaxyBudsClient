using System;
using GalaxyBudsClient.Platform.Interfaces;
using Serilog;
using Tmds.DBus;

namespace GalaxyBudsClient.Platform.Linux
{
    public class MediaKeyRemote : IMediaKeyRemote
    {
#if Linux
        private readonly ThePBone.MprisClient? _client;

        public MediaKeyRemote()
        {
            try
            {
                _client = new ThePBone.MprisClient();
            }
            catch (PlatformNotSupportedException)
            {
                _client = null;
            }
        }
        
        public void Play()
        {
            try
            {
                _client?.Player?.PlayAsync();
                Log.Debug("Linux.MediaKeyRemote: Play request sent");
            }
            catch (DBusException ex)
            {
                Log.Error($"{ex.ErrorName}: {ex.ErrorMessage}");
            }
        }

        public void Pause()
        {
            try
            {
                _client?.Player?.PauseAsync();
                Log.Debug("Linux.MediaKeyRemote: Pause request sent");
            }
            catch (DBusException ex)
            {
                Log.Error($"{ex.ErrorName}: {ex.ErrorMessage}");
            }
        }

        public void PlayPause()
        {
            try
            {
                _client?.Player?.PlayPauseAsync();
                Log.Debug("Linux.MediaKeyRemote: PlayPause request sent");
            }
            catch (DBusException ex)
            {
                Log.Error($"{ex.ErrorName}: {ex.ErrorMessage}");
            }
        }
#else
        public void Play()
        {
            _dummy.Play();
        }

        public void Pause()
        {
            _dummy.Pause();
        }

        public void PlayPause()
        {
            _dummy.PlayPause();
        }
        
        private readonly IMediaKeyRemote _dummy = new Dummy.MediaKeyRemote();
#endif
    }
}