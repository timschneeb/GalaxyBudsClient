using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using ThePBone.OSX.Native.Unmanaged;
using GalaxyBudsClient.Bluetooth;
using System.Net.Mail;
using System.Security.Cryptography;

namespace ThePBone.OSX.Native
{
    public class BluetoothService : IBluetoothService
    {
        private static readonly SemaphoreSlim ConnSemaphore = new SemaphoreSlim(1, 1);
        private static readonly SemaphoreSlim SearchSemaphore = new SemaphoreSlim(1, 1);

        private string _currentMac = string.Empty;
        private string _currentUuid = string.Empty;

        public event EventHandler? RfcommConnected;
        public event EventHandler? Connecting;
        public event EventHandler? Connected;
#pragma warning disable 0067
        public event EventHandler<string>? Disconnected;
#pragma warning restore
        public event EventHandler<BluetoothException>? BluetoothErrorAsync;
        public event EventHandler<byte[]>? NewDataAvailable;

        private unsafe BluetoothImpl* _nativePtr = null;

        private Bluetooth.Bt_OnChannelData? _onChannelData;
        private Bluetooth.Bt_OnChannelClosed? _onChannelClosed;
        private Bluetooth.BtDev_OnConnected? _onConnected;
        private Bluetooth.BtDev_OnDisconnected? _onDisconnected;

        public BluetoothService()
        {
            Allocate();
        }

        private void Allocate()
        {
            bool allocationResult;
            unsafe
            { 
                fixed (BluetoothImpl** ptr = &_nativePtr)
                {
                    allocationResult = Bluetooth.bt_alloc(ptr);
                }
            }

            if (!allocationResult)
            {
                Log.Error("OSX.BluetoothService: Unable to allocate native object. Please free some RAM up.");
                throw new BluetoothException(BluetoothException.ErrorCodes.AllocationFailed,
                    "Unable to allocate memory for native Bluetooth object. Please free some RAM up.");
            }

            unsafe
            {
                _onChannelData = OnChannelData;
                Bluetooth.bt_set_on_channel_data(_nativePtr, _onChannelData);

                _onChannelClosed = OnChannelClosed;
                Bluetooth.bt_set_on_channel_closed(_nativePtr, _onChannelClosed);
            }
            
            Log.Debug("OSX.BluetoothService: Setting device detection up");

            unsafe
            {
                _onConnected = OnConnected;
                Bluetooth.bt_set_on_connected(_nativePtr, _onConnected);

                _onDisconnected = OnDisconnected;
                Bluetooth.bt_set_on_disconnected(_nativePtr, _onDisconnected);
            }
        }

        private void OnChannelData(IntPtr data, ulong size)
        {
            unsafe
            {
                var byteArray = new Span<byte>(data.ToPointer(), (int) size).ToArray();
                Console.WriteLine($"NEW DATA PACKET size={size}; content={BitConverter.ToString(byteArray)}");

                NewDataAvailable?.Invoke(this, byteArray);
            }
        }

        private void OnChannelClosed()
        {
            Disconnected?.Invoke(this, "Bluetooth channel closed");
            Console.WriteLine("OSX.BluetoothService: Channel closed. Disconnected.");
        }
        
        public bool IsStreamConnected
        {
            get
            {
                unsafe
                {
                    return Bluetooth.bt_is_connected(_nativePtr);
                }
            }
        }

        #region Detection
        public async Task<BluetoothDevice[]> GetDevicesAsync()
        {
            var semResult = await SearchSemaphore.WaitAsync(5000);
            if (semResult == false)
            {
                Log.Error($"OSX.BluetoothService: Enumerate attempt timed out due to blocked semaphore");
                throw new BluetoothException(BluetoothException.ErrorCodes.TimedOut, "Timed out while waiting to enter enumerate phase. Another task is already enumerating.");
            }

            BT_ENUM_RESULT status;
            BluetoothDevice[] devices;
            unsafe
            {
                EnumerationResult result = new EnumerationResult();
                status = Bluetooth.bt_enumerate(_nativePtr, ref result);
                if (status == BT_ENUM_RESULT.BT_ENUM_SUCCESS)
                {
                    devices = new BluetoothDevice[result.length];
                    Device* /* Device[] */ rawDevices = (Device*)result.devices;
                    for (int i = 0; i < result.length; i++)
                    {
                        Device* d = &rawDevices[i];
                        devices[i] = new BluetoothDevice(
                            Marshal.PtrToStringUTF8(d->device_name) ?? String.Empty,
                            (Marshal.PtrToStringUTF8(d->mac_address) ?? String.Empty).Replace("-", ":"),
                            d->is_connected,
                            d->is_paired,
                            new BluetoothCoD(d->cod));
                        Memory.btdev_free(ref *d);
                    }
                    Memory.mem_free(rawDevices);
                }
                else
                {
                    devices = new BluetoothDevice[0];
                }
            }

            Log.Debug($"OSX.BluetoothService: found {devices.Length} paired devices");

            if (status != BT_ENUM_RESULT.BT_ENUM_SUCCESS)
            {
                Log.Error($"OSX.BluetoothService: Enumerate attempt failed due to error code {status.ToString()}");
                SearchSemaphore.Release();
                throw new BluetoothException(BluetoothException.ErrorCodes.Unknown, "Search failed.");
            }

            SearchSemaphore.Release();

            return devices;
        }

        private void OnDisconnected(IntPtr mac)
        {
            var macAddr = Marshal.PtrToStringAnsi(mac) ?? string.Empty;
            macAddr = macAddr.Replace("-", ":");
            Log.Information($"BOD: PARTIAL DISCONNECTION {macAddr}");
            if (string.Equals(macAddr, _currentMac, StringComparison.CurrentCultureIgnoreCase))
            {
                Log.Information("BOD: MATCH");
                Disconnected?.Invoke(this, "Bluetooth channel closed");
            }

            unsafe
            {
                Memory.mem_free(mac.ToPointer());
            }
        }

        private void OnConnected(IntPtr mac, IntPtr name)
        {
            var reconnect = new Action(async() =>
            {
                try
                {
                    await ConnectAsync(_currentMac, _currentUuid);
                }
                catch (BluetoothException ex)
                {
                    BluetoothErrorAsync?.Invoke(this, ex);
                }
            });
            
            var macAddr = Marshal.PtrToStringAnsi(mac) ?? string.Empty;
            macAddr = macAddr.Replace("-", ":");
            Log.Information($"BOC: PARTIAL CONNECTION {macAddr}");
            if (string.Equals(macAddr, _currentMac, StringComparison.CurrentCultureIgnoreCase))
            {
                Log.Information("BOC: MATCH");
                reconnect();
            }

            unsafe
            {
                Memory.mem_free(mac.ToPointer());
                Memory.mem_free(name.ToPointer());
            }
        }

        #endregion

        #region Connection
        public async Task ConnectAsync(string macAddress, string uuid, bool noRetry = false)
        {
            //TODO osx, do we need noRetry?
            var semResult = await ConnSemaphore.WaitAsync(5000);
            if (semResult == false)
            {
                Log.Error($"OSX.BluetoothService: Connection attempt timed out due to blocked semaphore");
                throw new BluetoothException(BluetoothException.ErrorCodes.TimedOut, "Timed out while waiting to enter connection phase. Another task is already preparing a connection.");
            }
            
            if (IsStreamConnected)
            {
                Log.Debug("OSX.BluetoothService: Already connected, skipped.");
                ConnSemaphore.Release();
                return;
            }

            Connecting?.Invoke(this, EventArgs.Empty);
            Log.Debug($"OSX.BluetoothService: Connecting...");
            
            _currentMac = macAddress;
            _currentUuid = uuid;

            BT_CONN_RESULT result;
            unsafe
            {
                var uuidBytes = new Guid(uuid).ToByteArray();
                uuidBytes.FixEndiannessOfGuidBytes();
                fixed (byte* rawUuid = uuidBytes)
                {
                    result = Bluetooth.bt_connect(_nativePtr, macAddress, rawUuid);
                }
            }

            switch (result)
            {
                case BT_CONN_RESULT.BT_CONN_EBASECONN:
                    throw new BluetoothException(BluetoothException.ErrorCodes.ConnectFailed, 
                        "Unable to connect to the Bluetooth device");
                case BT_CONN_RESULT.BT_CONN_ENOTFOUND:
                    throw new BluetoothException(BluetoothException.ErrorCodes.ConnectFailed, 
                        "Bluetooth device not found nearby. Make sure it is turned on and discoverable.");
                case BT_CONN_RESULT.BT_CONN_ESDP:
                    throw new BluetoothException(BluetoothException.ErrorCodes.ConnectFailed,
                        "Unable to read SDP records of the Bluetooth device. It appears to be unsupported by this app.");
                case BT_CONN_RESULT.BT_CONN_ECID:
                    throw new BluetoothException(BluetoothException.ErrorCodes.ConnectFailed,
                        "Device returned invalid Bluetooth channel id. Cannot open connection, please try again.");
                case BT_CONN_RESULT.BT_CONN_EOPEN:
                    throw new BluetoothException(BluetoothException.ErrorCodes.ConnectFailed,
                        "Unable to open serial channel on the target device. Please make sure the device is responsive and accessible.");
            }

            Log.Debug($"OSX.BluetoothService: Registering disconnection notification for connected device");
            unsafe
            {
                Bluetooth.bt_register_disconnect_notification(_nativePtr, macAddress);
            }
            
            Connected?.Invoke(this, EventArgs.Empty);
            RfcommConnected?.Invoke(this, EventArgs.Empty);
            Log.Debug($"OSX.BluetoothService: Connected.");

            ConnSemaphore.Release();
        }
        #endregion

        #region Disconnection
        public async Task DisconnectAsync()
        {
            Log.Debug("OSX.BluetoothService: Disconnecting...");
            bool result;
            unsafe
            {
                result = Bluetooth.bt_disconnect(_nativePtr);
            }

            if (!result)
            {
                Log.Error("Disconnecting failed");
            }
            else
            {
                Log.Error("Disconnection successful");
            }

            await Task.CompletedTask;
        }
        #endregion

        #region Transmission
        public async Task SendAsync(byte[] data)
        {
            BT_SEND_RESULT result;
            unsafe
            {
                fixed (byte* raw = data)
                {
                    result = Bluetooth.bt_send(_nativePtr, raw, (uint)data.Length);
                }
            }

            switch (result)
            {
                case BT_SEND_RESULT.BT_SEND_EPARTIAL:
                    Log.Error("OSX.BluetoothService.SendAsync: Data has been partially sent. Data loss is very likely.");
                    break;
                case BT_SEND_RESULT.BT_SEND_EUNKNOWN:
                    Log.Error("OSX.BluetoothService.SendAsync: Non-null status value returned by native Bluetooth implementation");
                    break;
                case BT_SEND_RESULT.BT_SEND_ENULL:
                    Log.Error("OSX.BluetoothService.SendAsync: Native object not properly allocated");
                    break;
            }
            
            await Task.CompletedTask;
        }
        #endregion
        
        public void Dispose()
        {
            unsafe
            {
                Bluetooth.bt_free(_nativePtr);
            }
        }
        
        
        
           /*
            The C interop code below is now mostly wrapped into the BluetoothService class. I'll temporarily leave it for reference here.
            
            IntPtr iptr;
            unsafe
            {
                BluetoothImpl* ptr = null;
                
                var allocResult = Bluetooth.bt_alloc(&ptr);
                iptr = (IntPtr)ptr;
                Console.WriteLine(allocResult);
                
                Bluetooth.bt_set_on_channel_data(ptr, (data, size) =>
                {
                    var byteArray = new Span<byte>(data.ToPointer(), (int)size).ToArray();
                    Console.WriteLine($"NEW DATA PACKET size={size}; content={BitConverter.ToString(byteArray)}");
                });
                
                Bluetooth.bt_set_on_channel_closed(ptr, () =>
                {
                    Console.WriteLine("CHANNEL CLOSED!");
                });
                
                fixed (byte* sUuid = standardUuid)
                {
                    var conn_result = Bluetooth.bt_connect(ptr, "80:7b:3e:21:79:ec", sUuid);
                    Console.WriteLine($"Connection result: {conn_result}");
                }

                
                
                Bluetooth.bt_set_on_connected(ptr, (mac, name) =>
                {
                    Console.WriteLine("OKAY!");
                    Console.WriteLine($"{Marshal.PtrToStringAnsi(mac)}, {Marshal.PtrToStringAnsi(name)}");
                    Memory.mem_free(mac.ToPointer());
                    Memory.mem_free(name.ToPointer());
                });
                
                
                           
                unsafe
                {
                    fixed (byte* b = fmgStart)
                        Bluetooth.bt_send((BluetoothImpl*) iptr, b, (uint) fmgStart.Length);
                }

                await Task.Delay(5000);
                
                unsafe
                {
                    fixed (byte* b = fmgStop)
                        Bluetooth.bt_send((BluetoothImpl*) iptr, b, (uint) fmgStop.Length);
                }

                
                Bluetooth.bt_free(ptr);

                var result = new Device();
                var arrayPtr = new byte*[2];
                fixed (byte* sUuid = standardUuid, lUuid = legacyUuid)
                {
                    arrayPtr[0] = sUuid;
                    arrayPtr[1] = lUuid;
                    fixed (byte** uuids = arrayPtr)
                    {
                        Unmanaged.SystemDialogs.ui_select_bt_device(uuids, 2, ref result);
                    }
                }
                
                Console.WriteLine(result.device_name);
                Console.WriteLine(result.mac_address);
                Console.WriteLine(result.is_connected);
                Console.WriteLine(result.is_paired);
                
                Memory.btdev_free(ref result);
            }*/
    }
}