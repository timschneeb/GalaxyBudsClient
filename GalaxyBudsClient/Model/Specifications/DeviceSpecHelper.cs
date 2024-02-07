using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using GalaxyBudsClient.Model.Constants;
using Serilog;

namespace GalaxyBudsClient.Model.Specifications
{
    public static class DeviceSpecHelper
    {
        private static readonly List<IDeviceSpec> Specs = new List<IDeviceSpec>();
        
        static DeviceSpecHelper()
        {
            Specs.Add(new StubDeviceSpec());
            Specs.Add(new BudsDeviceSpec());
            Specs.Add(new BudsPlusDeviceSpec());
            Specs.Add(new BudsLiveDeviceSpec());
            Specs.Add(new BudsProDeviceSpec());
            Specs.Add(new Buds2DeviceSpec());
            Specs.Add(new Buds2ProDeviceSpec());
            Specs.Add(new BudsFeDeviceSpec());
        }
        
        public static IDeviceSpec? FindByDeviceName(string deviceName)
        {
            return Specs.FirstOrDefault(spec => deviceName.Contains(spec.DeviceBaseName));
        }
        
        public static IDeviceSpec? FindByModel(Models model)
        {
            return Specs.FirstOrDefault(spec => model == spec.Device);
        }
    }
}