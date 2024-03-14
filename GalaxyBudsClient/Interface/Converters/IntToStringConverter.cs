using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;

namespace GalaxyBudsClient.Interface.Converters;

public abstract class IntToStringConverter : IValueConverter
{
    protected abstract Dictionary<int, string> Mapping { get; }
    
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int i)
            throw new ArgumentException("Unsupported value type");
        
        return Mapping.TryGetValue(i, out var str) ? str : i.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}