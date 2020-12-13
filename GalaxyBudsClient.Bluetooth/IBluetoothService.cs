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

        public override string ToString()
        {
            return $"BluetoothDevice[Name={Name},Address={Address},IsConnected={IsConnected},IsPaired='{IsPaired}',CoD='{Class}']";
        }

        public string Name { get; }
        public string Address { get; }
        public bool IsConnected { get; }
        public bool IsPaired { get; }
        public BluetoothCoD Class { get; }
        
    }
    
    public interface IBluetoothService
    {
        event EventHandler<BluetoothException> BluetoothErrorAsync;   
        event EventHandler Connecting;
        event EventHandler Connected;
        event EventHandler RfcommConnected;
        event EventHandler<string> Disconnected;
        event EventHandler<byte[]> NewDataAvailable;
        
        bool IsStreamConnected { set; get; }

        Task ConnectAsync(string macAddress, string serviceUuid, bool noRetry = false);
        Task DisconnectAsync();
        Task SendAsync(byte[] data);

        Task<BluetoothDevice[]> GetDevicesAsync();
    }
}