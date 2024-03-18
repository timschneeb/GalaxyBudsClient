using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace GalaxyBudsClient.Interface.Converters
{
    public class MonthValueConverter : IValueConverter
    {
        public static readonly MonthValueConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value switch
            {
                null => null,
                int num => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(num),
                _ => throw new NotSupportedException()
            };
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}