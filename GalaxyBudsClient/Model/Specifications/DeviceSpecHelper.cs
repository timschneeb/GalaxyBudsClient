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
            Assembly? a = Assembly.GetEntryAssembly();
            
            if (a?.DefinedTypes != null)
            {
                foreach (TypeInfo ti in a.DefinedTypes)
                {
                    if (ti.ImplementedInterfaces.Contains(typeof(IDeviceSpec)))
                    {
                        var inst = a.CreateInstance(ti.FullName ?? string.Empty) as IDeviceSpec;
                        if (inst != null)
                        {
                            _specs.Add(inst);
                        }
                        else
                        {
                            Log.Error($"DeviceSpecHelper: unable to instantiate '{ti.FullName}'");
                        }
                    }
                }
            }
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