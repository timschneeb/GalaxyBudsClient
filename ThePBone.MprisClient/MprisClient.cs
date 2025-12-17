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
        public IPlayer? Player => _player;

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
        }

        public async Task InitializeAsync()
        {
            await _connection.ConnectAsync().ConfigureAwait(false);
            _dbus = _connection.CreateProxy<IDBus>("org.freedesktop.DBus", "/");
            await UpdateTargetAsync().ConfigureAwait(false);
        }
        
        public async Task UpdateTargetAsync()
        {
            if (_dbus == null) return;

            var names = await _dbus.ListNamesAsync().ConfigureAwait(false);
            foreach (var name in names)
            {
                if (name.StartsWith("org.mpris.MediaPlayer2"))
                {
                    try
                    {
                        // TODO: Better logic to select the "best" player if multiple exist?
                        _player = _connection.CreateProxy<IPlayer>(name, "/org/mpris/MediaPlayer2");
                        return; // Found one, stop searching
                    }
                    catch (DBusException)
                    {
                        Log.Error("MprisClient: {Name} is not ready", name);
                    }
                }
            }
        }
    }
}