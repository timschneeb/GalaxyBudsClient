using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using GalaxyBudsClient.Bluetooth;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Scripting;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;
using Serilog;
using Task = System.Threading.Tasks.Task;

namespace GalaxyBudsClient.Platform;

public sealed class BluetoothService : IDisposable, INotifyPropertyChanged
{ 
    private static readonly object Padlock = new();
    private static BluetoothService? _instance;
    public static BluetoothService Instance
    {
        get
        {
            lock (Padlock)
            {
                return _instance ??= new BluetoothService();
            }
        }
    }

    public static void Reallocate()
    {
        Log.Debug("BluetoothImpl: Reallocating");
        _instance?.Dispose();
        _instance = null;
        _instance = new BluetoothService();
    }

    private readonly IBluetoothService _backend;
        
    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? Connected;
    public event EventHandler? Connecting;
    public event EventHandler<string>? Disconnected;
    public event EventHandler<SppMessage>? MessageReceived;
    public event EventHandler<InvalidPacketException>? InvalidDataReceived;
    public event EventHandler<byte[]>? NewDataReceived;
    public event EventHandler<BluetoothException>? BluetoothError;

    public bool SuppressDisconnectionEvents { set; get; } = false;
    public bool ShowDummyDevices { set; get; } = false;
    public static Models ActiveModel => Settings.Instance.RegisteredDevice.Model;
    public IDeviceSpec DeviceSpec => DeviceSpecHelper.FindByModel(ActiveModel) ?? new StubDeviceSpec();

    public string DeviceName
    {
        private set => RaiseAndSet(ref _deviceName, value);
        get => _deviceName;
    }

    public bool IsConnected
    {
        private set => RaiseAndSet(ref _isConnected, value);
        get => _isConnected;
    }

    public string LastErrorMessage
    {
        set => RaiseAndSet(ref _lastErrorMessage, value);
        get => _lastErrorMessage;
    }

    public bool IsConnectedLegacy => _backend.IsStreamConnected;

    private readonly ArrayList _incomingData = [];
    private static readonly ConcurrentQueue<byte[]> IncomingQueue = new();
    private readonly CancellationTokenSource _cancelSource;
    private readonly Task? _loop;
        
    private string _deviceName = "Galaxy Buds";
    private bool _isConnected;
    private string _lastErrorMessage = string.Empty;

    private Guid ServiceUuid => DeviceSpec.ServiceUuid;

    private BluetoothService()
    {
        try
        {
            // We don't want to initialize the backend in design mode. It would conflict with the actual application.
            if (!Design.IsDesignMode)
            {
#if Windows
                if (PlatformUtils.IsWindows && Settings.Instance.UseBluetoothWinRT
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
                if (PlatformUtils.IsLinux)

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
            }

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
            
        _backend.Connecting += (_, _) => Connecting?.Invoke(this, EventArgs.Empty); 
        _backend.NewDataAvailable += OnNewDataAvailable;
        _backend.NewDataAvailable += (_, bytes) =>  NewDataReceived?.Invoke(this, bytes);
        _backend.BluetoothErrorAsync += (_, exception) => OnBluetoothError(exception); 
            
        _backend.RfcommConnected += (_, _) => Task.Run(async () =>
            await Task.Delay(150).ContinueWith((_) =>
            {
                if (RegisteredDeviceValid)
                {
                    Connected?.Invoke(this, EventArgs.Empty);
                    IsConnected = true;
                }
                else
                    Log.Error("BluetoothImpl: Suppressing Connected event, device not properly registered");
            }));
            
        _backend.Disconnected += (_, reason) =>
        {
            if (!SuppressDisconnectionEvents)
            {
                LastErrorMessage = Loc.Resolve("connlost");
                IsConnected = false;
                Disconnected?.Invoke(this, reason);
            }
        };
            
        MessageReceived += SppMessageHandler.Instance.MessageReceiver;
        InvalidDataReceived += OnInvalidDataReceived;
    }

    public async void Dispose()
    {
        try
        {
            await _backend.DisconnectAsync();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "BluetoothImpl.Dispose: Error while disconnecting");
        }

        MessageReceived -= SppMessageHandler.Instance.MessageReceiver;
            
        await _cancelSource.CancelAsync();
        await Task.Delay(50);

        try
        {
            _loop?.Dispose();
            _cancelSource.Dispose();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "BluetoothImpl.Dispose: Error while disposing children");
        }
    }
    
    private void OnInvalidDataReceived(object? sender, InvalidPacketException e)
    {
        LastErrorMessage = e.ErrorCode.GetDescription();
        if (IsConnected)
        {
            _ = DisconnectAsync()
                .ContinueWith(_ => Task.Delay(500))
                .ContinueWith(_ => ConnectAsync());
        }
    }
        
    private void OnBluetoothError(BluetoothException exception)
    {
        if (!SuppressDisconnectionEvents)
        {
            LastErrorMessage = exception.ErrorMessage ?? exception.Message;
            IsConnected = false;
            BluetoothError?.Invoke(this, exception);
        }
    }
        
    public async Task<IEnumerable<BluetoothDevice>> GetDevicesAsync()
    {
        try
        {
            if (ShowDummyDevices)
            {
                return (await _backend.GetDevicesAsync()).Concat(BluetoothDevice.DummyDevices());
            }

            return await _backend.GetDevicesAsync();
        }
        catch (BluetoothException ex)
        {
            OnBluetoothError(ex);
        }

        return Array.Empty<BluetoothDevice>();
    }

    private async Task<string> GetDeviceNameAsync()
    {
        var fallbackName = ActiveModel.GetModelMetadata()?.Name ?? Loc.Resolve("unknown");
        try
        {
            var devices = await _backend.GetDevicesAsync();
            var device = devices.FirstOrDefault(d => d.Address == Settings.Instance.RegisteredDevice.MacAddress);
            return device?.Name ?? fallbackName;
        }
        catch (BluetoothException ex)
        {
            Log.Error(ex, "BluetoothImpl.GetDeviceName: Error while fetching device name");
            return fallbackName;
        }
    }
        
    public async Task<bool> ConnectAsync(bool noRetry = false)
    {
        /* Load from configuration */
        if (RegisteredDeviceValid && ServiceUuid != new StubDeviceSpec().ServiceUuid)
        {
            try
            {
                DeviceName = await GetDeviceNameAsync();
                        
                await _backend.ConnectAsync(Settings.Instance.RegisteredDevice.MacAddress,
                    ServiceUuid.ToString()!, noRetry);
                return true;
            }
            catch (BluetoothException ex)
            {
                OnBluetoothError(ex);
                return false;
            }
        }
        
        Log.Error("BluetoothImpl: Connection attempt without valid device");
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
        
    public async Task SendAsync(SppMessage msg)
    {
        if (!IsConnectedLegacy)
            return;
            
        try
        {
            Log.Verbose("<< Outgoing: {Msg}", msg);
                
            foreach(var hook in ScriptManager.Instance.MessageHooks)
            {
                hook.OnMessageSend(ref msg);
            }

            var raw = msg.EncodeMessage();
                
            foreach(var hook in ScriptManager.Instance.RawStreamHooks)
            {
                hook.OnRawDataSend(ref raw);
            }
                
            await _backend.SendAsync(raw);
        }
        catch (BluetoothException ex)
        {
            OnBluetoothError(ex);
        }
    }
        
    public async Task SendResponseAsync(SppMessage.MessageIds id, params byte[]? payload)
    {
        await SendAsync(new SppMessage{Id = id, Payload = payload ?? Array.Empty<byte>(), Type = SppMessage.MsgType.Response});
    }

    public async Task SendRequestAsync(SppMessage.MessageIds id, params byte[]? payload)
    {
        await SendAsync(new SppMessage{Id = id, Payload = payload ?? Array.Empty<byte>(), Type = SppMessage.MsgType.Request});
    }
        
    public async Task SendRequestAsync(SppMessage.MessageIds id, bool payload)
    {
        await SendRequestAsync(id, payload ? [0x01] : [0x00]);
    }
        
    public void UnregisterDevice()
    {
        Settings.Instance.RegisteredDevice.Model = Models.NULL;
        Settings.Instance.RegisteredDevice.MacAddress = string.Empty;
        DeviceMessageCache.Instance.Clear();
        // don't wait for this to complete as it may confuse users if the menu option waits until connect timed out
        _ = DisconnectAsync();
    }
        
    public static bool RegisteredDeviceValid =>
        IsDeviceValid(Settings.Instance.RegisteredDevice.Model,
            Settings.Instance.RegisteredDevice.MacAddress);
        
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

        IsConnected = true;
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
                _incomingData.Clear();
                throw;
            }
                
            lock (IncomingQueue)
            {
                if (IncomingQueue.IsEmpty) continue;
                while (IncomingQueue.TryDequeue(out var frame))
                {
                    _incomingData.AddRange(frame);
                }
            }
                
            var failCount = 0;
            do
            {
                var msgSize = 0;
                try
                {
                    var raw = _incomingData.OfType<byte>().ToArray();

                    foreach (var hook in ScriptManager.Instance.RawStreamHooks)
                    {
                        hook.OnRawDataAvailable(ref raw);
                    }

                    var msg = SppMessage.DecodeMessage(raw);
                    msgSize = msg.TotalPacketSize;

                    Log.Verbose(">> Incoming: {Msg}", msg);

                    foreach (var hook in ScriptManager.Instance.MessageHooks)
                    {
                        hook.OnMessageAvailable(ref msg);
                    }

                    MessageReceived?.Invoke(this, msg);
                }
                catch (InvalidPacketException e)
                {
                    // Attempt to remove broken message, otherwise skip data block
                    var somIndex = 0;
                    for (var i = 1; i < _incomingData.Count; i++)
                    {
                        if ((ActiveModel == Models.Buds &&
                             (byte)(_incomingData[i] ?? 0) == (byte)SppMessage.Constants.SOM) ||
                            (ActiveModel != Models.Buds &&
                             (byte)(_incomingData[i] ?? 0) == (byte)SppMessage.Constants.SOMPlus))
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

                if (msgSize >= _incomingData.Count)
                {
                    _incomingData.Clear();
                    break;
                }

                _incomingData.RemoveRange(0, msgSize);

                if (ByteArrayUtils.IsBufferZeroedOut(_incomingData))
                {
                    /* No more data remaining */
                    break;
                }

            } while (_incomingData.Count > 0);
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool RaiseAndSet<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}