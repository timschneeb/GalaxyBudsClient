using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using FluentIcons.Avalonia;
using FluentIcons.Common;

namespace GalaxyBudsClient.Utils.Interface;

/// <summary>
/// Renders Fluent icons into bitmaps for native tray menu items.
/// macOS scales menu item images to the menu font height and keeps the aspect ratio,
/// so extra transparent width on the left is used to indent items.
/// </summary>
internal static class TrayMenuIcons
{
    // Rendered at ~2x the final menu icon height for sharp results on Retina displays
    private const int Height = 36;
    private const int IndentWidth = 22;

    private static readonly Dictionary<string, Bitmap> Cache = new();
    private static bool? _cachedDarkMode;
    private static Color? _cachedAccent;

    private static bool IsDarkMode =>
        Application.Current?.PlatformSettings?.GetColorValues().ThemeVariant == PlatformThemeVariant.Dark;

    private static Color Accent =>
        Application.Current?.PlatformSettings?.GetColorValues().AccentColor1 ?? Color.FromRgb(0x0A, 0x84, 0xFF);

    private static Color LabelColor => IsDarkMode
        ? Color.FromArgb(0xD9, 0xFF, 0xFF, 0xFF)
        : Color.FromArgb(0xD9, 0x00, 0x00, 0x00);

    private static Color InactiveCircleColor => IsDarkMode
        ? Color.FromArgb(0x33, 0xFF, 0xFF, 0xFF)
        : Color.FromArgb(0x1A, 0x00, 0x00, 0x00);

    /// <summary>
    /// Plain glyph in the system label color
    /// </summary>
    public static Bitmap Glyph(Symbol symbol)
    {
        return GetOrCreate($"glyph:{symbol}", () =>
        {
            var bitmap = new RenderTargetBitmap(new PixelSize(Height, Height));
            using var ctx = bitmap.CreateDrawingContext();
            DrawSymbol(ctx, symbol, IconVariant.Regular, LabelColor, new Rect(2, 2, Height - 4, Height - 4));
            return bitmap;
        });
    }

    /// <summary>
    /// Indented glyph inside a circle; filled with the accent color when selected
    /// </summary>
    public static Bitmap Option(Symbol symbol, bool selected)
    {
        return GetOrCreate($"option:{symbol}:{selected}", () =>
        {
            var bitmap = new RenderTargetBitmap(new PixelSize(IndentWidth + Height, Height));
            using var ctx = bitmap.CreateDrawingContext();
            var circle = new Rect(IndentWidth, 0, Height, Height);
            ctx.DrawEllipse(new SolidColorBrush(selected ? Accent : InactiveCircleColor), null, circle);
            DrawSymbol(ctx, symbol, selected ? IconVariant.Filled : IconVariant.Regular,
                selected ? Colors.White : LabelColor, circle.Deflate(8));
            return bitmap;
        });
    }

    private static void DrawSymbol(DrawingContext ctx, Symbol symbol, IconVariant variant, Color color, Rect dest)
    {
        var image = new SymbolImage
        {
            Symbol = symbol,
            IconVariant = variant,
            FontSize = dest.Height,
            Foreground = new SolidColorBrush(color)
        };
        image.Draw(ctx, new Rect(image.Size), dest);
    }

    private static Bitmap GetOrCreate(string key, System.Func<Bitmap> factory)
    {
        // Drop cached icons when the system appearance changes
        var dark = IsDarkMode;
        var accent = Accent;
        if (_cachedDarkMode != dark || _cachedAccent != accent)
        {
            Cache.Clear();
            _cachedDarkMode = dark;
            _cachedAccent = accent;
        }

        if (!Cache.TryGetValue(key, out var bitmap))
        {
            bitmap = factory();
            Cache[key] = bitmap;
        }
        return bitmap;
    }
}
