using Windows.Devices.Enumeration;
using GalaxyBudsClient.Platform.Interfaces;

namespace GalaxyBudsClient.Platform.WindowsRT
{
    internal class BluetoothDeviceRt(DeviceInformation info) : BluetoothDevice(0)
    {
        public DeviceInformation DeviceInfo { get; } = info;

        public string Id => DeviceInfo.Id;
        public override string Name => DeviceInfo.Name;
        public override string Address => DeviceInfo.Properties["System.Devices.Aep.DeviceAddress"]?.ToString() ?? string.Empty;
        public override bool IsPaired => (bool) (DeviceInfo.Properties["System.Devices.Aep.IsPaired"] ?? false);
        public override bool IsConnected => (bool) (DeviceInfo.Properties["System.Devices.Aep.IsConnected"] ?? false);

        public void Update(DeviceInformationUpdate? update)
        {
            if (update == null)
            {
                return;
            }
            
            DeviceInfo.Update(update);
        }
    }
}