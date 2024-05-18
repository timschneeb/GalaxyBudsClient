using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GalaxyBudsClient.Platform.Interfaces
{
    public class BluetoothDevice(string name, string address, bool isConnected, bool isPaired, BluetoothCoD cod, Guid[]? serviceUuids = null)
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
        public virtual Guid[]? ServiceUuids { get; } = serviceUuids;

        public static IEnumerable<BluetoothDevice> DummyDevices()
        {
            /* Dummy devices for testing */
            var cod = new BluetoothCoD(0);
            return new BluetoothDevice[]
            {
                new("Galaxy Buds (36FD) [Dummy]", "36:AB:38:F5:04:FD", true, true, cod),
                new("Galaxy Buds+ (A2D5) [Dummy]", "A2:BF:D4:4A:52:D5", true, true, cod),
                new("Galaxy Buds Live (4AC3) [Dummy]", "4A:6B:87:E5:12:C3", true, true, cod),
                new("Galaxy Buds Pro (E43F) [Dummy]", "E4:25:FA:6D:B9:3F", true, true, cod),
                new("Galaxy Buds2 (D592) [Dummy]", "D5:97:B8:23:AB:92", true, true, cod),
                new("Galaxy Buds2 Pro (3292) [Dummy]", "32:97:B8:23:AB:92", true, true, cod),
                new("Galaxy Buds FE (A7D4) [Dummy]", "A7:97:B8:23:AB:D4", true, true, cod)
            };
        }
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

        Task ConnectAsync(string macAddress, string serviceUuid, CancellationToken cancelToken);
        Task DisconnectAsync();
        Task SendAsync(byte[] data);

        Task<BluetoothDevice[]> GetDevicesAsync();
    }
}