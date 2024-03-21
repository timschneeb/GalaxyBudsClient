using System.Collections.Generic;
using System.Linq;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Model.Specifications;

public static class DeviceSpecHelper
{
    private static readonly List<IDeviceSpec> Specs = [];
        
    static DeviceSpecHelper()
    {
        Specs.Add(new StubDeviceSpec());
        Specs.Add(new BudsDeviceSpec());
        Specs.Add(new BudsPlusDeviceSpec());
        Specs.Add(new BudsLiveDeviceSpec());
        Specs.Add(new BudsProDeviceSpec());
        Specs.Add(new Buds2ProDeviceSpec()); // important: B2Pro is added before B2 to avoid false positives
        Specs.Add(new Buds2DeviceSpec());
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