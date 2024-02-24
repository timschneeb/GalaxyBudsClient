using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace GalaxyBudsClient.Utils.Converters
{
    public class BitmapAssetValueConverter : IValueConverter
    {
        public static BitmapAssetValueConverter Instance = new BitmapAssetValueConverter();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            switch (value)
            {
                case null:
                    return null;
                case string rawUri when targetType == typeof(Bitmap):
                {
                    // Allow for assembly overrides
                    var uri =
                        rawUri.StartsWith("avares://") ? new Uri(rawUri) : new Uri($"{Program.AvaresUrl}{rawUri}");
                    return new Bitmap(AssetLoader.Open(uri));
                }
                default:
                    throw new NotSupportedException();
            }
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}