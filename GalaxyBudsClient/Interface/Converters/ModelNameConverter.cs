using System;
using System.Globalization;
using Avalonia.Data.Converters;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Interface.Converters;

public class ModelNameConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Models e) 
            return "<null>";
        
        return e.GetModelMetadataAttribute()?.Name ?? "<null>";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
