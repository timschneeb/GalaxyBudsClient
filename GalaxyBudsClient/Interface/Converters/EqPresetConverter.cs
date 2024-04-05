using System;
using System.Globalization;
using Avalonia.Data.Converters;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Utils.Interface;

namespace GalaxyBudsClient.Interface.Converters;

public class EqPresetConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int)
            throw new ArgumentException("Unsupported value type");
  
        return (EqPresets)value switch
        {
            EqPresets.BassBoost => Strings.EqBass,
            EqPresets.Soft => Strings.EqSoft,
            EqPresets.Dynamic => Strings.EqDynamic,
            EqPresets.Clear => Strings.EqClear,
            EqPresets.TrebleBoost => Strings.EqTreble,
            _ => Strings.Unknown
        };;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}