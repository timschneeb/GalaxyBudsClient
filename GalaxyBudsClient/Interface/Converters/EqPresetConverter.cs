using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;

namespace GalaxyBudsClient.Interface.Converters;

public class EqPresetConverter : IValueConverter
{
    // TODO: handle localization changes
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int)
            throw new ArgumentException("Unsupported value type");
        
        var key = (EqPresets)value switch
        {
            EqPresets.BassBoost => "eq_bass",
            EqPresets.Soft => "eq_soft",
            EqPresets.Dynamic => "eq_dynamic",
            EqPresets.Clear => "eq_clear",
            EqPresets.TrebleBoost => "eq_treble",
            _ => "unknown"
        };
        return Loc.Resolve(key);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}