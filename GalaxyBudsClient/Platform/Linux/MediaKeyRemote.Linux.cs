using System;
using GalaxyBudsClient.Platform.Interfaces;
using Serilog;
using ThePBone.MprisClient;
using Tmds.DBus;

namespace GalaxyBudsClient.Platform.Linux
{
    public class MediaKeyRemote : IMediaKeyRemote
    {
        private readonly MprisClient? _client;

        public MediaKeyRemote()
        {
            try
            {
                _client = new MprisClient();
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
    }
}