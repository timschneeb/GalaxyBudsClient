using System;
using System.Threading.Tasks;
using GalaxyBudsClient.Bluetooth;

#pragma warning disable CS0067

namespace GalaxyBudsClient.Platform.Dummy
{
    public class BluetoothService : IBluetoothService
    {
        public event EventHandler<BluetoothException>? BluetoothErrorAsync;
        public event EventHandler? Connecting;
        public event EventHandler? Connected;
        public event EventHandler? RfcommConnected;
        public event EventHandler<string>? Disconnected;
        public event EventHandler<byte[]>? NewDataAvailable;
        public bool IsStreamConnected => false;
        
        public async Task ConnectAsync(string macAddress, string serviceUuid, bool noRetry = false)
        {
            BluetoothErrorAsync?.Invoke(this, new BluetoothException(BluetoothException.ErrorCodes.Unknown, "Platform not supported. On Windows systems, we only support the default Microsoft Bluetooth stack drivers. Third-party stacks such as BlueSoleil and Widcomm are NOT supported!"));
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
            throw new PlatformNotSupportedException(
                "ERROR: Platform not supported. On Windows systems, we only support the default Microsoft Bluetooth stack drivers. Third-party stacks such as BlueSoleil and Widcomm are NOT supported!");
        }
    }
}