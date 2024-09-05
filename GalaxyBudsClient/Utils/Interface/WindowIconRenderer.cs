using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using Bitmap = Avalonia.Media.Imaging.Bitmap;
using Brushes = Avalonia.Media.Brushes;
using Point = Avalonia.Point;

namespace GalaxyBudsClient.Utils.Interface;

public static class WindowIconRenderer
{
    private static readonly WindowIcon DefaultIcon = MakeDefaultIcon();

    public static void UpdateDynamicIcon(IBasicStatusUpdate status)
    {
        var trayIcons = TrayIcon.GetIcons(Application.Current!);
        if (trayIcons == null)
            return;

        var batteryLeft = status.BatteryL;
        var batteryRight = status.BatteryR;

        // Ignore battery level of disconnected earbuds
        if (batteryLeft <= 0)
            batteryLeft = batteryRight;
        if (batteryRight <= 0)
            batteryRight = batteryLeft;

        int? level = Settings.Data.DynamicTrayIconMode switch
        {
            DynamicTrayIconModes.BatteryMin => Math.Min(batteryLeft, batteryRight),
            DynamicTrayIconModes.BatteryAvg => (batteryLeft + batteryRight) / 2,
            _ => null
        };

        if (level != null)
        {
            Dispatcher.UIThread.Post(() => { trayIcons[0].Icon = MakeFromBatteryLevel(Math.Min(level.Value, 99)); });
        }
    }

    public static void ResetIconToDefault()
    {
        Dispatcher.UIThread.Post(() =>
        {
            var trayIcons = TrayIcon.GetIcons(Application.Current!);
            if (trayIcons == null)
                return;

            trayIcons[0].Icon = DefaultIcon;
        });
    }

    private static WindowIcon MakeFromBatteryLevel(int level)
    {
        // Create the formatted text based on the properties set.
        var formattedText = new FormattedText(
            $"{level}",
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            Typeface.Default,
            210,
            Brushes.Black // This brush does not matter since we use the geometry of the text.
        );

        // Build the geometry object that represents the text.
        var textGeometry = formattedText.BuildGeometry(new Point(0, -30));
        var render = new RenderTargetBitmap(new PixelSize(256, 256), new Vector(96, 96));

        using (var ctx = render.CreateDrawingContext())
        {
            ctx.PushRenderOptions(new RenderOptions
            {
                BitmapInterpolationMode = BitmapInterpolationMode.HighQuality,
                TextRenderingMode = TextRenderingMode.Antialias,
                EdgeMode = EdgeMode.Antialias,
                RequiresFullOpacityHandling = true
            });


            if(PlatformUtils.IsOSX)
                ctx.DrawGeometry(Brushes.Black, new Pen(Brushes.Transparent, 0), textGeometry!);
            else
                ctx.DrawGeometry(new SolidColorBrush(Settings.Data.AccentColor), new Pen(Brushes.Transparent, 0), textGeometry!);
        }

        return new WindowIcon(render);
    }

    private static WindowIcon MakeDefaultIcon()
    {
        return new WindowIcon(MakeDefaultBitmap());
    }

    private static Bitmap MakeDefaultBitmap()
    {
        // OSX uses templated icons
        var type = PlatformUtils.IsOSX ? "black" : PlatformUtils.IsWindows ? "white_outlined_single" : "white_outlined_multi";
        var uri = $"{Program.AvaresUrl}/Resources/icon_{type}_tray.ico";
        return new Bitmap(AssetLoader.Open(new Uri(uri)));
    }
}
