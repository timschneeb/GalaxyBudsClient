using System;
using System.Threading.Tasks;

namespace GalaxyBudsClient.Bluetooth
{
    public class BluetoothDevice(string name, string address, bool isConnected, bool isPaired, BluetoothCoD cod)
    {
        public BluetoothDevice(uint cod) : this(string.Empty, string.Empty, false, false, new BluetoothCoD(cod))
        {
        }

        public override string ToString()
        {
            return $"{Name} ({Address})"; //$"BluetoothDevice[Name={Name},Address={Address},IsConnected={IsConnected},IsPaired='{IsPaired}',CoD='{Class}']";
        }

        public virtual string Name { get; } = name;
        public virtual string Address { get; } = address;
        public virtual bool IsConnected { get; } = isConnected;
        public virtual bool IsPaired { get; } = isPaired;
        public BluetoothCoD Class { get; } = cod;
    }
    
    public interface IBluetoothService
    {
        event EventHandler<BluetoothException>? BluetoothErrorAsync;   
        event EventHandler? Connecting;
        event EventHandler? Connected;
        event EventHandler? RfcommConnected;
        event EventHandler<string>? Disconnected;
        event EventHandler<byte[]>? NewDataAvailable;
        
        bool IsStreamConnected { get; }

        Task ConnectAsync(string macAddress, string serviceUuid, bool noRetry = false);
        Task DisconnectAsync();
        Task SendAsync(byte[] data);

        Task<BluetoothDevice[]> GetDevicesAsync();
    }
}