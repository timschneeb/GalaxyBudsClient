using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Enumeration;
using GalaxyBudsClient.Platform.Interfaces;
using GalaxyBudsClient.Platform.Model;

namespace GalaxyBudsClient.Platform.WindowsRT
{
    internal class BluetoothDeviceRt(DeviceInformation info, IEnumerable<Guid>? services) : BluetoothDevice(0)
    {
        public DeviceInformation DeviceInfo { get; } = info;
        private IEnumerable<Guid>? _services = services;

        public string Id => DeviceInfo.Id;
        public override string Name => DeviceInfo.Name;
        public override string Address => DeviceInfo.Properties["System.Devices.Aep.DeviceAddress"]?.ToString() ?? string.Empty;
        public override bool IsPaired => (bool) (DeviceInfo.Properties["System.Devices.Aep.IsPaired"] ?? false);
        public override bool IsConnected => (bool) (DeviceInfo.Properties["System.Devices.Aep.IsConnected"] ?? false);
        public override Guid[]? ServiceUuids => _services?.ToArray();

        public void Update(DeviceInformationUpdate? update, IEnumerable<Guid>? services)
        {
            if (update == null)
            {
                return;
            }
            
            DeviceInfo.Update(update);
            _services = services;
        }
    }
}