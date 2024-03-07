using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace GalaxyBudsClient.Interface.Converters;
public class ResourceKeyToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value != null && 
            Application.Current!.TryGetResource(value, null, out var icon))
        {
            return icon;
        }

        return null;
    }

    object IValueConverter.ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
