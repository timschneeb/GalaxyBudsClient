using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using GalaxyBudsClient.Platform;
using Bitmap = Avalonia.Media.Imaging.Bitmap;
using Brushes = Avalonia.Media.Brushes;
using Point = Avalonia.Point;
using Size = Avalonia.Size;

namespace GalaxyBudsClient.Utils.Interface;

public static class TrayIconFactory
{
    public static WindowIcon MakeFromBatteryLevel(int level)
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
            //ctx.DrawImage(MakeDefaultBitmap(), new Rect(0, 0, 256, 256));
            ctx.DrawGeometry(Brushes.Transparent, new Pen(Brushes.Black, 3), textHighLightGeometry);
            ctx.DrawGeometry(Brushes.White, new Pen(Brushes.Transparent, 0), textGeometry);
        }
        
        return new WindowIcon(render);
    }

    private static Bitmap RenderToBitmap(int width, int height, params Control[] targets)
    {
        var pixelSize = new PixelSize(width, height);
        var size = new Size(width, height);
        var bitmap = new RenderTargetBitmap(pixelSize, new Vector(96, 96));
        foreach (var target in targets)
        {
            target.Measure(size);
            target.Arrange(new Rect(size));
            bitmap.Render(target);
        }
        return bitmap;
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