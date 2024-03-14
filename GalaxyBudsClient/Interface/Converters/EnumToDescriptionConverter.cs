using System;
using System.Globalization;
using Avalonia.Data.Converters;
using GalaxyBudsClient.Model.Attributes;

namespace GalaxyBudsClient.Interface.Converters;

public class EnumToDescriptionConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Enum e)
        {
            // Replace format argument with enum value if it exists
            return e.GetDescription().Replace("{0}", ((int)(object)e).ToString());
        }
        return "<null>";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
