using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using GalaxyBudsClient.Generated.Enums;
using GalaxyBudsClient.Model.Attributes;

namespace GalaxyBudsClient.Interface.Converters;

public class EnumToDescriptionConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return "<null>";

        // Replace format argument with enum value if it exists
        return CompiledEnums.GetDescriptionByType(value.GetType(), value)
            .Replace("{0}", ((int)value).ToString());
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
