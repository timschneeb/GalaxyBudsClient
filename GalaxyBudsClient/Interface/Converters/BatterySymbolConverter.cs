using System;
using System.Globalization;
using Avalonia.Data.Converters;
using FluentIcons.Common;

namespace GalaxyBudsClient.Interface.Converters;

public class BatterySymbolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int i)
            throw new ArgumentException("Unsupported value type");
        
        return i switch
        {
            < 5 => Symbol.Battery0,
            < 10 => Symbol.Battery1,
            < 20 => Symbol.Battery2,
            < 30 => Symbol.Battery3,
            < 40 => Symbol.Battery4,
            < 50 => Symbol.Battery5,
            < 60 => Symbol.Battery6,
            < 70 => Symbol.Battery7,
            < 80 => Symbol.Battery8,
            < 90 => Symbol.Battery9,
            _ => Symbol.Battery10
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}