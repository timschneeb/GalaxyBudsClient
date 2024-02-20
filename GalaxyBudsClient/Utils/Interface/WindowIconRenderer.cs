using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using Bitmap = Avalonia.Media.Imaging.Bitmap;
using Brushes = Avalonia.Media.Brushes;
using Point = Avalonia.Point;

namespace GalaxyBudsClient.Utils.Interface;

public static class WindowIconRenderer
{
    public static void UpdateDynamicIcon(IBasicStatusUpdate status)
    {
        var trayIcons = TrayIcon.GetIcons(Application.Current!);
        if (trayIcons == null) 
            return;

        // Ignore battery level of disconnected earbuds
        if (status.BatteryL <= 0)
            status.BatteryL = status.BatteryR;
        if (status.BatteryR <= 0)
            status.BatteryR = status.BatteryL;
        
        int? level = SettingsProvider.Instance.DynamicTrayIconMode switch
        {
            DynamicTrayIconMode.BatteryMin => Math.Min(status.BatteryL, status.BatteryR),
            DynamicTrayIconMode.BatteryAvg => (status.BatteryL + status.BatteryR) / 2,
            _ => null
        };

        if (level != null)
        {
            trayIcons[0].Icon = MakeFromBatteryLevel(level.Value);
        }
    }

    public static void ResetIconToDefault()
    {
        var trayIcons = TrayIcon.GetIcons(Application.Current!);
        if (trayIcons == null) 
            return;
        
        trayIcons[0].Icon = MakeDefaultIcon();
    }

    private static WindowIcon MakeFromBatteryLevel(int level)
    {
        // Create the formatted text based on the properties set.
        var formattedText = new FormattedText(
            $"{level}",
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            Typeface.Default, 
            200, 
            Brushes.Black // This brush does not matter since we use the geometry of the text.
        );

        // Build the geometry object that represents the text.
        var textGeometry = formattedText.BuildGeometry(new Point(0, 0));

        // Build the geometry object that represents the text highlight.
        var textHighLightGeometry = formattedText.BuildHighlightGeometry(new Point(0, 0));

        var render = new RenderTargetBitmap(new PixelSize(256, 256), new Vector(96, 96));
        
        using (var ctx = render.CreateDrawingContext())
        {
            ctx.PushRenderOptions(new RenderOptions()
            {
                BitmapInterpolationMode = BitmapInterpolationMode.HighQuality,
                TextRenderingMode = TextRenderingMode.Antialias,
                EdgeMode = EdgeMode.Antialias,
                RequiresFullOpacityHandling = true
            });
            
            var outlineColor = PlatformUtils.IsOSX ? Brushes.White : Brushes.Black;
            var fillColor = PlatformUtils.IsOSX ? Brushes.Black : Brushes.White;
            
            ctx.DrawGeometry(Brushes.Transparent, new Pen(outlineColor, 3), textHighLightGeometry!);
            ctx.DrawGeometry(fillColor, new Pen(Brushes.Transparent, 0), textGeometry!);
        }
        
        return new WindowIcon(render);
    }

    public static WindowIcon MakeDefaultIcon()
    {
        return new WindowIcon(MakeDefaultBitmap());
    }

    private static Bitmap MakeDefaultBitmap()
    {
        // OSX uses templated icons
        var uri = $"avares://GalaxyBudsClient/Resources/icon_{(PlatformUtils.IsOSX ? "black" : "white")}_tray.ico";
        return new Bitmap(AssetLoader.Open(new Uri(uri)));
    }
}