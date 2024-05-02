using System;
using System.Collections.Generic;
using System.Linq;
using GalaxyBudsClient.Bluetooth;
using GalaxyBudsClient.Model.Constants;
using Serilog;

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
        // TODO add Buds3/Pro
    }
    
    public static IDeviceSpec? FindByDevice(BluetoothDevice device)
    {
        if (device.ServiceUuids != null)
        {
            // If the device has this SPP service (>= Buds2), it can be renamed, so we can't rely on the name
            if(device.ServiceUuids.Any(x => x == Uuids.SppNew))
            {
                const string deviceIdPrefix = "d908aab5-7a90-4cbe-8641-86a553db";
                var deviceIdHex = device.ServiceUuids
                    .Select(s => s.ToString("D"))
                    .FirstOrDefault(s => s.StartsWith(deviceIdPrefix))?
                    .Replace(deviceIdPrefix, string.Empty);

                if (deviceIdHex != null)
                {
                    var deviceIdBytes = Enumerable.Range(0, deviceIdHex.Length)
                        .Where(x => x % 2 == 0)
                        .Select(x => Convert.ToByte(deviceIdPrefix.Substring(x, 2), 16))
                        .ToArray();

                    // Check based on DeviceManager.getWearableDeviceFromBudsUUID() from the Wearable container app
                    if (((DeviceIds)BitConverter.ToInt32(deviceIdBytes)).GetAssociatedModel() is Models model)
                    {
                        return FindByModel(model);
                    }
                }
                else if(device.ServiceUuids.Where(x => x == Uuids.LeAudio).Any(x => x == Uuids.Handsfree))
                {
                    return FindByModel(Models.Buds2Pro);
                }
                else
                {
                    // Likely Buds2
                    return FindByModel(Models.Buds2);
                }
            }
        }
        
        // Fallback: use the device name
        return Specs.FirstOrDefault(spec => device.Name.Contains(spec.DeviceBaseName));
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