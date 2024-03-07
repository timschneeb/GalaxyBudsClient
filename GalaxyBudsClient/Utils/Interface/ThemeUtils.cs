using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using FluentAvalonia.Styling;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using Serilog;

namespace GalaxyBudsClient.Utils.Interface
{
    public static class ThemeUtils
    {
        private static readonly FluentAvaloniaTheme FaTheme = (Application.Current?.Styles.Single(x => x is FluentAvaloniaTheme) as FluentAvaloniaTheme)!;
        
        public static void Reload()
        {
            if (Application.Current == null)
            {
                return;
            }
            
            // TODO do not load light/dark resources this way. it does not regard the system theme settings 
            if (SettingsProvider.Instance.DarkMode == DarkModes.Light)
            {
                MainWindow2.Instance.RequestedThemeVariant = ThemeVariant.Light;
                
                SetBrushSource("Brushes");
            }
            else if (SettingsProvider.Instance.DarkMode == DarkModes.Dark)
            {
                MainWindow2.Instance.RequestedThemeVariant = ThemeVariant.Dark;
                SetBrushSource("BrushesDark");
            }
            else
            {
                MainWindow2.Instance.RequestedThemeVariant = null;
            }

            FaTheme.PreferSystemTheme = SettingsProvider.Instance.DarkMode == DarkModes.System;
            ReloadAccentColor();
        }

        public static void ReloadAccentColor()
        {
            var color = SettingsProvider.Instance.AccentColor;
            if (color.A == 0)
            {
                color = SettingsProvider.Instance.AccentColor = AccentColorParser.DefaultColor;
            }
            FaTheme.CustomAccentColor = color;
        }
        
        [Obsolete("TODO: get rid of custom brushes; except for some images maybe?")]
        private static void SetBrushSource(string name)
        {
            if (Application.Current == null)
            {
                return;
            }
            
            try
            {
                var dictId = ResourceIndexer.Find("Brushes-");
                if (dictId == -1)
                {
                    Log.Error("ThemeUtils: No active brushes resource found. Cannot switch themes");
                }
                else
                {
                    Application.Current.Resources.MergedDictionaries[dictId] =
                        new ResourceInclude((Uri?)null)
                        {
                            Source = new Uri($"{Program.AvaresUrl}/InterfaceOld/Styles/{name}.xaml")
                        };
                }
            }
            catch (IOException e)
            {
                Log.Error("Localization: IOError while loading locales. Details: {EMessage}", e.Message);
            }
        }
    }
}