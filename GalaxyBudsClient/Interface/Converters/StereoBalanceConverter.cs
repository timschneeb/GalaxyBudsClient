using System;
using System.Globalization;
using Avalonia.Data.Converters;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;

namespace GalaxyBudsClient.Interface.Converters;

public class StereoBalanceConverter : IValueConverter
{
    // TODO: handle localization changes
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int i)
            throw new ArgumentException("Unsupported value type");
        
        var progress = (int) ((float) i / 32 * 100.0f);
        return progress == 50 ? 
            Loc.Resolve("eq_stereo_balance_neutral") : 
            string.Format(Loc.Resolve("eq_stereo_balance_value"), 100 - progress, progress);

    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}