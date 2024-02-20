using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace GalaxyBudsClient.Utils.Interface;

public static class TrayIconFactory
{
    public static WindowIcon MakeFromBatteryLevel(int level)
    {
        var control = new TextBlock()
        {
            Width = 256,
            Height = 256,
            Text = $"{level}%",
            FontSize = 150,
            Foreground = Brushes.WhiteSmoke
        };

        return new WindowIcon(RenderToBitmap(control));
    }

    private static Bitmap RenderToBitmap(Control target)
    {
        var pixelSize = new PixelSize((int) target.Width, (int) target.Height);
        var size = new Size(target.Width, target.Height);
        var bitmap = new RenderTargetBitmap(pixelSize, new Vector(96, 96));
        target.Measure(size);
        target.Arrange(new Rect(size));
        bitmap.Render(target);
        return bitmap;
    }

    public static WindowIcon MakeDefaultIcon()
    {
        return new WindowIcon(
            new Bitmap(AssetLoader.Open(new Uri("avares://GalaxyBudsClient/Resources/icon_white_tray.ico"))));
    }
}