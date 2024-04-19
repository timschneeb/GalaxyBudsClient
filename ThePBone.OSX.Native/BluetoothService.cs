using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using ThePBone.OSX.Native.Unmanaged;
using GalaxyBudsClient.Bluetooth;

namespace ThePBone.OSX.Native
{
    public class BluetoothService : IBluetoothService
    {
        private static readonly SemaphoreSlim ConnSemaphore = new(1, 1);
        private static readonly SemaphoreSlim SearchSemaphore = new(1, 1);

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

        private readonly unsafe BluetoothImpl* _nativePtr = null;

        private Bluetooth.Bt_OnChannelData? _onChannelData;
        private Bluetooth.Bt_OnChannelClosed? _onChannelClosed;
        private Bluetooth.BtDev_OnConnected? _onConnected;
        private Bluetooth.BtDev_OnDisconnected? _onDisconnected;

        public BluetoothService()
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
                Log.Error("OSX.BluetoothService: Unable to allocate native object. Please free some RAM up");
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
                NewDataAvailable?.Invoke(this, byteArray);
            }
        }

        private void OnChannelClosed()
        {
            Console.WriteLine("OSX.BluetoothService: Channel closed. Disconnected.");
            Disconnected?.Invoke(this, "Device lost connection");
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
                Log.Error("OSX.BluetoothService: Enumerate attempt timed out due to blocked semaphore");
                throw new BluetoothException(BluetoothException.ErrorCodes.TimedOut, "Timed out while waiting to enter enumerate phase. Another task is already enumerating.");
            }

            try
            {
                BT_ENUM_RESULT status;
                BluetoothDevice[] devices;
                unsafe
                {
                    EnumerationResult result = new();
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
                        devices = Array.Empty<BluetoothDevice>();
                    }
                }

                Log.Debug("OSX.BluetoothService: found {Count} paired devices", devices.Length);

                if (status != BT_ENUM_RESULT.BT_ENUM_SUCCESS)
                {
                    Log.Error("OSX.BluetoothService: Enumerate attempt failed due to error code {Status}", status);
                    throw new BluetoothException(BluetoothException.ErrorCodes.Unknown, "Search failed.");
                }

                return devices;
            } finally
            {
                SearchSemaphore.Release();
            }
        }

        private void OnDisconnected(IntPtr mac)
        {
            var macAddr = Marshal.PtrToStringAnsi(mac) ?? string.Empty;
            macAddr = macAddr.Replace("-", ":");
            if (string.Equals(macAddr, _currentMac, StringComparison.CurrentCultureIgnoreCase))
            {
                Disconnected?.Invoke(this, "Device was disconnected");
            }

            unsafe
            {
                Memory.mem_free(mac.ToPointer());
            }
        }

        private void OnConnected(IntPtr mac, IntPtr name)
        {
            if (ConnSemaphore.CurrentCount == 1)
            {
                var reconnect = new Action(async () =>
                {
                    try
                    {
                        await ConnectAsync(_currentMac, _currentUuid, CancellationToken.None);
                    }
                    catch (BluetoothException ex)
                    {
                        BluetoothErrorAsync?.Invoke(this, ex);
                    }
                });

                var macAddr = Marshal.PtrToStringAnsi(mac) ?? string.Empty;
                macAddr = macAddr.Replace("-", ":");
                if (string.Equals(macAddr, _currentMac, StringComparison.CurrentCultureIgnoreCase))
                {
                    Log.Debug("OSX.BluetoothService: Reconnecting to {MacAddr}", macAddr);
                    reconnect();
                }
            }

            unsafe
            {
                Memory.mem_free(mac.ToPointer());
                Memory.mem_free(name.ToPointer());
            }
        }

        #endregion

        #region Connection
        public async Task ConnectAsync(string macAddress, string uuid, CancellationToken cancelToken)
        {
            var semResult = await ConnSemaphore.WaitAsync(5000, cancelToken);
            if (semResult == false)
            {
                Log.Error("OSX.BluetoothService: Connection attempt timed out due to blocked semaphore");
                throw new BluetoothException(BluetoothException.ErrorCodes.TimedOut, "Timed out while waiting to enter connection phase. Another task is already preparing a connection.");
            }

            try {
                if (IsStreamConnected)
                {
                    Log.Debug("OSX.BluetoothService: Already connected, skipped");
                    return;
                }

                Connecting?.Invoke(this, EventArgs.Empty);
                Log.Debug("OSX.BluetoothService: Connecting...");
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

                if (result != BT_CONN_RESULT.BT_CONN_SUCCESS)
                {
                    Log.Error("OSX.BluetoothService: connect returned {Result}", result.ToString());
                    switch (result)
                    {
                        case BT_CONN_RESULT.BT_CONN_EBASECONN:
                            throw new BluetoothException(BluetoothException.ErrorCodes.ConnectFailed,
                                "Unable to connect to the Bluetooth device");
                        case BT_CONN_RESULT.BT_CONN_ENOTFOUND:
                            throw new BluetoothException(BluetoothException.ErrorCodes.ConnectFailed,
                                "Bluetooth device not found nearby. Make sure it is turned on and discoverable.");
                        case BT_CONN_RESULT.BT_CONN_ENOTPAIRED:
                            throw new BluetoothException(BluetoothException.ErrorCodes.ConnectFailed,
                                "Bluetooth device not paired to computer.");
                        case BT_CONN_RESULT.BT_CONN_ESDP:
                            throw new BluetoothException(BluetoothException.ErrorCodes.ConnectFailed,
                                "Unable to read SDP records of the Bluetooth device. It appears to be unsupported by this app.");
                        case BT_CONN_RESULT.BT_CONN_ECID:
                            throw new BluetoothException(BluetoothException.ErrorCodes.ConnectFailed,
                                "Device returned invalid Bluetooth channel id. Cannot open connection, please try again.");
                        case BT_CONN_RESULT.BT_CONN_EOPEN:
                            throw new BluetoothException(BluetoothException.ErrorCodes.ConnectFailed,
                                "Unable to open serial channel on the target device. Please make sure the device is responsive and accessible.");
                        default:
                            throw new BluetoothException(BluetoothException.ErrorCodes.Unknown,
                                "Unknown error");
                    }
                }

                unsafe
                {
                    Bluetooth.bt_register_disconnect_notification(_nativePtr, macAddress);
                }

                Log.Debug("OSX.BluetoothService: Connected");
                Connected?.Invoke(this, EventArgs.Empty);
                RfcommConnected?.Invoke(this, EventArgs.Empty);
            }
            finally
            {
                ConnSemaphore.Release();
            }
        }
        #endregion

        #region Disconnection
        public async Task DisconnectAsync()
        {
            Log.Debug("OSX.BluetoothService: Disconnecting...");
            var semResult = await ConnSemaphore.WaitAsync(5000);
            if (semResult == false)
            {
                Log.Error("OSX.BluetoothService: Disconnection attempt timed out due to blocked semaphore");
                throw new BluetoothException(BluetoothException.ErrorCodes.TimedOut, "Timed out while waiting to enter connection phase. Another task is already preparing a connection.");
            }

            try
            {
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
                    Log.Debug("Disconnection successful");
                }
            }
            finally
            {
                ConnSemaphore.Release();
            }
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

            if (result != BT_SEND_RESULT.BT_SEND_SUCCESS)
            {
                switch (result)
                {
                    case BT_SEND_RESULT.BT_SEND_EPARTIAL:
                        Log.Error(
                            "OSX.BluetoothService.SendAsync: Data has been partially sent. Data loss is very likely");
                        break;                    
                    case BT_SEND_RESULT.BT_SEND_ENULL:
                        Log.Error(
                            "OSX.BluetoothService.SendAsync: RFCOMM channel is apparently closed");
                        break;
                    case BT_SEND_RESULT.BT_SEND_EUNKNOWN:
                        Log.Error(
                            "OSX.BluetoothService.SendAsync: Non-null status value returned by native Bluetooth implementation");
                        break;
                }
                await DisconnectAsync();
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
    }
}