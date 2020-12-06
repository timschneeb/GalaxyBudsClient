using System;
using System.Threading.Tasks;

namespace GalaxyBudsClient.Bluetooth
{
    public interface IBluetoothService
    {
        event EventHandler Connected;
        event EventHandler RfcommConnected;
        event EventHandler<string> Disconnected;
        event EventHandler<byte[]> NewDataAvailable;
        
        bool IsStreamConnected { set; get; }

        Task ConnectAsync(string macAddress, string serviceUuid);
        Task DisconnectAsync();
        Task SendAsync(byte[] data);
    }
}