using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using GalaxyBudsClient.Model.Constants;
using Serilog;

namespace GalaxyBudsClient.Model.Specifications
{
    public static class DeviceSpecHelper
    {
        private static readonly Collection<IDeviceSpec> _specs = new Collection<IDeviceSpec>();
        
        static DeviceSpecHelper()
        {
            _specs.Add(new StubDeviceSpec());
            _specs.Add(new BudsDeviceSpec());
            _specs.Add(new BudsPlusDeviceSpec());
            _specs.Add(new BudsLiveDeviceSpec());
            _specs.Add(new BudsProDeviceSpec());
            _specs.Add(new Buds2DeviceSpec());
        }
        
        public static IDeviceSpec? FindByDeviceName(string deviceName)
        {
            return _specs.FirstOrDefault(spec => deviceName.StartsWith(spec.DeviceBaseName));
        }
        
        public static IDeviceSpec? FindByModel(Models model)
        {
            return _specs.FirstOrDefault(spec => model == spec.Device);
        }
    }
}