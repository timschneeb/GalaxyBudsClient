using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Serilog;

namespace GalaxyBudsClient.Bluetooth.WindowsRT
{
    public class BluetoothService : IBluetoothService
    {
        public event EventHandler<BluetoothException>? BluetoothErrorAsync;
        public event EventHandler? Connecting;
        public event EventHandler? Connected;
        public event EventHandler? RfcommConnected;
        public event EventHandler<string>? Disconnected;
        public event EventHandler<byte[]>? NewDataAvailable;
        public bool IsStreamConnected { set; get; }
        public bool IsConnecting { set; get; }

        private readonly DeviceWatcher _deviceWatcher;
        private StreamSocket? _socket;
        private DataWriter? _writer;
        private RfcommDeviceService? _service;
        private Windows.Devices.Bluetooth.BluetoothDevice? _bluetoothDevice;

        private Task? _loop; 
        private CancellationTokenSource _readerCancellation = new CancellationTokenSource();
        private CancellationTokenSource _loopCancellation = new CancellationTokenSource();

        private readonly HashSet<BluetoothDeviceRT> _deviceCache = new HashSet<BluetoothDeviceRT>();
        
        public BluetoothService()
        {
            Connecting += (sender, args) => IsConnecting = true;
            Connected += (sender, args) => IsConnecting = false;
            BluetoothErrorAsync += (sender, exception) =>
            {
                IsConnecting = false;
                IsStreamConnected = false;
            };
            Disconnected += (sender, exception) =>
            {
                IsConnecting = false;
                IsStreamConnected = false;
            };
            
            string[] requestedProperties = { 
                "System.Devices.Aep.DeviceAddress", 
                "System.Devices.Aep.IsConnected",
                "System.Devices.Aep.IsPaired"
            };

            try
            {
                _deviceWatcher = DeviceInformation.CreateWatcher(
                    "(System.Devices.Aep.ProtocolId:=\"{e0cbf06c-cd8b-4647-bb8a-263b43f0f974}\")",
                    requestedProperties,
                    DeviceInformationKind.AssociationEndpoint);

                _deviceWatcher.Added += (watcher, deviceInfo) =>
                {
                    Log.Debug($"WindowsRT.BluetoothService: Device added: {deviceInfo.Id}");
                    if (deviceInfo.Name != string.Empty)
                    {
                        _deviceCache?.Add(new BluetoothDeviceRT(deviceInfo));
                    }
                };

                _deviceWatcher.Updated += (watcher, deviceInfoUpdate) =>
                {
                    Log.Debug($"WindowsRT.BluetoothService: Device updated: {deviceInfoUpdate?.Id}");

                    _deviceCache?.Where(x => x?.Id == deviceInfoUpdate?.Id).ToList().ForEach(async x =>
                    {
                        if (string.Equals(x.Address, _bluetoothDevice?.BluetoothAddressAsString(),
                                StringComparison.CurrentCultureIgnoreCase) &&
                            (deviceInfoUpdate?.Properties?.ContainsKey("System.Devices.Aep.IsConnected") ?? false))
                        {
                            if ((bool) deviceInfoUpdate.Properties["System.Devices.Aep.IsConnected"])
                            {
                                Log.Debug($"WindowsRT.BluetoothService: Target device connected");
                                await ConnectAsync(x.Address,
                                    _service?.ServiceId?.AsString() ?? "{00001101-0000-1000-8000-00805F9B34FB}");
                            }
                            else
                            {
                                Log.Debug($"WindowsRT.BluetoothService: Target device disconnected");
                                Disconnected?.Invoke(this, "Device disconnected");
                            }
                        }
                    });
                    _deviceCache?.Where(x => deviceInfoUpdate?.Id == x?.Id)
                        .ToList()
                        .ForEach(x => x?.Update(deviceInfoUpdate ?? null));
                };

                _deviceWatcher.Removed += (watcher, deviceInfoUpdate) =>
                {
                    Log.Debug($"WindowsRT.BluetoothService: Device removed: {deviceInfoUpdate.Id}");

                    _deviceCache?.Where(x => x?.Id == deviceInfoUpdate?.Id).ToList().ForEach(x =>
                    {
                        if (string.Equals(x?.Address, _bluetoothDevice?.BluetoothAddressAsString(),
                            StringComparison.CurrentCultureIgnoreCase))
                        {
                            Log.Debug($"WindowsRT.BluetoothService: Target device removed");
                            Disconnected?.Invoke(this,
                                "Device was removed/un-paired from Windows. Please check your computer's bluetooth settings.");
                        }
                    });
                    _deviceCache?.RemoveWhere(x => x?.Id == deviceInfoUpdate?.Id);
                };

                _deviceWatcher.EnumerationCompleted += (watcher, obj) =>
                {
                    Log.Debug("WindowsRT.BluetoothService: Device enumeration complete");
                };

                _deviceWatcher.Stopped += (watcher, obj) =>
                {
                    Log.Warning("WindowsRT.BluetoothService: Device watcher stopped");
                };

                _deviceWatcher.Start();
                Log.Debug("WindowsRT.BluetoothService: Device watcher launched");
            }
            catch (ArgumentException ex)
            {
                Log.Error(
                    $"WindowsRT.BluetoothService: Failed to set up device watcher. Protocol GUID probably not found. Details: {ex}");
                throw new PlatformNotSupportedException("Failed to set up device watcher. Make sure you have a compatible Bluetooth driver installed.");
            }
        }
        
        public async Task ConnectAsync(string macAddress, string serviceUuid, bool noRetry = false)
        {
            if (IsConnecting)
            {
                Log.Debug($"WindowsRT.BluetoothService: Already connecting. Skipping request.");
                return;
            }
            if (IsStreamConnected)
            {
                Log.Debug($"WindowsRT.BluetoothService: Already connected. Skipping request.");
                return;
            }
            
            Connecting?.Invoke(this, EventArgs.Empty);

            try
            {
                var matches = _deviceCache.Where(x =>
                    string.Equals(x.Address, macAddress, StringComparison.CurrentCultureIgnoreCase)).ToList();
                if (matches.Count <= 0)
                {
                    Log.Error(
                        $"WindowsRT.BluetoothService: Registered device not available. Expected MAC: {macAddress}");
                    BluetoothErrorAsync?.Invoke(this, new BluetoothException(
                        BluetoothException.ErrorCodes.ConnectFailed,
                        "Device unavailable. Not device with registered MAC address not found nearby. If you are certain that your earbuds are connected to this computer, please unregister them and try again."));
                }
                else
                {
                    Log.Debug(
                        $"WindowsRT.BluetoothService: Selected '{matches[0].Name}' ({matches[0].Address}) from cache as target");
                }

                // Perform device access checks before trying to get the device.
                // First, we check if consent has been explicitly denied by the user.
                var accessStatus = DeviceAccessInformation.CreateFromId(matches[0].Id).CurrentStatus;
                if (accessStatus == DeviceAccessStatus.DeniedByUser)
                {
                    Log.Error($"WindowsRT.BluetoothService: Access to device explicitly denied by user");
                    BluetoothErrorAsync?.Invoke(this,
                        new BluetoothException(BluetoothException.ErrorCodes.ConnectFailed,
                            "This app does not have access to connect to the remote device (please grant access in Settings > Privacy > Other Devices"));
                    return;
                }

                if (accessStatus == DeviceAccessStatus.DeniedBySystem)
                {
                    Log.Error($"WindowsRT.BluetoothService: Access to device denied by system");
                    BluetoothErrorAsync?.Invoke(this,
                        new BluetoothException(BluetoothException.ErrorCodes.ConnectFailed,
                            "Access denied by system. This app does not have access to connect to the remote device"));
                    return;
                }

                // If not, try to get the Bluetooth device
                try
                {
                    _bluetoothDevice = await Windows.Devices.Bluetooth.BluetoothDevice.FromIdAsync(matches[0].Id);
                }
                catch (Exception ex)
                {
                    Log.Error(
                        $"WindowsRT.BluetoothService: Error while getting Bluetooth device from cached id: {ex.Message}");
                    BluetoothErrorAsync?.Invoke(this,
                        new BluetoothException(BluetoothException.ErrorCodes.ConnectFailed,
                            ex.Message));
                    return;
                }

                // If we were unable to get a valid Bluetooth device object,
                // it's most likely because the user has specified that all unpaired devices
                // should not be interacted with.
                if (_bluetoothDevice == null)
                {
                    Log.Error($"WindowsRT.BluetoothService: BluetoothDevice.FromIdAsync returned NULL");
                    BluetoothErrorAsync?.Invoke(this,
                        new BluetoothException(BluetoothException.ErrorCodes.ConnectFailed,
                            "Unable to retrieve device object. Try to re-pair your Bluetooth device."));
                    return;
                }

                // This should return a list of uncached Bluetooth services (so if the server was not active when paired, it will still be detected by this call
                var rfcommServices = await _bluetoothDevice.GetRfcommServicesForIdAsync(
                    RfcommServiceId.FromUuid(new Guid(serviceUuid)), BluetoothCacheMode.Uncached);

                if (rfcommServices.Services.Count > 0)
                {
                    _service = rfcommServices.Services[0];
                }
                else
                {
                    Log.Error($"WindowsRT.BluetoothService: SPP service not discovered");
                    BluetoothErrorAsync?.Invoke(this,
                        new BluetoothException(BluetoothException.ErrorCodes.ConnectFailed,
                            "Unable to discover SDP record for the RFCOMM protocol. Either your earbuds are out of range or ran out of battery."));
                    return;
                }

                lock (this)
                {
                    _socket = new StreamSocket();
                }

                try
                {
                    await _socket.ConnectAsync(_service.ConnectionHostName, _service.ConnectionServiceName);
                    Connected?.Invoke(this, EventArgs.Empty);
                    Log.Debug($"WindowsRT.BluetoothService: Connected");

                    _writer = new DataWriter(_socket.OutputStream);

                    Log.Debug("WindowsRT.BluetoothService: Launching BluetoothServiceLoop...");
                    RfcommConnected?.Invoke(this, EventArgs.Empty);

                    IsStreamConnected = true;

                    _loopCancellation = new CancellationTokenSource();
                    _loop = Task.Run(BluetoothServiceLoop);
                }
                catch (Exception ex) when ((uint) ex.HResult == 0x80070490) // ERROR_ELEMENT_NOT_FOUND
                {
                    Log.Error(
                        "WindowsRT.BluetoothService: Error while connecting (HRESULT: ERROR_ELEMENT_NOT_FOUND): " +
                        ex.Message);
                    BluetoothErrorAsync?.Invoke(this,
                        new BluetoothException(BluetoothException.ErrorCodes.ConnectFailed,
                            "SPP server on remote device unavailable. Please reboot your earbuds by placing both into the case and closing it. (ERROR_ELEMENT_NOT_FOUND)"));
                }
                catch (Exception ex) when ((uint) ex.HResult == 0x80072740) // WSAEADDRINUSE
                {
                    Log.Error("WindowsRT.BluetoothService: Address already in use");
                    BluetoothErrorAsync?.Invoke(this,
                        new BluetoothException(BluetoothException.ErrorCodes.ConnectFailed,
                            "Target address already in use. Only one app can talk to the Galaxy Buds at a time. " +
                            "Please make sure to close duplicate instances of this app and close all applications that are interacting with the proprietary RFCOMM protocol, such as Samsung's official firmware updater"));
                }
            }
            catch (Exception ex)
            {
                Log.Error("WindowsRT.BluetoothService: Unknown error while connecting: " + ex);
                BluetoothErrorAsync?.Invoke(this,
                    new BluetoothException(BluetoothException.ErrorCodes.ConnectFailed,
                        ex.Message));

            }
        }

        public async Task DisconnectAsync()
        {
            Log.Debug("WindowsRT.BluetoothService: Closing connection...");

            _loopCancellation?.Cancel();
            
            if (_writer != null)
            {
                try
                {
                    _writer.DetachStream();
                    _writer = null;
                }
                catch (Exception ex)
                {
                    Log.Error($"WindowsRT.BluetoothService: Exception while detaching writer stream: {ex}");
                }
            }

            if (_service != null)
            {
                _service.Dispose();
                _service = null;
            }
            lock (this)
            {
                if (_socket != null)
                {
                    _socket.Dispose();
                    _socket = null;
                }
            }

            IsStreamConnected = false;
            Log.Debug("WindowsRT.BluetoothService: Memory freed. Disconnected.");
            await Task.CompletedTask;
        }

        public async Task SendAsync(byte[] data)
        {
            if (!IsStreamConnected)
            {
                Log.Error("WindowsRT.BluetoothService: Cannot send message. Not connected.");
                BluetoothErrorAsync?.Invoke(this, new BluetoothException(BluetoothException.ErrorCodes.SendFailed,
                    "Stream disconnected while dispatching a message"));
                await DisconnectAsync();
                return;
            }

            try
            {
                if (_writer == null)
                {
                    Log.Warning("WindowsRT.BluetoothService: Cannot send message. Writer is NULL");
                    IsStreamConnected = false;
                    BluetoothErrorAsync?.Invoke(this, new BluetoothException(BluetoothException.ErrorCodes.SendFailed,
                        "Stream disconnected while dispatching a message"));
                    await DisconnectAsync();    
                    return;
                }

                _writer.WriteBytes(data);
                await _writer.StoreAsync();
            }
            catch (Exception ex) when ((uint) ex.HResult == 0x80072745)
            {
                // The remote device has disconnected the connection
                Log.Error("WindowsRT.BluetoothService: Remote closed connection while dispatching message");
                BluetoothErrorAsync?.Invoke(this,
                    new BluetoothException(BluetoothException.ErrorCodes.SendFailed,
                        "Remote device closed connection: " + ex.HResult.ToString() + " - " + ex.Message));
                await DisconnectAsync();
            }
            catch (Exception ex)
            {
                Log.Error("WindowsRT.BluetoothService: Error while sending: " + ex.Message);       
                BluetoothErrorAsync?.Invoke(this,                                                                  
                    new BluetoothException(BluetoothException.ErrorCodes.SendFailed,                               
                        "Remote device closed connection: " + ex.HResult.ToString() + " - " + ex.Message));  
                await DisconnectAsync();      
            }
        }

        public async Task<BluetoothDevice[]> GetDevicesAsync()
        {
            await Task.Delay(100);
            return _deviceCache.Cast<BluetoothDevice>().ToArray();
        }

        private async void BluetoothServiceLoop()
        {
            while (true)
            {
                try
                {
                    _loopCancellation.Token.ThrowIfCancellationRequested();
                    Task.Delay(100).Wait(_loopCancellation.Token);
                }
                catch (OperationCanceledException)
                {
                    IsStreamConnected = false;
                    Log.Debug("WindowsRT.BluetoothService: BluetoothServiceLoop cancelled.");
                    return;
                }
                
                try
                {  
                    Stream? incoming = null;
                    if (_socket == null)
                    {
                        Log.Error($"WindowsRT.BluetoothService: StreamSocket is null");
                        BluetoothErrorAsync?.Invoke(this,
                            new BluetoothException(BluetoothException.ErrorCodes.ReceiveFailed, 
                                "Cannot retrieve incoming data stream. Device probably disconnected."));
                        await DisconnectAsync();    
                        return;
                    }
                
                    lock (_socket)
                    {
                        incoming = _socket?.InputStream.AsStreamForRead();
                    }

                    if (incoming == null)
                    {
                        Log.Error($"WindowsRT.BluetoothService: Cannot retrieve incoming data stream");
                        BluetoothErrorAsync?.Invoke(this,
                            new BluetoothException(BluetoothException.ErrorCodes.ReceiveFailed, 
                                "Cannot retrieve incoming data stream. Device probably disconnected."));
                        await DisconnectAsync();    
                        return;
                    }
                    
                    byte[] buffer = new byte[10000];
                    
                    var b = incoming.Read(buffer, 0, buffer.Length);
                    if (b == 0)
                    {
                        // EOS
                        Log.Warning("WindowsRT.BluetoothService: End of stream. Connection closed by remote host");
                        break;
                    }

                    buffer = buffer.ShrinkTo(b);

                    NewDataAvailable?.Invoke(this, buffer);
                }
                catch (Exception ex)
                {
                    if (_socket == null)
                    {
                        switch ((uint) ex.HResult)
                        {
                            // the user closed the socket.
                            case 0x80072745:

                                BluetoothErrorAsync?.Invoke(this, new BluetoothException(
                                    BluetoothException.ErrorCodes.ReceiveFailed,
                                    "Disconnect triggered by remote device"));
                                break;
                            case 0x800703E3:
                                Log.Debug(
                                    "WindowsRT.BluetoothService: The I/O operation has been aborted because of either a thread exit or an application request");
                                break;
                        }
                    }
                    else
                    {
                        BluetoothErrorAsync?.Invoke(this, new BluetoothException(
                            BluetoothException.ErrorCodes.ReceiveFailed,
                            "Failed to read stream: " + ex.Message));
                    }

                    await DisconnectAsync();
                }
            }
        }
    }
}