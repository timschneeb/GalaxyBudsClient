using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using ThePBone.BlueZNet;
using ThePBone.BlueZNet.Interop;
using Tmds.DBus;

namespace GalaxyBudsClient.Bluetooth.Linux
{
    public class BluetoothService : IBluetoothService
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(15);
        private static readonly ConcurrentQueue<byte[]> TransmitterQueue = new ConcurrentQueue<byte[]>();
        
        private CancellationTokenSource _cancelSource = new CancellationTokenSource();
        private readonly BluetoothSocket _profile = new BluetoothSocket();
        private Task? _loop;
        
        private IAdapter1? _adapter;
        private IDevice1? _device;

        private string _currentMac = String.Empty;
        private string _currentUuid = String.Empty;

        private IDisposable? _connectionWatchdog;

        private IDisposable? ConnectionWatchdog
        {
            get => _connectionWatchdog;
            set
            {
                /* Update connection watcher */
                _connectionWatchdog?.Dispose();
                _connectionWatchdog = value;
            }
        }
        
        public event EventHandler? RfcommConnected;
        public event EventHandler? Connecting;
        public event EventHandler? Connected;
        public event EventHandler<string>? Disconnected;
        public event EventHandler<byte[]>? NewDataAvailable;

        public bool IsStreamConnected { get; set; }

        public BluetoothService()
        {
            RfcommConnected += (sender, args) => IsStreamConnected = true;
            Disconnected += (sender, args) => IsStreamConnected = false;
        }
        
        public async Task ConnectAsync(string macAddress, string uuid)
        {
            Connecting?.Invoke(this, EventArgs.Empty);
            
            if (_adapter == null)
            {
                Log.Debug("Linux.BluetoothService: No adapter preselected. Choosing default one.");
                await SelectAdapter();
            }
            
            _device = await _adapter.GetDeviceAsync(macAddress);
            if (_device == null)
            {
                Disconnected?.Invoke(this, 
                    "Bluetooth peripheral with address '{macAddress}' not found. Use `bluetoothctl` or Bluetooth Manager to scan and possibly pair first.");
                return;
            }

            _currentMac = macAddress;
            _currentUuid = uuid;
            
            var conn = new Connection(Address.System);
            await conn.ConnectAsync();
            
            _profile.NewConnection = (path, handle, arg3) => OnConnectionEstablished();
            _profile.RequestDisconnection = async (path, handle) => await DisconnectAsync();
            await conn.RegisterObjectAsync(_profile);

            for (int attempt = 1; attempt <= 15; attempt++)
            {
                Log.Debug($"Linux.BluetoothService: Connecting... (attempt {attempt}/15)");
                if (await AttemptBasicConnectionAsync())
                {
                    break;
                }
                if (attempt >= 15)
                {
                    Log.Fatal("Linux.BluetoothService: Gave up after 15 attempts. Timed out.");
                    throw new BluetoothException(BluetoothException.ErrorCodes.TimedOut, "BlueZ timed out while connecting to device.");
                }
            }

            await _device.WaitForPropertyValueAsync("Connected", value: true, Timeout);
            Connected?.Invoke(this, EventArgs.Empty);
            ConnectionWatchdog = _device.WatchForPropertyChangeAsync("Connected", true, async delegate(bool state)
            {
                if (state)
                {
                    Connected?.Invoke(this, EventArgs.Empty);
                    Log.Debug("Linux.BluetoothService: Reconnected. Attempting to auto-connect to profile...");
                    await ConnectAsync(_currentMac, _currentUuid);
                }
                else
                {
                    Disconnected?.Invoke(this, "Reported as disconnected by Bluez");
                    Log.Debug("Linux.BluetoothService: Disconnected");
                }
            });
            Log.Debug($"Linux.BluetoothService: Device ready. Registering profile client for UUID {uuid}...");

            var properties = new Dictionary<string, object>
            {
                ["Role"] = "client", 
                ["Service"] = uuid, 
                ["Name"] = "GalaxyBudsClient"
            };

            var profileManager = conn.CreateProxy<IProfileManager1>(BluezConstants.DbusService, "/org/bluez");

            try
            {
                await profileManager.RegisterProfileAsync(_profile.ObjectPath, uuid, properties);
            }
            catch (DBusException e)
            {
                var ex = new BlueZException(e);

                switch (ex.ErrorCode)
                {
                    case BlueZException.ErrorCodes.AlreadyExists:
                        Log.Warning("Linux.BluetoothService: Already registered. This may be fatal when multiple instances are active.");
                        break;
                    case BlueZException.ErrorCodes.InvalidArguments:
                        Log.Fatal($"Linux.BluetoothService: Invalid arguments. Cannot register profile: {ex.ErrorMessage}");
                        throw new BluetoothException(BluetoothException.ErrorCodes.Unknown, $"{ex.ErrorName}: {ex.ErrorMessage}");
                    default:
                        /* Other unknown dbus errors */
                        Log.Fatal($"Linux.BluetoothService: Cannot register profile. {ex.ErrorName}: {ex.ErrorMessage}");
                        throw new BluetoothException(BluetoothException.ErrorCodes.Unknown, $"{ex.ErrorName}: {ex.ErrorMessage}");
                }
            }
            
            for (int attempt = 1; attempt <= 15; attempt++)
            {
                Log.Debug($"Linux.BluetoothService: Connecting to profile... (attempt {attempt}/15)");

                try
                {
                    await _device.ConnectProfileAsync(uuid);
                    break;
                }
                catch (DBusException e)
                {
                    var ex = new BlueZException(e);

                    switch (ex.ErrorCode)
                    {
                        case BlueZException.ErrorCodes.Failed:
                            Log.Debug($"Linux.BluetoothService: Failed: '{ex.ErrorMessage}'. Retrying...");
                            await Task.Delay(250);
                            break;
                        case BlueZException.ErrorCodes.InProgress:
                            Log.Debug("Linux.BluetoothService: Already connecting, retrying...");
                            await Task.Delay(250);
                            break;
                        case BlueZException.ErrorCodes.AlreadyConnected:
                            Log.Debug("Linux.BluetoothService: Success. Already connected.");
                            return; /* We return here */
                        case BlueZException.ErrorCodes.ConnectFailed:
                            throw new BluetoothException(BluetoothException.ErrorCodes.ConnectFailed, $"{ex.ErrorName}: {ex.ErrorMessage}");
                        case BlueZException.ErrorCodes.DoesNotExist:
                            Log.Fatal("Linux.BluetoothService: Unsupported device. Device does not provide requested Bluetooth profile.");
                            throw new BluetoothException(BluetoothException.ErrorCodes.UnsupportedDevice, $"Device does not provide required Bluetooth profile");
                        default:
                            /* Other unknown dbus errors */
                            Log.Fatal($"Linux.BluetoothService: Cannot connect to profile. {ex.ErrorName}: {ex.ErrorMessage}");
                            throw new BluetoothException(BluetoothException.ErrorCodes.Unknown, $"{ex.ErrorName}: {ex.ErrorMessage}");
                    }
                }

                if (attempt >= 15)
                {
                    Log.Fatal("Linux.BluetoothService: Gave up after 15 attempts. Timed out.");
                    throw new BluetoothException(BluetoothException.ErrorCodes.TimedOut, "BlueZ timed out while connecting to profile");
                }
            }
            
            FINISH_PROFILE_CONNECTION: ;
        }

        private async Task<bool> AttemptBasicConnectionAsync()
        {
            try
            {
                await _device?.ConnectAsync()!;
            }
            catch (DBusException e)
            {
                var ex = new BlueZException(e);
                switch (ex.ErrorCode)
                {    
                    case BlueZException.ErrorCodes.Failed:
                        Log.Debug($"Linux.BluetoothService: Failed: '{ex.ErrorMessage}'. Retrying...");
                        await Task.Delay(250);
                        break;
                    
                    case BlueZException.ErrorCodes.InProgress:
                        Log.Debug("Linux.BluetoothService: Already connecting, retrying...");
                        await Task.Delay(250);
                        return false;
                        
                    case BlueZException.ErrorCodes.AlreadyConnected:
                        Log.Debug("Linux.BluetoothService: Already connected. Skipping ahead...");
                        break;
                    default:
                        /* org.bluez.Error.NotReady, org.bluez.Error.Failed */
                        Log.Fatal($"Linux.BluetoothService: Connect call failed due to: {ex.ErrorMessage} ({ex.ErrorCode})");
                        throw new BluetoothException(BluetoothException.ErrorCodes.Unknown, $"{ex.ErrorName}: {ex.ErrorMessage}");
                }
            }

            return true;
        }
            
        public async Task DisconnectAsync()
        {
            Log.Debug("Linux.BluetoothService: Disconnecting...");
            if (_loop == null || _loop.Status == TaskStatus.Created)
            {
                Log.Debug("Linux.BluetoothService: BluetoothServiceLoop not yet launched. No need to cancel.");
            }
            else
            {  
                Log.Debug("Linux.BluetoothService: Cancelling BluetoothServiceLoop...");
                _cancelSource.Cancel();
            }

            /* Disconnect device if not already done... */
            if (_device != null)
            {
                try
                {
                    await _device.DisconnectProfileAsync(_currentUuid);
                    Log.Debug("Linux.BluetoothService: Profile disconnected");
                }
                catch (DBusException ex)
                {
                    Log.Warning($"Linux.BluetoothService: (Non-critical) Exception raised while disconnecting profile: {ex.ErrorName}: {ex.ErrorMessage}");
                    /* Discard non-critical exceptions. */
                }
            }

            /* Attempt to unregister profile if not already done... */
            var profileManager = Connection.System.CreateProxy<IProfileManager1>(BluezConstants.DbusService, "/org/bluez");
            try
            {
                await profileManager.UnregisterProfileAsync(_profile.ObjectPath);
                Log.Debug("Linux.BluetoothService: Profile unregistered");
            }
            catch (DBusException ex)
            {
                Log.Warning($"Linux.BluetoothService: (Non-critical) Exception raised while unregistering profile: {ex.ErrorName}: {ex.ErrorMessage}");
                /* Discard non-critical exceptions. */
            }
        }

        public async Task SendAsync(byte[] data)
        {
            lock (TransmitterQueue)
            {
                TransmitterQueue.Enqueue(data);
            }
            await Task.CompletedTask;
        }
        
        private void OnConnectionEstablished()
        {
            Log.Debug("Linux.BluetoothService: Connection established. Launching BluetoothServiceLoop.");

            _loop?.Dispose();
            _cancelSource = new CancellationTokenSource();
            _loop = Task.Run(BluetoothServiceLoop, _cancelSource.Token);
                
            RfcommConnected?.Invoke(this, null);
        }
        
        public async Task SelectAdapter(string preferred = "")
        {
            if (preferred.Length > 0)
            {
                try
                {
                    _adapter = await BlueZManager.GetAdapterAsync(preferred);
                }
                catch(BlueZException ex)
                {
                    Log.Warning($"Preferred adapter not available: " + ex.ErrorName);
                    _adapter = null;
                }
            }
            
            if(_adapter == null || preferred.Length == 0)
            {
                var adapters = await BlueZManager.GetAdaptersAsync();
                if (adapters.Count == 0)
                {
                    throw new BluetoothException(BluetoothException.ErrorCodes.NoAdaptersAvailable);
                }

                _adapter = adapters.First();
            }
            
            var adapterPath = _adapter.ObjectPath.ToString();
            var adapterName = adapterPath.Substring(adapterPath.LastIndexOf("/", StringComparison.Ordinal) + 1);
            
            Log.Debug($"Linux.BluetoothService: Using Bluetooth adapter: {adapterName}");
        }
        
        private void BluetoothServiceLoop()
        {
            while (true)
            {
                _cancelSource.Token.ThrowIfCancellationRequested();

                /* Handle incoming stream */
                byte[] buffer = new byte[2048];
                var dataAvailable = false;
                try
                {
                    dataAvailable = _profile.Stream?.Read(buffer, 0, buffer.Length) >= 0;
                }
                catch (UnixSocketException ex)
                {
                    Log.Error($"Linux.BluetoothService: BluetoothServiceLoop: SocketException thrown while reading unsafe stream: {ex.Message}. Cancelled.");
                    Disconnected?.Invoke(this, ex.Message);
                    throw;
                }
                
                if (dataAvailable)
                {
                    NewDataAvailable?.Invoke(this, buffer);
                }

                /* Handle outgoing stream */
                lock (TransmitterQueue)
                {
                    if (TransmitterQueue.Count <= 0) continue;
                    if (!TransmitterQueue.TryDequeue(out var raw)) continue;
                    try
                    {
                        _profile.Stream?.Write(raw, 0, raw.Length);
                    }
                    catch (SocketException ex)
                    {
                        Log.Error($"Linux.BluetoothService: BluetoothServiceLoop: SocketException thrown while writing unsafe stream: {ex.Message}. Cancelled.");
                        Disconnected?.Invoke(this, ex.Message);
                    }
                    catch (IOException ex)
                    {
                        if (ex.InnerException != null && ex.InnerException.GetType() == typeof(SocketException))
                        {
                            Log.Error($"Linux.BluetoothService: BluetoothServiceLoop: IO and SocketException thrown while writing unsafe stream: {ex.Message}. Cancelled.");
                            Disconnected?.Invoke(this, ex.Message);
                        }
                    }
                }
            }
        }
    }
}