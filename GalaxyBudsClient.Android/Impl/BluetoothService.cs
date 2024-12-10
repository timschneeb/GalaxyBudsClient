using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Android.Bluetooth;
using Android.Content;
using Android.Runtime;
using GalaxyBudsClient.Android.Utils;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Platform.Interfaces;
using GalaxyBudsClient.Platform.Model;
using Java.Lang;
using Java.Util;
using Sentry;
using Serilog;
using BluetoothDevice = GalaxyBudsClient.Platform.Model.BluetoothDevice;
using Exception = System.Exception;
using Timer = System.Timers.Timer;

#pragma warning disable CS0067

namespace GalaxyBudsClient.Android.Impl;

public class BluetoothService : IBluetoothService
{
    private static readonly ConcurrentQueue<byte[]> TransmitterQueue = new();
        
    private CancellationTokenSource _cancelSource = new();
    private Task? _loop;
    private readonly Timer _connCheckTimer = new(2000);
        
    public event EventHandler? RfcommConnected;
    public event EventHandler? Connecting;
    public event EventHandler? Connected;
    public event EventHandler<string>? Disconnected;
    public event EventHandler<BluetoothException>? BluetoothErrorAsync;
    public event EventHandler<byte[]>? NewDataAvailable;

    public bool IsStreamConnected { get; set; }
    private readonly BluetoothAdapter _adapter;
    private BluetoothSocket? _socket;
        
    public BluetoothService(Context context)
    {
        RfcommConnected += (sender, args) => IsStreamConnected = true;
        BluetoothErrorAsync += (sender, exception) => _connCheckTimer.Stop(); 
        Disconnected += (sender, args) =>
        {
            _connCheckTimer.Stop();
            IsStreamConnected = false;
        };
            
        var bluetoothManager = (BluetoothManager?)context.GetSystemService(Class.FromType(typeof(BluetoothManager)));
        var adapter = bluetoothManager?.Adapter;

        _adapter = adapter ?? throw new PlatformNotSupportedException("This device does not support Bluetooth");
        _connCheckTimer.Elapsed += OnConnectionCheckTimerHit;
    }

    private void OnConnectionCheckTimerHit(object? sender, ElapsedEventArgs e)
    {
        if (_socket == null) 
            return;

        if (_socket.IsConnected == false)
        {
            Log.Warning("Android.BluetoothService: Connection check timer hit. Connection not active anymore");
            throw new BluetoothException(BluetoothException.ErrorCodes.ConnectFailed, 
                "Connection failed. Your earbuds are either out of range or in use by the official manager app.");
        }

        _connCheckTimer.Start();
    }

    private void RequireBluetoothEnabled()
    {
        if (!_adapter.IsEnabled)
        {
            throw new BluetoothException(BluetoothException.ErrorCodes.NoAdaptersAvailable,
                "Bluetooth is not enabled");
        }
    }

    #region Connection
    public async Task ConnectAsync(string macAddress, string uuid, CancellationToken cancelToken)
    {
        RequireBluetoothEnabled();

        _socket?.CloseSafely();

        Connecting?.Invoke(this, EventArgs.Empty);
            
        var device = _adapter.GetRemoteDevice(macAddress);
        if (device == null)
        {
            throw new BluetoothException(BluetoothException.ErrorCodes.ConnectFailed,
                "The selected Bluetooth device is not available. Please make sure it is paired and connected.");
        }

        var socket = device.CreateRfcommSocketToServiceRecord(UUID.FromString(uuid));

        _socket = socket ?? throw new BluetoothException(BluetoothException.ErrorCodes.ConnectFailed,
            "The selected Bluetooth device does not support Samsung's configuration protocol. " +
            "Either the device is not a genuine Samsung Galaxy Buds device or the wrong model has been chosen during manual setup.");
            
        Log.Debug("Connecting to socket for {MacAddress} with UUID {Uuid}...", macAddress, uuid);
        try
        {
            await _socket.ConnectAsync();
        }
        catch (Exception ex) when (ex is Java.IO.IOException or IOException)
        {
            _socket.CloseSafely();
            _socket = null;
            
            Log.Error("Android.BluetoothService: ConnectAsync: {ExMessage}", ex.Message);
            throw new BluetoothException(BluetoothException.ErrorCodes.ConnectFailed, "Connection failed. Your earbuds are either out of range or in use by the official manager app.");
        }

        OnConnectionEstablished();
    }

    private void OnConnectionEstablished()
    {
        Log.Debug("Android.BluetoothService: Connection established. Launching BluetoothServiceLoop");

        try
        {
            _loop?.Dispose();
        }
        catch (InvalidOperationException) {}

        _connCheckTimer.Start();
        
        _cancelSource = new CancellationTokenSource();
        _loop = Task.Run(BluetoothServiceLoop, _cancelSource.Token);
                
        RfcommConnected?.Invoke(this, EventArgs.Empty);
    }
    #endregion     
        
    #region Disconnection
    public async Task DisconnectAsync()
    {
        _connCheckTimer.Stop();
        
        Log.Debug("Android.BluetoothService: Disconnecting...");
        if (_loop == null || _loop.Status == TaskStatus.Created)
        {
            Log.Debug("Android.BluetoothService: BluetoothServiceLoop not yet launched. No need to cancel");
        }
        else
        {  
            Log.Debug("Android.BluetoothService: Cancelling BluetoothServiceLoop...");
            await _cancelSource.CancelAsync();
        }

        /* Disconnect device if not already done... */
        _socket?.CloseSafely();
        _socket = null;
    }
    #endregion

    #region Transmission
    public async Task SendAsync(byte[] data)
    {
        RequireBluetoothEnabled();
            
        lock (TransmitterQueue)
        {
            TransmitterQueue.Enqueue(data);
        }
        await Task.CompletedTask;
    }

    public Task<BluetoothDevice[]> GetDevicesAsync()
    {
        RequireBluetoothEnabled();
            
        return Task.FromResult(_adapter.BondedDevices?
            .Where(d => d.Address != null)
            .Select(d => new BluetoothDevice(d.Name ?? d.Address!, d.Address!, 
                true /* connection state unknown */, true, 
                new BluetoothCoD((uint?)d.BluetoothClass?.MajorDeviceClass ?? 0, 0),
                d
                    .GetUuids()?
                    .Where(u => u.Uuid != null)
                    .Select(u => Guid.Parse(u.Uuid!.ToString()))
                    .ToArray())
            )
            .ToArray() ?? []);
    }
    #endregion
        
    #region Service
    private async void BluetoothServiceLoop()
    {
        while (true)
        {
            try
            {
                try
                {
                    _cancelSource.Token.ThrowIfCancellationRequested();
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                bool hasIoActivity;

                var inputStream = ((InputStreamInvoker?)_socket?.InputStream)?.BaseInputStream;

                if (inputStream == null)
                {
                    /* Stream not yet ready */
                    await Task.Delay(100);
                    continue;
                }

                // ReSharper disable once ConstantConditionalAccessQualifier
                var incomingCount = inputStream?.Available() ?? 0;
                if (incomingCount > 0)
                {
                    IsStreamConnected = true;


                    /* Handle incoming stream */
                    var buffer = new byte[incomingCount];
                    var dataAvailable = false;
                    try
                    {
                        dataAvailable = inputStream?.Read(buffer, 0, incomingCount) >= 0;
                    }
                    catch (Exception ex) when (ex is Java.IO.IOException or IOException)
                    {
                        Log.Error(
                            "Android.BluetoothService: BluetoothServiceLoop: Exception thrown while writing unsafe stream: {ExMessage}. Cancelled",
                            ex.Message);
                        Disconnected?.Invoke(this, "Connection closed. The earbuds are either out of range or in use by the official manager app.");
                    }

                    if (dataAvailable)
                    {
                        NewDataAvailable?.Invoke(this, buffer);
                    }
                }

                /* Handle outgoing stream */
                lock (TransmitterQueue)
                {
                    if (TransmitterQueue.IsEmpty || !TransmitterQueue.TryDequeue(out var raw))
                    {
                        hasIoActivity = false;
                    }
                    else
                    {
                        hasIoActivity = true;

                        try
                        {
                            ((OutputStreamInvoker?)_socket?.OutputStream)?
                                // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
                                .BaseOutputStream?
                                .Write(raw, 0, raw.Length);
                        }
                        catch (Exception ex) when (ex is Java.IO.IOException or IOException)
                        {
                            Log.Error(
                                "Android.BluetoothService: BluetoothServiceLoop: Exception thrown while writing unsafe stream: {ExMessage}. Cancelled",
                                ex.Message);
                            Disconnected?.Invoke(this, "Error while sending data. Connection to the earbuds has been closed.");
                        }
                    }
                }

                if (!hasIoActivity)
                {
                    await Task.Delay(50);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Android.BluetoothService: BluetoothServiceLoop: Unhandled exception");
                SentrySdk.CaptureException(ex);
            }
        }
    }
    #endregion
}