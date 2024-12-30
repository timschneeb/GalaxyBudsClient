using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Platform;
using System.Reflection;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Model.Config;

namespace GalaxyBudsClient.Interface.MarkupExtensions;

public class DeviceColorBindingSource : MarkupExtension
{
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var currentDevice = BluetoothImpl.Instance.Device.Current;
        if (currentDevice == null)
            return Array.Empty<DeviceIds>();

        // Get current device's model and color
        var currentModel = currentDevice.Model;
        var currentColor = currentDevice.DeviceColor ?? 
                         DeviceMessageCache.Instance.ExtendedStatusUpdate?.DeviceColor ?? 
                         DeviceIds.Unknown;

        // Filter DeviceIds based on current model
        var values = Enum.GetValues(typeof(DeviceIds))
            .Cast<DeviceIds>()
            .Where(x => {
                var field = typeof(DeviceIds).GetField(x.ToString());
                var attr = field?.GetCustomAttribute<AssociatedModelAttribute>();
                return attr != null && attr.Model == currentModel;
            })
            .OrderBy(x => x.ToString())
            .ToList();

        // Move current color to top if it exists in the list
        if (currentColor != DeviceIds.Unknown && values.Contains(currentColor))
        {
            values.Remove(currentColor);
            values.Insert(0, currentColor);
        }

        // Set the initial value if no override is set
        if (Settings.Data.ColorOverride == null)
        {
            Settings.Data.ColorOverride = currentColor;
        }
        
        return values;
    }
}
