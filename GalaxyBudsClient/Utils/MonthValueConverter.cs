using System;
using System.Globalization;
using System.Reflection;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace GalaxyBudsClient.Utils
{
    public class MonthValueConverter : IValueConverter
    {
        public static MonthValueConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value switch
            {
                null => null,
                int num /*when targetType == typeof(string)*/ =>
                    CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(num),
                _ => throw new NotSupportedException()
            };
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}