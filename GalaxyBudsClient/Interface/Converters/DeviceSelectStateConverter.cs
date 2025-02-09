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
        if (values.Count < 2)
            return null;

        // If the first value is null, you likely can't compare anything, so pick a fallback:
        if (values[0] is null)
            return null;

        if (values[0] is not Device device)
        {
            // Instead of throwing, return a fallback
            return null; 
        }

        var lastConnectedMac = values[1] as string;
        var isSelected = device.MacAddress == lastConnectedMac;

        return parameter switch
        {
            DeviceStateConverterTarget.Label => isSelected ? Strings.DevicesSelectActive : Strings.DevicesSelectInactive,
            DeviceStateConverterTarget.Icon => new SymbolIconSource
            {
                Symbol = isSelected ? Symbol.CheckboxChecked : Symbol.CheckboxUnchecked,
                IsFilled = isSelected
            },
            DeviceStateConverterTarget.Bool => isSelected,
            DeviceStateConverterTarget.BoolInverted => !isSelected,
            _ => isSelected
        };
    }
}
