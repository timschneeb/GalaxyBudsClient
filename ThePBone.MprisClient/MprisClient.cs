using System;
using System.Data.Common;
using System.Diagnostics;
using System.Threading.Tasks;
using Serilog;
using Tmds.DBus;

namespace ThePBone.MprisClient
{
    public class MprisClient
    {
        public IPlayer? Player
        {
            private set => _player = value;
            get
            {
                try
                {
                    UpdateTarget();
                }
                catch(DBusException ex)
                {                        
                    Log.Error($"MprisClient: Failed to update player target: {ex}");
                    return null;
                }
                return _player;
            }
        }

        private IPlayer? _player;
        private IDBus _dbus;
        private readonly Connection _connection;
        
        public MprisClient()
        {
            if (Address.Session == null)
            {
                Log.Error("MprisClient: Session bus unavailable. Cannot initialize");
                throw new PlatformNotSupportedException("Session bus unavailable");
            }
            
            var clientOptions = new ClientConnectionOptions(Address.Session)
            {
                AutoConnect = false
            };
         
            _connection = new Connection(clientOptions);
            _connection.ConnectAsync();
         
            _dbus = _connection.CreateProxy<IDBus>("org.freedesktop.DBus", "/");

            UpdateTarget();
        }
        
        private async void UpdateTarget()
        {
            var names = await _dbus.ListNamesAsync();
            foreach (var name in names)
            {
                if (name.StartsWith("org.mpris.MediaPlayer2"))
                {
                    try
                    {
                        _player = _connection.CreateProxy<IPlayer>(name, "/org/mpris/MediaPlayer2");
                    }
                    catch (DBusException)
                    {
                        Log.Error($"MprisClient: {name} is not ready");
                    }
                }
            }
        }
    }
}