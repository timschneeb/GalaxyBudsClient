using System;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Interface.Converters;
using GalaxyBudsClient.Model.Config;

namespace GalaxyBudsClient.Interface.MarkupExtensions;

public class DeviceStateBindingExtension : MarkupExtension
{
    public required IBinding DeviceBinding { get; set; }
    public required DeviceStateConverterTarget Target { get; set; }
    
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return new MultiBinding
        {
            Bindings = new[]
            {
                DeviceBinding,
                new Binding(nameof(Settings.Data.LastDeviceMac))
                {
                    Source = Settings.Data
                }
            },
            Converter = new DeviceSelectStateConverter(),
            ConverterParameter = Target
        };
    }
}