using System;
using System.Globalization;
using Avalonia.Data.Converters;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Utils.Interface;

namespace GalaxyBudsClient.Interface.Converters;

public class StereoBalanceConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int i)
            throw new ArgumentException("Unsupported value type");
        
        var progress = (int) ((float) i / 32 * 100.0f);
        return progress == 50 ? Strings.EqStereoBalanceNeutral : 
            string.Format(Strings.EqStereoBalanceValue, 100 - progress, progress);

    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}