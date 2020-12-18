using Serilog;
using System;
using System.Threading.Tasks;

namespace GalaxyBudsClient.Bluetooth
{    
    public class DummyBluetoothService : IBluetoothService
    {
        bool IBluetoothService.IsStreamConnected => false;

        public event EventHandler<BluetoothException>? BluetoothErrorAsync;
        public event EventHandler? Connecting;
        public event EventHandler? Connected;
        public event EventHandler? RfcommConnected;
        public event EventHandler<string>? Disconnected;
        public event EventHandler<byte[]>? NewDataAvailable;

        public DummyBluetoothService()
        {
            Log.Warning("DummyBluetoothService instantiated");
        }

        public Task ConnectAsync(string macAddress, string serviceUuid, bool noRetry = false) {
            Log.Debug("DummyBluetoothService: ConnectAsync called");
            return Task.CompletedTask;
        }

        public Task DisconnectAsync()
        {
            Log.Debug("DummyBluetoothService: DisconnectAsync called");
            return Task.CompletedTask;
        }

        public Task SendAsync(byte[] data)
        {
            Log.Debug("DummyBluetoothService: SendAsync called");
            return Task.CompletedTask;
        }

        public Task<BluetoothDevice[]> GetDevicesAsync() {
            Log.Debug("DummyBluetoothService: GetDevicesAsync called");
            return Task.FromResult(new BluetoothDevice[0]);
        }
    }
}