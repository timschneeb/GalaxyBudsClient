using System;
using System.Threading.Tasks;

namespace GalaxyBudsClient.Bluetooth
{
    public class BluetoothDevice
    {
        public BluetoothDevice(string name, string address, bool isConnected, bool isPaired, BluetoothCoD cod)
        {
            Name = name;
            Address = address;
            IsConnected = isConnected;
            IsPaired = isPaired;
            Class = cod;
        }
        
        public BluetoothDevice(uint cod)
        {
            Name = string.Empty;
            Address = string.Empty;
            Class = new BluetoothCoD(cod);
        }

        public override string ToString()
        {
            return $"BluetoothDevice[Name={Name},Address={Address},IsConnected={IsConnected},IsPaired='{IsPaired}',CoD='{Class}']";
        }

        public virtual string Name { get; }
        public virtual string Address { get; }
        public virtual bool IsConnected { get; }
        public virtual bool IsPaired { get; }
        public BluetoothCoD Class { get; }
        
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