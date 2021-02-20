using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Enumeration;
using Serilog;

namespace GalaxyBudsClient.Bluetooth.WindowsRT
{
    internal class BluetoothDeviceRT : BluetoothDevice
    {
        public DeviceInformation DeviceInfo { get; }

        public string Id => DeviceInfo.Id;
        public override string Name => DeviceInfo.Name;
        public override string Address => DeviceInfo.Properties["System.Devices.Aep.DeviceAddress"]?.ToString() ?? string.Empty;
        public override bool IsPaired => (bool) (DeviceInfo.Properties["System.Devices.Aep.IsPaired"] ?? false);
        public override bool IsConnected => (bool) (DeviceInfo.Properties["System.Devices.Aep.IsConnected"] ?? false);
        
        public BluetoothDeviceRT(DeviceInformation info) : base(0)
        {
            DeviceInfo = info;
        }

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