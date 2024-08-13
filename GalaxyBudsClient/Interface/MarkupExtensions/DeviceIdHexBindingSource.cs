using System;
using System.Linq;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Message;

namespace GalaxyBudsClient.Interface.MarkupExtensions;

public class DeviceIdHexBindingSource : MarkupExtension
{
    public override object ProvideValue(IServiceProvider? serviceProvider)
    {
        return HiddenCmds.GetDeviceIds()
            .Select(c => new DeviceIdHexBindingItem
            {
                Id = c.Key,
                Description = $"{c.Value} {c.Value}"
            })
            .ToArray();
    }
}

public class DeviceIdHexBindingItem
{
    public required string Id { init; get; }
    public required string Description { init; get; }
}