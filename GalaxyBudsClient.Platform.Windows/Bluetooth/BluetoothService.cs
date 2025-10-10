using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GalaxyBudsClient.Platform.Windows.Devices;
using GalaxyBudsClient.Platform.Windows.Utils;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Platform.Interfaces;
using GalaxyBudsClient.Platform.Model;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Bluetooth.Msft;
using InTheHand.Net.Sockets;
using Serilog;

namespace GalaxyBudsClient.Platform.Windows.Bluetooth
{
    public class BluetoothService : IBluetoothService
    {
        private static readonly ConcurrentQueue<byte[]> TransmitterQueue = new ConcurrentQueue<byte[]>();

        private CancellationTokenSource _cancelSource = new CancellationTokenSource();
        private Task? _loop;

        private BluetoothClient? _client;
        private readonly object _btlock = new object();
        static readonly SemaphoreSlim _connSemaphore = new SemaphoreSlim(1, 1);
        
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

        public bool IsStreamConnected => _client?.Connected ?? false;

        public BluetoothService()
        {
            try
            {
                if (!WindowsBluetoothRadio.IsPlatformSupported)
                {
                    Log.Error("Windows.BluetoothService: Microsoft Bluetooth Stack unavailable");
                    throw new PlatformNotSupportedException("Microsoft Bluetooth stack not available");
                }
            }
            catch (Exception ex)
            {
                throw new PlatformNotSupportedException($"Error while initializing legacy Bluetooth backend for older Windows 10 versions: {ex.Message}");
            }

            Task.Delay(2000).ContinueWith(x => SetupDeviceDetection());
        }

        #region Detection
        private void SetupDeviceDetection()
        {
            Log.Debug("Windows.BluetoothService: Setting device detection up");
            Task.Factory.StartNew(Win32DeviceChangeListener.Init);
            
            try
            {
                Win32DeviceChangeListener.Instance.DeviceInRange += DeviceInRange;
            }
            catch (Win32Exception ex)
            {
                Log.Error("Windows.BluetoothService.Win32DeviceChangeListener: Win32Exception: {ExMessage} (Win32ErrorCode = {ExNativeErrorCode}; HRESULT = {ExErrorCode})", ex.Message, ex.NativeErrorCode, ex.ErrorCode);
            }
            catch (InvalidOperationException)
            {
                /* Listener will stay disabled */
                Log.Error($"Windows.BluetoothService.Win32DeviceChangeListener: No adapter on the Microsoft Bluetooth stack available");
            }
            catch (ArgumentException ex)
            {
                /* Listener will stay disabled */
                Log.Error("Windows.BluetoothService.Win32DeviceChangeListener: Invalid argument: {ExMessage}", ex.Message);
                
            }
        }
        
        private async void DeviceInRange(object? sender, BluetoothWin32RadioInRangeEventArgs? e)
        {
            if(e?.Device == null)
            {
                Log.Warning("Windows.BluetoothService: Discovered NULL device");
                return;
            }

            if(e.Device.DeviceAddress == MacUtils.ToAddress(_currentMac))
            {
                Log.Debug("Windows.BluetoothService: Target device inbound ({CurrentMac})", _currentMac);
                if (IsStreamConnected)
                {
                    Log.Debug($"Windows.BluetoothService: Target device already connected");
                }
                else
                {
                    try
                    {
                        await ConnectAsync(_currentMac, _currentUuid, CancellationToken.None);
                    }
                    catch (InvalidOperationException ex)
                    {
                        BluetoothErrorAsync?.Invoke(this, new BluetoothException(BluetoothException.ErrorCodes.Unknown, ex.Message));
                        Log.Error("Windows.BluetoothService: DeviceInRange: InvalidOperationException caught ({ExMessage})", ex.Message);
                    }
                    catch (BluetoothException ex)
                    {
                        BluetoothErrorAsync?.Invoke(this, ex);
                        Log.Error("Windows.BluetoothService: DeviceInRange: BluetoothException caught ({ExMessage})", ex.Message);
                        
                    }

                }
            }
        }
        #endregion

        #region Adapter
        public void SelectAdapter()
        {
            try
            {
                _client = new BluetoothClient();
            }
            catch (PlatformNotSupportedException)
            {
                BluetoothErrorAsync?.Invoke(this, new BluetoothException(BluetoothException.ErrorCodes.NoAdaptersAvailable));
                return;
            }

            Log.Debug($"Windows.BluetoothService: Bluetooth adapter found");
        }
        #endregion

        #region Connection
        public async Task ConnectAsync(string macAddress, string uuid, CancellationToken cancelToken)
        {
            var semResult = await _connSemaphore.WaitAsync(5000, cancelToken);
            if (semResult == false)
            {
                Log.Error($"Windows.BluetoothService: Connection attempt timed out due to blocked semaphore");
                throw new BluetoothException(BluetoothException.ErrorCodes.TimedOut, "Timed out while waiting to enter connection phase. Another task is already preparing a connection.");
            }

            if (_client?.Connected ?? false)
            {
                Log.Debug("Windows.BluetoothService: Already connected, skipped");
                _connSemaphore.Release();
                return;
            }

            SelectAdapter();
            
            Connecting?.Invoke(this, EventArgs.Empty);
            Log.Debug($"Windows.BluetoothService: Connecting...");

            _cancelSource?.Cancel();
            _client?.Close();
            _client = null;
            SelectAdapter();

            if (_client == null)
            {
                Log.Error("Windows.BluetoothService: Cannot create client and connect");
                BluetoothErrorAsync?.Invoke(this, new BluetoothException(BluetoothException.ErrorCodes.Unknown, "Cannot create client"));
                _connSemaphore.Release();
                return;
            }

            try
            {
                try
                {
                    var addr = MacUtils.ToAddress(macAddress);
                    if (addr == null)
                    {
                        Log.Error("Windows.BluetoothService: Invalid MAC address. Failed to connect");
                        throw new BluetoothException(BluetoothException.ErrorCodes.ConnectFailed, "Invalid MAC address. Please deregister your device and try again.");
                    }

                    _currentMac = macAddress;
                    _currentUuid = uuid;

                    _client.Connect(addr, new Guid(uuid));
                    
                    /*await  Task.Factory.FromAsync(
                        (callback, stateObject) => _client.BeginConnect(addr, new Guid(uuid), callback, stateObject),
                        _client.EndConnect, null);*/
                }
                catch (ArgumentException)
                {
                    BluetoothErrorAsync?.Invoke(this, new BluetoothException(
                        BluetoothException.ErrorCodes.ConnectFailed,
                        $"Invalid MAC address. Please unregister your device and try again."));
                    _connSemaphore.Release();
                    return;
                }

                Connected?.Invoke(this, EventArgs.Empty);
                Log.Debug($"Windows.BluetoothService: Connected. Launching BluetoothServiceLoop.");

                _cancelSource = new CancellationTokenSource();
                _loop = Task.Run(BluetoothServiceLoop, _cancelSource.Token);
            }
            catch (SocketException e)
            {
                BluetoothErrorAsync?.Invoke(this,
                    new BluetoothException(BluetoothException.ErrorCodes.ConnectFailed, e.Message));
            }
            finally
            {
                _connSemaphore.Release();
            }
        }
        #endregion

        #region Disconnection
        public async Task DisconnectAsync()
        {
            Log.Debug("Windows.BluetoothService: Disconnecting...");
            if (_loop == null || _loop.Status == TaskStatus.Created)
            {
                Log.Debug("Windows.BluetoothService: BluetoothServiceLoop not yet launched. No need to cancel");
            }
            else
            {
                Log.Debug("Windows.BluetoothService: Cancelling BluetoothServiceLoop...");
                _cancelSource.Cancel();
            }

            /* Detach device if not already done... */
            _client?.Close();
            _client = null;
            
            await Task.CompletedTask;
        }
        #endregion

        #region Transmission
        public async Task SendAsync(byte[] data)
        {
            lock (TransmitterQueue)
            {
                TransmitterQueue.Enqueue(data);
            }
            await Task.CompletedTask;
        }
        #endregion

        #region Enumeration
        public async Task<BluetoothDevice[]> GetDevicesAsync()
        {
            if (_client == null)
            {
                SelectAdapter();
            }

            if (_client == null)
            {
                Log.Error("Windows.BluetoothService: Cannot create client and get devices");
                return Array.Empty<BluetoothDevice>();
            }

            var devs = await Task.Factory.FromAsync((callback, stateObject) => _client.BeginDiscoverDevices(20, true, true, false, false, callback, stateObject), _client.EndDiscoverDevices, null);
            if (devs == null)
            {
                return Array.Empty<BluetoothDevice>();
            }
            
            var devices = new BluetoothDevice[devs.Length];
            for (var i = 0; i < devs.Length; i++)
            {
                var formattedMac = Regex.Replace(devs[i].DeviceAddress.ToString(), ".{2}", "$0:").TrimEnd(':');
                devices[i] = new BluetoothDevice(devs[i].DeviceName, formattedMac,
                    devs[i].Connected, true, new BluetoothCoD(devs[i].ClassOfDevice.Value), devs[i].InstalledServices);
            }

            return devices;
        }
        #endregion

        #region Service
        private void BluetoothServiceLoop()
        {
            Stream? peerStream = null;
            DateTime lastActivity = DateTime.Now;
            const int connectionTimeoutMs = 5000; // 5 seconds timeout

            while (true)
            {
                try
                {
                    _cancelSource.Token.ThrowIfCancellationRequested();
                    Task.Delay(50).Wait(_cancelSource.Token);
                }
                catch (OperationCanceledException)
                {
                    peerStream?.Close();
                    _client?.Close();
                    throw;
                }
                
                // Enhanced disconnection detection
                if (_client is not { Connected: true })
                {
                    if (peerStream != null)
                    {
                        Log.Debug("Windows.BluetoothService: Client disconnected, triggering disconnection event");
                        peerStream?.Close();
                        peerStream = null;
                        BluetoothErrorAsync?.Invoke(this, new BluetoothException(BluetoothException.ErrorCodes.ConnectFailed, "Connection lost"));
                    }
                    continue;
                }

                // Check for connection timeout (no activity for too long)
                if (peerStream != null && (DateTime.Now - lastActivity).TotalMilliseconds > connectionTimeoutMs)
                {
                    try
                    {
                        // Try to detect if connection is still alive by checking stream properties
                        if (!peerStream.CanRead || !peerStream.CanWrite)
                        {
                            Log.Debug("Windows.BluetoothService: Stream became unreadable/unwritable, connection lost");
                            peerStream?.Close();
                            peerStream = null;
                            BluetoothErrorAsync?.Invoke(this, new BluetoothException(BluetoothException.ErrorCodes.ConnectFailed, "Stream became unavailable"));
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Debug("Windows.BluetoothService: Exception while checking stream status: {Message}", ex.Message);
                        peerStream?.Close();
                        peerStream = null;
                        BluetoothErrorAsync?.Invoke(this, new BluetoothException(BluetoothException.ErrorCodes.ConnectFailed, "Stream check failed"));
                        continue;
                    }
                }

                if (peerStream == null)
                {
                    lock (_btlock)
                    {
                        if (!_client.Connected)
                        {
                            continue;
                        }

                        peerStream = _client.GetStream();
                    }

                    RfcommConnected?.Invoke(this, EventArgs.Empty);
                    continue;
                }


                var available = _client.Available;
                if (available > 0 && peerStream.CanRead)
                {
                    var buffer = new byte[available];
                    try
                    {
                        peerStream.Read(buffer, 0, available);
                        lastActivity = DateTime.Now; // Update activity timestamp on successful read
                    }
                    catch (SocketException ex)
                    {
                        Log.Error("Windows.BluetoothService: BluetoothServiceLoop: SocketException thrown while reading from socket: {ExMessage}. Cancelled", ex.Message);
                        BluetoothErrorAsync?.Invoke(this, new BluetoothException(BluetoothException.ErrorCodes.ReceiveFailed, ex.Message));
                        return;
                    }
                    catch (IOException ex)
                    {
                        Log.Error("Windows.BluetoothService: BluetoothServiceLoop: IOException thrown while writing to socket: {ExMessage}. Cancelled", ex.Message);
                        BluetoothErrorAsync?.Invoke(this, new BluetoothException(BluetoothException.ErrorCodes.ReceiveFailed, ex.Message));
                    }

                    if (buffer.Length > 0)
                    {
                        NewDataAvailable?.Invoke(this, buffer);
                    }
                }

                lock (TransmitterQueue)
                {
                    if (TransmitterQueue.Count <= 0) continue;
                    if (!TransmitterQueue.TryDequeue(out var raw)) continue;
                    try
                    {
                        peerStream.Write(raw, 0, raw.Length);
                        lastActivity = DateTime.Now; // Update activity timestamp on successful write
                    }
                    catch (SocketException ex)
                    {
                        Log.Error("Windows.BluetoothService: BluetoothServiceLoop: SocketException thrown while writing to socket: {ExMessage}. Cancelled", ex.Message);
                        BluetoothErrorAsync?.Invoke(this, new BluetoothException(BluetoothException.ErrorCodes.SendFailed, ex.Message));
                    }
                    catch (IOException ex)
                    {
                        Log.Error("Windows.BluetoothService: BluetoothServiceLoop: IOException thrown while writing to socket: {ExMessage}. Cancelled", ex.Message);
                        BluetoothErrorAsync?.Invoke(this, new BluetoothException(BluetoothException.ErrorCodes.SendFailed, ex.Message));
                    }
                }
            }
        }
        #endregion
    }
}