using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using GalaxyBudsClient.Bluetooth;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Scripting;
using GalaxyBudsClient.Utils;
using Serilog;
using Task = System.Threading.Tasks.Task;

namespace GalaxyBudsClient.Platform
{
    public class BluetoothImpl : IDisposable
    { 
        private static readonly object Padlock = new object();
        private static BluetoothImpl? _instance;
        public static BluetoothImpl Instance
        {
            get
            {
                lock (Padlock)
                {
                    return _instance ??= new BluetoothImpl();
                }
            }
        }

        public static void Reallocate()
        {
            Log.Debug("BluetoothImpl: Reallocating");
            _instance?.Dispose();
            _instance = null;
            _instance = new BluetoothImpl();
        }

        private readonly IBluetoothService _backend;
        
        public event EventHandler? Connected;
        public event EventHandler? Connecting;
        public event EventHandler<string>? Disconnected;
        public event EventHandler<SPPMessage>? MessageReceived;
        public event EventHandler<InvalidPacketException>? InvalidDataReceived;
        public event EventHandler<byte[]>? NewDataReceived;
        public event EventHandler<BluetoothException>? BluetoothError;

        public bool SuppressDisconnectionEvents { set; get; } = false;
        public Models ActiveModel => SettingsProvider.Instance.RegisteredDevice.Model;
        public IDeviceSpec DeviceSpec => DeviceSpecHelper.FindByModel(ActiveModel) ?? new StubDeviceSpec();
        public bool IsConnected => _backend.IsStreamConnected;
        
        public readonly ArrayList IncomingData = new ArrayList();
        private static readonly ConcurrentQueue<byte[]> IncomingQueue = new ConcurrentQueue<byte[]>();
        private readonly CancellationTokenSource _cancelSource;
        private Task? _loop;
        
        private Guid ServiceUuid => DeviceSpec.ServiceUuid;

        private BluetoothImpl()
        {
            try
            {
#if Windows
                if (PlatformUtils.IsWindows && SettingsProvider.Instance.UseBluetoothWinRT
                                            && PlatformUtils.IsWindowsContractsSdkSupported)
                {
                    Log.Debug("BluetoothImpl: Using WinRT.BluetoothService");
                    _backend = new Bluetooth.WindowsRT.BluetoothService();
                }
                else if (PlatformUtils.IsWindows)
                {
                    Log.Debug("BluetoothImpl: Using Windows.BluetoothService");
                    _backend = new Bluetooth.Windows.BluetoothService();
                }
#elif Linux
                if(PlatformUtils.IsLinux)

                {   
                    Log.Debug("BluetoothImpl: Using Linux.BluetoothService");
                    _backend = new Bluetooth.Linux.BluetoothService();
                }
#elif OSX
                if (PlatformUtils.IsOSX)
                {
                    Log.Debug("BluetoothImpl: Using OSX.BluetoothService");
                    _backend = new ThePBone.OSX.Native.BluetoothService();
                }
#endif
                if (_backend == null)
                {
                    Log.Warning("BluetoothImpl: Using Dummy.BluetoothService");
                    _backend = new Dummy.BluetoothService();
                }
            }
            catch (PlatformNotSupportedException)
            {
                Log.Error("BluetoothImpl: Critical error while preparing bluetooth backend");
                Log.Error("BluetoothImpl: Backend swapped out with non-functional dummy object in order to prevent crash");
                _backend = new Dummy.BluetoothService();
            }

            _cancelSource = new CancellationTokenSource();
            _loop = Task.Run(DataConsumerLoop, _cancelSource.Token);
            
            _backend.Connecting += (sender, args) => Connecting?.Invoke(this, EventArgs.Empty); 
            _backend.NewDataAvailable += OnNewDataAvailable;
            _backend.NewDataAvailable += (sender, bytes) =>  NewDataReceived?.Invoke(this, bytes);
            _backend.BluetoothErrorAsync += (sender, exception) => OnBluetoothError(exception); 
            
            _backend.RfcommConnected += (sender, args) => Task.Run(async () =>
                await Task.Delay(150).ContinueWith((_) =>
                {
                    if (RegisteredDeviceValid)
                        Connected?.Invoke(this, EventArgs.Empty);
                    else
                        Log.Error("BluetoothImpl: Suppressing Connected event, device not properly registered");
                }));
            
            _backend.Disconnected += (sender, reason) =>
            {
                if (!SuppressDisconnectionEvents)
                {
                    Disconnected?.Invoke(this, reason);
                }
            };
            
            MessageReceived += SPPMessageHandler.Instance.MessageReceiver;
        }

        public async void Dispose()
        {
            try
            {
                await _backend.DisconnectAsync();
            }
            catch (Exception ex)
            {
                Log.Warning("BluetoothImpl.Dispose: Error while disconnecting: " + ex);
            }

            MessageReceived -= SPPMessageHandler.Instance.MessageReceiver;
            
            _cancelSource.Cancel();

            await Task.Delay(50);

            try
            {
                _loop?.Dispose();
                _cancelSource.Dispose();
            }
            catch (Exception ex)
            {
                Log.Error("BluetoothImpl.Dispose: Error while disposing children: " + ex);
            }
        }
        
        private void OnBluetoothError(BluetoothException exception)
        {
            if (!SuppressDisconnectionEvents)
            {
                BluetoothError?.Invoke(this, exception);
            }
        }
        
        public async Task<BluetoothDevice[]> GetDevicesAsync()
        {
            try
            {
                return await _backend.GetDevicesAsync();
            }
            catch (BluetoothException ex)
            {
                OnBluetoothError(ex);
            }

            return new BluetoothDevice[0];
        }

        public async Task<bool> ConnectAsync(string? macAddress = null, Models? model = null, bool noRetry = false)
        {
            /* Load from configuration */
            if (macAddress == null && model == null)
            {
                if (RegisteredDeviceValid && ServiceUuid != new StubDeviceSpec().ServiceUuid)
                {
                    try
                    {
                        await _backend.ConnectAsync(SettingsProvider.Instance.RegisteredDevice.MacAddress,
                            ServiceUuid.ToString()!, noRetry);
                        return true;
                    }
                    catch (BluetoothException ex)
                    {
                        OnBluetoothError(ex);
                        return false;
                    }
                }
                else
                {
                    Log.Error("BluetoothImpl: Connection attempt without valid cached device");
                }
            }
            /* Update device registration */
            else if(macAddress != null && model != null)
            {
                if (IsDeviceValid((Models) model, macAddress))
                {
                    SettingsProvider.Instance.RegisteredDevice.Model = (Models) model;
                    SettingsProvider.Instance.RegisteredDevice.MacAddress = macAddress;

                    /* Load from configuration this time */
                    return await ConnectAsync();
                }
                else
                {
                    Log.Error("BluetoothImpl: Connection attempt without valid device");
                    return false;
                }
            }
            else
            {
                throw new ArgumentException("Either all or none arguments must be null");
            }

            return false;
        }

        public async Task DisconnectAsync()
        {
            try
            {
                await _backend.DisconnectAsync();
            }
            catch (BluetoothException ex)
            {
                OnBluetoothError(ex);
            }
        }
        
        public async Task SendAsync(SPPMessage msg)
        {
            if (!IsConnected)
            {
                // ConnectionLostPage hides error details for SendFailed, so add stack trace for the times we do need to debug this
                OnBluetoothError(new BluetoothException(BluetoothException.ErrorCodes.SendFailed, $"Attempted to send command to disconnected device: {Environment.StackTrace.Substring(700)}"));
                return;
            }
            
            try
            {
                Log.Verbose($"<< Outgoing: {msg}");
                
                foreach(var hook in ScriptManager.Instance.MessageHooks)
                {
                    hook?.OnMessageSend(ref msg);
                }

                var raw = msg.EncodeMessage();
                
                foreach(var hook in ScriptManager.Instance.RawStreamHooks)
                {
                    hook?.OnRawDataSend(ref raw);
                }
                
                await _backend.SendAsync(raw);
            }
            catch (BluetoothException ex)
            {
                OnBluetoothError(ex);
            }
        }
        
        public async Task SendResponseAsync(SPPMessage.MessageIds id, params byte[]? payload)
        {
            await SendAsync(new SPPMessage{Id = id, Payload = payload ?? new byte[0], Type = SPPMessage.MsgType.Response});
        }

        public async Task SendRequestAsync(SPPMessage.MessageIds id, params byte[]? payload)
        {
            await SendAsync(new SPPMessage{Id = id, Payload = payload ?? new byte[0], Type = SPPMessage.MsgType.Request});
        }
        
        public async Task SendRequestAsync(SPPMessage.MessageIds id, bool payload)
        {
            await SendRequestAsync(id, payload ? new byte[]{0x01} : new byte[]{0x00});
        }
        
        public void UnregisterDevice()
        {
            SettingsProvider.Instance.RegisteredDevice.Model = Models.NULL;
            SettingsProvider.Instance.RegisteredDevice.MacAddress = string.Empty;
            DeviceMessageCache.Instance.Clear();
            // don't wait for this to complete as it may confuse users if the menu option waits until connect timed out
            DisconnectAsync();
        }
        
        public bool RegisteredDeviceValid =>
            IsDeviceValid(SettingsProvider.Instance.RegisteredDevice.Model,
                SettingsProvider.Instance.RegisteredDevice.MacAddress);

        private static bool IsDeviceValid(Models model, string macAddress)
        {
            return model != Models.NULL && macAddress.Length >= 12;
        }
        
        private void OnNewDataAvailable(object? sender, byte[] frame)
        {
            /* Discard data if not properly registered */
            if (!RegisteredDeviceValid)
            {
                return;
            }

            IncomingQueue.Enqueue(frame);
        }

        private void DataConsumerLoop()
        {
            while (true)
            {
                try
                {
                    _cancelSource.Token.ThrowIfCancellationRequested();
                    Task.Delay(50).Wait(_cancelSource.Token);
                }
                catch (OperationCanceledException)
                {
                    IncomingData?.Clear();
                    throw;
                }
                
                lock (IncomingQueue)
                {
                    if (IncomingQueue.Count <= 0) continue;
                    while (IncomingQueue.TryDequeue(out var frame))
                    {
                        IncomingData.AddRange(frame);
                    }
                }
                
                var failCount = 0;
                do
                {
                    var msgSize = 0;
                    SPPMessage? msg = null;
                    try
                    {
                        var raw = IncomingData.OfType<byte>().ToArray();

                        foreach (var hook in ScriptManager.Instance.RawStreamHooks)
                        {
                            hook?.OnRawDataAvailable(ref raw);
                        }

                        msg = SPPMessage.DecodeMessage(raw);
                        msgSize = msg.TotalPacketSize;

                        Log.Verbose($">> Incoming: {msg}");

                        foreach (var hook in ScriptManager.Instance.MessageHooks)
                        {
                            hook?.OnMessageAvailable(ref msg);
                        }

                        MessageReceived?.Invoke(this, msg);
                    }
                    catch (InvalidPacketException e)
                    {
                        // Attempt to remove broken message, otherwise skip data block
                        var somIndex = 0;
                        for (var i = 1; i < IncomingData.Count; i++)
                        {
                            if ((ActiveModel == Models.Buds &&
                                 (byte)(IncomingData[i] ?? 0) == (byte)SPPMessage.Constants.SOM) ||
                                (ActiveModel != Models.Buds &&
                                 (byte)(IncomingData[i] ?? 0) == (byte)SPPMessage.Constants.SOMPlus))
                            {
                                somIndex = i;
                                break;
                            }
                        }

                        msgSize = somIndex;
                    
                        if (failCount > 5)
                        {
                            // Abandon data block
                            InvalidDataReceived?.Invoke(this, e);
                            break;
                        }
                    
                        failCount++;
                    }

                    if (msgSize >= IncomingData.Count)
                    {
                        IncomingData.Clear();
                        break;
                    }

                    IncomingData.RemoveRange(0, msgSize);

                    if (ByteArrayUtils.IsBufferZeroedOut(IncomingData))
                    {
                        /* No more data remaining */
                        break;
                    }

                } while (IncomingData.Count > 0);
            }
        }
    }
}