using System.Linq;
using Avalonia;
using Avalonia.Styling;
using FluentAvalonia.Styling;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Utils.Interface;

public static class ThemeUtils
{
    private static readonly FluentAvaloniaTheme FaTheme = (Application.Current?.Styles.Single(x => x is FluentAvaloniaTheme) as FluentAvaloniaTheme)!;
        
    public static void Reload()
    {
        if (Application.Current == null)
        {
            return;
        }

        MainWindow.Instance.RequestedThemeVariant = Settings.Instance.DarkMode switch
        {
            DarkModes.Light => ThemeVariant.Light,
            DarkModes.Dark => ThemeVariant.Dark,
            _ => null
        };

        FaTheme.PreferSystemTheme = Settings.Instance.DarkMode == DarkModes.System;
        ReloadAccentColor();
    }

    public static void ReloadAccentColor()
    {
        var color = Settings.Instance.AccentColor;
        if (color.A == 0)
        {
            color = Settings.Instance.AccentColor = AccentColorParser.DefaultColor;
        }
        FaTheme.CustomAccentColor = color;
    }
}