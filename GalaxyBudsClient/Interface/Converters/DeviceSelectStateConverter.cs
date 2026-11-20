using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using FluentIcons.Avalonia.Fluent;
using FluentIcons.Common;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Model.Config;

namespace GalaxyBudsClient.Interface.Converters;

public enum DeviceStateConverterTarget
{
    Label,
    Icon,
    Bool,
    BoolInverted
}

public class DeviceSelectStateConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2 || values[0] is not Device device)
        {
            // FACommandBarButton crashes on a null IconSource (FluentAvalonia 3.1.0),
            // so the icon target must always receive a concrete icon.
            return parameter is DeviceStateConverterTarget.Icon
                ? new SymbolIconSource
                {
                    Symbol = Symbol.CheckboxUnchecked,
                    IconVariant = IconVariant.Regular
                }
                : null;
        }

        var lastConnectedMac = values[1] as string;
        var isSelected = device.MacAddress == lastConnectedMac;

        return parameter switch
        {
            DeviceStateConverterTarget.Label => isSelected ? Strings.DevicesSelectActive : Strings.DevicesSelectInactive,
            DeviceStateConverterTarget.Icon => new SymbolIconSource
            {
                Symbol = isSelected ? Symbol.CheckboxChecked : Symbol.CheckboxUnchecked,
                IconVariant = isSelected ? IconVariant.Filled : IconVariant.Regular
            },
            DeviceStateConverterTarget.Bool => isSelected,
            DeviceStateConverterTarget.BoolInverted => !isSelected,
            _ => isSelected
        };
    }
}
