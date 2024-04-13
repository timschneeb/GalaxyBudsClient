using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using GalaxyBudsClient.Generated.Enums;

namespace GalaxyBudsClient.Interface.Converters;

public class ColorUintConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is uint i)
            return Color.FromUInt32(i);
        if (value is Color c)
            return c.ToUInt32();
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is uint i)
            return Color.FromUInt32(i);
        if (value is Color c)
            return c.ToUInt32();
        return null;
    }
}
