using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GalaxyBudsClient.Platform.Model;

namespace GalaxyBudsClient.Platform.Interfaces;

public interface IBluetoothService
{
    event EventHandler<BluetoothException>? BluetoothErrorAsync;   
    event EventHandler? Connecting;
    event EventHandler? Connected;
    event EventHandler? RfcommConnected;
    event EventHandler<string>? Disconnected;
    event EventHandler<byte[]>? NewDataAvailable;
        
    bool IsStreamConnected { get; }

    Task ConnectAsync(string macAddress, string serviceUuid, CancellationToken cancelToken);
    Task DisconnectAsync();
    Task SendAsync(byte[] data);

    Task<BluetoothDevice[]> GetDevicesAsync();
}