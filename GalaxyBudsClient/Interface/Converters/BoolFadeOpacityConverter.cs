using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace GalaxyBudsClient.Interface.Converters;

public class BoolFadeOpacityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool b)
            throw new ArgumentException("Unsupported value type");
        
        return b ? 1 : 0.4;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}