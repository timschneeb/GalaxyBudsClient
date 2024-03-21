using System;
using System.Globalization;
using Avalonia.Data.Converters;
using GalaxyBudsClient.Model.Attributes;

namespace GalaxyBudsClient.Interface.Converters;

public class ModelNameConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Enum e)
        {
            var name = e.GetModelMetadata()?.Name;
            if (name != null) 
                return name;
        }
        return "<null>";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
