using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Encoder;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform.Interfaces;
using GalaxyBudsClient.Platform.Linux;
using GalaxyBudsClient.Platform.Stubs;
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
        DeviceMessageCache.Instance.Clear();
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
    
    public event EventHandler? ConnectedAlternative;
    public event EventHandler? ConnectingAlternative;
    public event EventHandler<string>? DisconnectedAlternative;
    public event EventHandler<SppAlternativeMessage>? MessageReceivedAlternative;
    public event EventHandler<InvalidPacketException>? InvalidDataReceivedAlternative;
    public event EventHandler<byte[]>? NewDataReceivedAlternative;
    public event EventHandler<BluetoothException>? BluetoothErrorAlternative;
    [Reactive] public bool IsConnectedAlternative { private set; get; }

    
    public Models CurrentModel => Device.Current?.Model ?? Models.NULL;
    public IDeviceSpec DeviceSpec => DeviceSpecHelper.FindByModel(CurrentModel) ?? new StubDeviceSpec();
    public static bool HasValidDevice => Settings.Data.Devices.Count > 0 && 
                                         Settings.Data.Devices.Any(x => x.Model != Models.NULL);
    
    [Reactive] public string DeviceName { private set; get; } = "Galaxy Buds";
    [Reactive] public bool IsConnected { private set; get; }
    [Reactive] public string LastErrorMessage { private set; get; } = string.Empty;
    [Reactive] public bool SuppressDisconnectionEvents { set; get; }
    [Reactive] public bool ShowDummyDevices { set; get; }

    public DeviceManager Device { get; } = new();
    
    [Reactive] public bool AlternativeModeEnabled { private set; get; }
    private readonly List<byte> _incomingData = [];
    private static readonly ConcurrentQueue<byte[]> IncomingQueue = new();
    private readonly CancellationTokenSource _loopCancelSource = new();
    private CancellationTokenSource _connectCancelSource = new();
    private readonly Task? _loop;
    // There is exactly one feature which requires connecting on a different UUID.

    private BluetoothImpl()
    {
        IBluetoothService? backend = null;
        
        try
        {
            // We don't want to initialize the backend in design mode. It would conflict with the actual application.
            if (!Design.IsDesignMode)
            {
                backend = PlatformImpl.Creator.CreateBluetoothService();
            }
        }
        catch (PlatformNotSupportedException)
        {
            Log.Error("BluetoothImpl: Critical error while preparing bluetooth backend");
        }

        if (backend == null)
        {
            Log.Warning("BluetoothImpl: Using Dummy.BluetoothService");
            backend = new DummyBluetoothService();
        }
        
        _backend = backend;
        _loop = Task.Run(DataConsumerLoop, _loopCancelSource.Token);
            
        _backend.Connecting += (_, _) =>
        {
            if (AlternativeModeEnabled)
                ConnectingAlternative?.Invoke(this, EventArgs.Empty);
            else
                Connecting?.Invoke(this, EventArgs.Empty);
        };
        _backend.BluetoothErrorAsync += (_, exception) => OnBluetoothError(exception); 
        _backend.NewDataAvailable += OnNewDataAvailable;
        _backend.RfcommConnected += OnRfcommConnected;
        _backend.Disconnected += OnDisconnected;
            
        MessageReceived += SppMessageReceiver.Instance.MessageReceiver;
        InvalidDataReceived += OnInvalidDataReceived;
    }

    public bool SetAltMode(bool altMode)
    {
        if (AlternativeModeEnabled == altMode)
        {
            return true;
        }

        if (!AlternativeModeEnabled && IsConnected)
        {
            Log.Error("BluetoothImpl: cannot enable alt mode while buds connected");
            return false;
        }
        if (AlternativeModeEnabled && IsConnectedAlternative)
        {
            Log.Error("BluetoothImpl: cannot disable alt mode while buds alt connected");
            return false;
        }

        AlternativeModeEnabled = altMode;
        return true;
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
            
        await _loopCancelSource.CancelAsync();
        await Task.Delay(50);

        try
        {
            _loop?.Dispose();
            _loopCancelSource.Dispose();
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
        if (AlternativeModeEnabled)
        {
            LastErrorMessage = exception.ErrorMessage ?? exception.Message;
            IsConnectedAlternative = false;
            BluetoothErrorAlternative?.Invoke(this, exception);
            return;
        }
        if (SuppressDisconnectionEvents) 
            return;
        
        LastErrorMessage = exception.ErrorMessage ?? exception.Message;
        IsConnected = false;
        BluetoothError?.Invoke(this, exception);
        DeviceMessageCache.Instance.Clear();
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
        var fallbackName = CurrentModel.GetModelMetadataAttribute()?.Name ?? Strings.Unknown;
        try
        {
            var devices = await _backend.GetDevicesAsync();
            var device = devices.FirstOrDefault(d => d.Address == Device.Current?.MacAddress);
            return device?.Name ?? fallbackName;
        }
        catch (BluetoothException ex)
        {
            Log.Error(ex, "BluetoothImpl.GetDeviceName: Error while fetching device name");
            return fallbackName;
        }
    }

    public async Task<bool> ConnectAsync(Device? device = null, bool alternative = false)
    {
        if (alternative != AlternativeModeEnabled)
        {
            Log.Error("BluetoothImpl: Connection attempt in wrong mode {Alternative}", alternative);
            return false;
        }
        // Create new cancellation token source if the previous one has already been used
        if(_connectCancelSource.IsCancellationRequested)
            _connectCancelSource = new CancellationTokenSource();
        
        device ??= Device.Current;

        if (!HasValidDevice || device == null)
        {
            Log.Error("BluetoothImpl: Connection attempt without valid device");
            return false;
        }
        
        /* Load from configuration */
        try
        {
            var uuid = AlternativeModeEnabled ? Uuids.SmepSpp.ToString() : DeviceSpec.ServiceUuid.ToString();
            if (uuid == null)
            {
                throw new BluetoothException(BluetoothException.ErrorCodes.UnsupportedDevice,
                    "BluetoothImpl: Connection attempt without valid UUID (alt mode enabled but UUID unset?)");
            }
            DeviceName = await GetDeviceNameAsync();
            device.Name = DeviceName;
                        
            await _backend.ConnectAsync(device.MacAddress,  uuid, _connectCancelSource.Token);
            return true;
        }
        catch (BluetoothException ex)
        {
            OnBluetoothError(ex);
            return false;
        }
        catch (TaskCanceledException)
        {
            Log.Warning("BluetoothImpl: Connection task cancelled");
            return false;
        }
    }

    public async Task DisconnectAsync(bool alternative = false)
    {
        if (!alternative && AlternativeModeEnabled)
        {
            Disconnected?.Invoke(this, "User requested disconnect while alt mode enabled");
            IsConnected = false;
            return;
        }
        if (alternative && !AlternativeModeEnabled)
        {
            Disconnected?.Invoke(this, "User requested alt disconnect while alt mode disable");
            IsConnectedAlternative = false;
            return;
        }
        try
        {
            // Cancel the connection attempt if it's still in progress
            try
            {
                await _connectCancelSource.CancelAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "BluetoothImpl: Error while cancelling connection attempt");
            } 

            await _backend.DisconnectAsync();
            if (alternative)
            {
                IsConnectedAlternative = false;
                DisconnectedAlternative?.Invoke(this, "User requested disconnect");
            }
            else
            {
                IsConnected = false;
                Disconnected?.Invoke(this, "User requested disconnect");
            }
            LastErrorMessage = string.Empty;
        }
        catch (BluetoothException ex)
        {
            OnBluetoothError(ex);
        }
    }

    public async Task SendAsync(SppMessage msg)
    {
        if (AlternativeModeEnabled)
            return;
        if (!IsConnected)
            return;

        try
        {
            Log.Verbose("<< Outgoing: {Msg}", msg);
                
            foreach(var hook in ScriptManager.Instance.MessageHooks)
            {
                hook.OnMessageSend(ref msg);
            }

            var raw = msg.Encode(false);
                
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

    public async Task SendAltAsync(SppAlternativeMessage msg)
    {
        if (!AlternativeModeEnabled)
            return;
        if (!IsConnectedAlternative)
            return;
        try
        {
            var data = msg.Msg.Encode(true);
            Log.Verbose("<< Outgoing (alt): {Msg}", msg);
            await _backend.SendAsync(data);
        }
        catch (BluetoothException ex)
        {
            OnBluetoothError(ex);
        }
    }
        
    public async Task SendResponseAsync(MsgIds id, params byte[]? payload)
    {
        await SendAsync(new SppMessage{Id = id, Payload = payload ?? [], Type = MsgTypes.Response});
    }

    public async Task SendRequestAsync(MsgIds id, params byte[]? payload)
    {
        await SendAsync(new SppMessage{Id = id, Payload = payload ?? [], Type = MsgTypes.Request});
    }
        
    public async Task SendRequestAsync(MsgIds id, bool payload)
    {
        await SendRequestAsync(id, payload ? [0x01] : [0x00]);
    }

    public async Task SendAsync(BaseMessageEncoder encoder)
    {
        await SendAsync(encoder.Encode());
    }

    public void UnregisterDevice(Device? device = null)
    {
        if (AlternativeModeEnabled)
        {
            Log.Error("Unregister in alt mode");
            return;
        }
        var mac = device?.MacAddress ?? Device.Current?.MacAddress;
        var toRemove = Settings.Data.Devices.FirstOrDefault(x => x.MacAddress == mac);
        if (toRemove == null)
            return;
        
        // Disconnect if the device is currently connected
        if (mac == Device.Current?.MacAddress)
            _ = DisconnectAsync();

        Settings.Data.Devices.Remove(toRemove);
        DeviceMessageCache.Instance.Clear();
        
        Device.Current = Settings.Data.Devices.FirstOrDefault();
        BatteryHistoryManager.Instance.DeleteDatabaseForDevice(toRemove);
    }
    
    private void OnDisconnected(object? sender, string reason)
    {
        if (AlternativeModeEnabled)
        {
            LastErrorMessage = Strings.Connlost;
            IsConnectedAlternative = false;
            DisconnectedAlternative?.Invoke(this, reason);
        }
        else if (!SuppressDisconnectionEvents)
        {
            LastErrorMessage = Strings.Connlost;
            IsConnected = false;
            Disconnected?.Invoke(this, reason);
            DeviceMessageCache.Instance.Clear();
        }
    }

    private void OnRfcommConnected(object? sender, EventArgs e)
    {
        if(!HasValidDevice)
        {
            Log.Error("BluetoothImpl: Suppressing Connected event, device not properly registered");
            return;
        }
        
        _ = Task.Delay(150).ContinueWith(_ =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (AlternativeModeEnabled)
                {
                    IsConnectedAlternative = true;
                    ConnectedAlternative?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    IsConnected = true;
                    Connected?.Invoke(this, EventArgs.Empty);
                }
            });
        });
    }
    
    private void OnNewDataAvailable(object? sender, byte[] frame)
    {
        /* Discard data if not properly registered */
        if (!HasValidDevice)
        {
            return;
        }

        if (AlternativeModeEnabled)
        {
            IsConnectedAlternative = true;
            NewDataReceivedAlternative?.Invoke(this, frame);
        }
        else
        {
            IsConnected = true;
            NewDataReceived?.Invoke(this, frame);
        }

        IncomingQueue.Enqueue(frame);
    }
    
    private void DataConsumerLoop()
    {
        while (true)
        {
            try
            {
                _loopCancelSource.Token.ThrowIfCancellationRequested();
                Task.Delay(50).Wait(_loopCancelSource.Token);
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

            ProcessDataBlock(_incomingData, CurrentModel);
        }
    }

    public void ProcessDataBlock(List<byte> data, Models targetModel)
    {
        try
        {
            foreach (var message in SppMessage.DecodeRawChunk(data, targetModel, AlternativeModeEnabled))
            {
                if (AlternativeModeEnabled)
                {
                    MessageReceivedAlternative?.Invoke(this, new SppAlternativeMessage(message));
                }
                else
                {
                    MessageReceived?.Invoke(this, message);
                }
            }
        }
        catch (InvalidPacketException ex)
        {
            if (AlternativeModeEnabled)
            {
                InvalidDataReceivedAlternative?.Invoke(this, ex);
            }
            else
            {
                InvalidDataReceived?.Invoke(this, ex);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed processing packet");
        }
    }
}