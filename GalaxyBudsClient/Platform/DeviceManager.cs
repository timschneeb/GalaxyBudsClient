using System;
using System.Linq;
using GalaxyBudsClient.Model.Config;

namespace GalaxyBudsClient.Platform;

public class DeviceManager
{
    private Device? _current;

    public Device? Current
    {
        get => _current;
        set
        {
            _current = value;
            Settings.Data.LastDeviceMac = value?.MacAddress;
            DeviceChanged?.Invoke(this, value);
        }
    }

    public event EventHandler<Device?>? DeviceChanged;
    
    public DeviceManager()
    {
        Current = Settings.Data.Devices.FirstOrDefault(d => d.MacAddress == Settings.Data.LastDeviceMac) ??
                  Settings.Data.Devices.FirstOrDefault();
    }
    
    public void RaiseDeviceChanged()
    {
        DeviceChanged?.Invoke(this, Current);
    }
    
    
}