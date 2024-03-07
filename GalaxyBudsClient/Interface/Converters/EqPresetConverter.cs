using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;

namespace GalaxyBudsClient.Interface.Converters;

public class EqPresetConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int)
            throw new ArgumentException("Unsupported value type");
        
        var key = (EqPreset)value switch
        {
            EqPreset.BassBoost => "eq_bass",
            EqPreset.Soft => "eq_soft",
            EqPreset.Dynamic => "eq_dynamic",
            EqPreset.Clear => "eq_clear",
            EqPreset.TrebleBoost => "eq_treble",
            _ => "unknown"
        };
        return Loc.Resolve(key);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}