using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.Bluetooth;
using Android.Content;
using Android.Runtime;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Platform.Interfaces;
using GalaxyBudsClient.Platform.Model;
using Java.Lang;
using Java.Util;
using Serilog;
using BluetoothDevice = GalaxyBudsClient.Platform.Model.BluetoothDevice;

#pragma warning disable CS0067

namespace GalaxyBudsClient.Android.Bluetooth;

public class BluetoothService : IBluetoothService
{
    private static readonly ConcurrentQueue<byte[]> TransmitterQueue = new();
        
    private CancellationTokenSource _cancelSource = new();
    private Task? _loop;
        
    public event EventHandler? RfcommConnected;
    public event EventHandler? Connecting;
    public event EventHandler? Connected;
    public event EventHandler<string>? Disconnected;
    public event EventHandler<BluetoothException>? BluetoothErrorAsync;
    public event EventHandler<byte[]>? NewDataAvailable;

    public bool IsStreamConnected { get; set; }

    private readonly Context _context;
    private readonly BluetoothAdapter _adapter;
        
    private BluetoothSocket? _socket;
        
    public BluetoothService(Context appContext)
    {
        _context = appContext;
            
        RfcommConnected += (sender, args) => IsStreamConnected = true;
        Disconnected += (sender, args) => IsStreamConnected = false;
            
        var bluetoothManager = (BluetoothManager?)_context.GetSystemService(Class.FromType(typeof(BluetoothManager)));
        var adapter = bluetoothManager?.Adapter;

        _adapter = adapter ?? throw new PlatformNotSupportedException("This device does not support Bluetooth");
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

        _socket?.Close();

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
        catch (Java.IO.IOException ex)
        {
            _socket.Close();
            _socket = null;
            
            Log.Error("Android.BluetoothService: ConnectAsync: {ExMessage}", ex.Message);
            throw new BluetoothException(BluetoothException.ErrorCodes.ConnectFailed, ex.Message);
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

        _cancelSource = new CancellationTokenSource();
        _loop = Task.Run(BluetoothServiceLoop, _cancelSource.Token);
                
        RfcommConnected?.Invoke(this, EventArgs.Empty);
    }
    #endregion     
        
    #region Disconnection
    public async Task DisconnectAsync()
    {
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
        _socket?.Close();
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
                catch (Java.IO.IOException ex)
                {
                    Log.Error("Android.BluetoothService: BluetoothServiceLoop: Exception thrown while writing unsafe stream: {ExMessage}. Cancelled",
                        ex.Message);
                    Disconnected?.Invoke(this, ex.Message ?? "Error while receiving data");
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
                    catch (Java.IO.IOException ex)
                    {
                        Log.Error(
                            "Android.BluetoothService: BluetoothServiceLoop: Exception thrown while writing unsafe stream: {ExMessage}. Cancelled",
                            ex.Message);
                        Disconnected?.Invoke(this, ex.Message ?? "Error while sending data");
                    }
                }
            }

            if (!hasIoActivity)
            {
                await Task.Delay(50);
            }
        }
    }
    #endregion
}