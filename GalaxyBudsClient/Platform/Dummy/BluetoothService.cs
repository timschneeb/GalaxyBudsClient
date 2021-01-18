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
            BluetoothErrorAsync?.Invoke(this, new BluetoothException(BluetoothException.ErrorCodes.Unknown, 
                "Platform configuration not supported." +
                "For Windows users, it is recommended to use Windows 10 build 1803 and later since it provides a much more reliable and newer bluetooth interface. " +
                $"You can find more information about the cause of this error in the application logs ({PlatformUtils.CombineDataPath("application.log")})."));
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
                "Platform not supported. On Windows systems, we only support the default Microsoft Bluetooth stack drivers. Third-party driver stacks such as BlueSoleil and Widcomm are NOT supported! This error can also occur if no Bluetooth adapter is enabled or connected to your PC.");
        }
    }
}