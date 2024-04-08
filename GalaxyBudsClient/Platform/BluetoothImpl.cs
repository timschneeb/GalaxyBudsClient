using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using GalaxyBudsClient.Bluetooth;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Encoder;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Scripting;
using GalaxyBudsClient.Utils;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;
using Task = System.Threading.Tasks.Task;

namespace GalaxyBudsClient.Platform;

public sealed class BluetoothImpl : ReactiveObject, IDisposable
{ 
    private static readonly object Padlock = new();
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
    public event EventHandler<SppMessage>? MessageReceived;
    public event EventHandler<InvalidPacketException>? InvalidDataReceived;
    public event EventHandler<byte[]>? NewDataReceived;
    public event EventHandler<BluetoothException>? BluetoothError;
    
    public static Models ActiveModel => Settings.Instance.DeviceLegacy.Model;
    public IDeviceSpec DeviceSpec => DeviceSpecHelper.FindByModel(ActiveModel) ?? new StubDeviceSpec();
    public static bool IsRegisteredDeviceValid => Settings.Instance.DeviceLegacy.Model != Models.NULL && 
                                                  Settings.Instance.DeviceLegacy.MacAddress.Length >= 12;
    
    [Reactive] public string DeviceName { private set; get; } = "Galaxy Buds";
    [Reactive] public bool IsConnected { private set; get; }
    [Reactive] public string LastErrorMessage { private set; get; } = string.Empty;
    [Reactive] public bool SuppressDisconnectionEvents { set; get; }
    [Reactive] public bool ShowDummyDevices { set; get; }
    
    [Obsolete("Use new IsConnected instead")]
    public bool IsConnectedLegacy => _backend.IsStreamConnected;
    
    private readonly List<byte> _incomingData = [];
    private static readonly ConcurrentQueue<byte[]> IncomingQueue = new();
    private readonly CancellationTokenSource _cancelSource;
    private readonly Task? _loop;

    private BluetoothImpl()
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
        }
        catch (PlatformNotSupportedException)
        {
            Log.Error("BluetoothImpl: Critical error while preparing bluetooth backend");
        }

        if (_backend == null)
        {
            Log.Warning("BluetoothImpl: Using Dummy.BluetoothService");
            _backend = new Dummy.BluetoothService();
        }
        
        _cancelSource = new CancellationTokenSource();
        _loop = Task.Run(DataConsumerLoop, _cancelSource.Token);
            
        _backend.Connecting += (_, _) => Connecting?.Invoke(this, EventArgs.Empty);
        _backend.BluetoothErrorAsync += (_, exception) => OnBluetoothError(exception); 
        _backend.NewDataAvailable += OnNewDataAvailable;
        _backend.RfcommConnected += OnRfcommConnected;
        _backend.Disconnected += OnDisconnected;
            
        MessageReceived += SppMessageReceiver.Instance.MessageReceiver;
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

        MessageReceived -= SppMessageReceiver.Instance.MessageReceiver;
            
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
        LastErrorMessage = e.ErrorCode.ToString();
        if (IsConnected)
        {
            _ = DisconnectAsync()
                .ContinueWith(_ => Task.Delay(500))
                .ContinueWith(_ => ConnectAsync());
        }
    }
        
    private void OnBluetoothError(BluetoothException exception)
    {
        if (SuppressDisconnectionEvents) 
            return;
        
        LastErrorMessage = exception.ErrorMessage ?? exception.Message;
        IsConnected = false;
        BluetoothError?.Invoke(this, exception);
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
        var fallbackName = ActiveModel.GetModelMetadataAttribute()?.Name ?? Strings.Unknown;
        try
        {
            var devices = await _backend.GetDevicesAsync();
            var device = devices.FirstOrDefault(d => d.Address == Settings.Instance.DeviceLegacy.MacAddress);
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
        if (!IsRegisteredDeviceValid)
        {
            Log.Error("BluetoothImpl: Connection attempt without valid device");
            return false;
        }
        
        /* Load from configuration */
        try
        {
            DeviceName = await GetDeviceNameAsync();
            Settings.Instance.DeviceLegacy.Name = DeviceName;
                        
            await _backend.ConnectAsync(Settings.Instance.DeviceLegacy.MacAddress,
                DeviceSpec.ServiceUuid.ToString(), noRetry);
            return true;
        }
        catch (BluetoothException ex)
        {
            OnBluetoothError(ex);
            return false;
        }
    }

    public async Task DisconnectAsync()
    {
        try
        {
            await _backend.DisconnectAsync();
            Disconnected?.Invoke(this, "User requested disconnect");
            LastErrorMessage = string.Empty;
        }
        catch (BluetoothException ex)
        {
            OnBluetoothError(ex);
        }
    }
        
    public async Task SendAsync(SppMessage msg)
    {
        if (!IsConnected)
            return;
            
        try
        {
            Log.Verbose("<< Outgoing: {Msg}", msg);
                
            foreach(var hook in ScriptManager.Instance.MessageHooks)
            {
                hook.OnMessageSend(ref msg);
            }

            var raw = msg.Encode();
                
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
        
    public async Task SendResponseAsync(MsgIds id, params byte[]? payload)
    {
        await SendAsync(new SppMessage{Id = id, Payload = payload ?? Array.Empty<byte>(), Type = MsgTypes.Response});
    }

    public async Task SendRequestAsync(MsgIds id, params byte[]? payload)
    {
        await SendAsync(new SppMessage{Id = id, Payload = payload ?? Array.Empty<byte>(), Type = MsgTypes.Request});
    }
        
    public async Task SendRequestAsync(MsgIds id, bool payload)
    {
        await SendRequestAsync(id, payload ? [0x01] : [0x00]);
    }
    
    public async Task SendAsync(BaseMessageEncoder encoder)
    {
        await SendAsync(encoder.Encode());
    }
        
    public void UnregisterDevice()
    {
        Settings.Instance.DeviceLegacy.Model = Models.NULL;
        Settings.Instance.DeviceLegacy.MacAddress = string.Empty;
        Settings.Instance.DeviceLegacy.Name = string.Empty;
        Settings.Instance.DeviceLegacy.DeviceColor = null;
        DeviceMessageCache.Instance.Clear();
        // Don't wait for this to complete as it may confuse users if the menu option waits until connect timed out
        _ = DisconnectAsync();
    }
    
    private void OnDisconnected(object? sender, string reason)
    {
        if (!SuppressDisconnectionEvents)
        {
            LastErrorMessage = Strings.Connlost;
            IsConnected = false;
            Disconnected?.Invoke(this, reason);
        }
    }

    private void OnRfcommConnected(object? sender, EventArgs e)
    {
        _ = Task.Delay(150).ContinueWith(_ =>
        {
            if (IsRegisteredDeviceValid)
            {
                Connected?.Invoke(this, EventArgs.Empty);
                IsConnected = true;
            }
            else
            {
                Log.Error("BluetoothImpl: Suppressing Connected event, device not properly registered");
            }
        });
    }
    
    private void OnNewDataAvailable(object? sender, byte[] frame)
    {
        NewDataReceived?.Invoke(this, frame);
        
        /* Discard data if not properly registered */
        if (!IsRegisteredDeviceValid)
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

            try
            {
                foreach (var message in SppMessage.DecodeRawChunk(_incomingData, ActiveModel))
                {
                    MessageReceived?.Invoke(this, message);
                }
            }
            catch (InvalidPacketException ex)
            {
                InvalidDataReceived?.Invoke(this, ex);
            }
        }
    }
}