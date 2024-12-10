using System;
using System.Threading;
using System.Threading.Tasks;
using GalaxyBudsClient.Platform.Interfaces;
using GalaxyBudsClient.Platform.Model;

#pragma warning disable CS0067

namespace GalaxyBudsClient.Platform.Stubs;

public class DummyBluetoothService : IBluetoothService
{
    public event EventHandler<BluetoothException>? BluetoothErrorAsync;
    public event EventHandler? Connecting;
    public event EventHandler? Connected;
    public event EventHandler? RfcommConnected;
    public event EventHandler<string>? Disconnected;
    public event EventHandler<byte[]>? NewDataAvailable;
    public bool IsStreamConnected => false;
        
    private static string GetErrorMessage()
    {
        return PlatformUtils.Platform switch
        {
            PlatformUtils.Platforms.Windows => 
                "Bluetooth driver missing or not supported. On Windows systems, " +
                "we only support the default Microsoft Bluetooth stack drivers. " +
                "Third-party driver stacks such as BlueSoleil and Widcomm are NOT supported! " +
                "This error can also occur if no Bluetooth adapter is enabled or connected to your PC.",
            PlatformUtils.Platforms.Android =>
                "Your device does not support Bluetooth. " +
                "Please enable and attach a Bluetooth adapter to your system or " +
                "switch to a different device with Bluetooth capabilities.",
            _ => "The Bluetooth backend for your platform could not be initialized. " +
                 "Please check the application logs for more information.",
        };
    }
    
    public async Task ConnectAsync(string macAddress, string serviceUuid, CancellationToken cancelToken)
    {
        BluetoothErrorAsync?.Invoke(this, 
            new BluetoothException(BluetoothException.ErrorCodes.Unknown, GetErrorMessage()));
        await Task.CompletedTask;
    }

    public async Task DisconnectAsync()
    {
        await Task.CompletedTask;
    }

    public async Task SendAsync(byte[] data)
    {
        await Task.CompletedTask;
    }

    public async Task<BluetoothDevice[]> GetDevicesAsync()
    {
        await Task.CompletedTask;
        throw new BluetoothException(BluetoothException.ErrorCodes.NoAdaptersAvailable, GetErrorMessage());
    }
}