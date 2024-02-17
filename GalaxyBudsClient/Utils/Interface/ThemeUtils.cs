using System;
using System.IO;
using System.Reflection;
using Avalonia;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using GalaxyBudsClient.Model.Constants;
using Serilog;

namespace GalaxyBudsClient.Utils.Interface
{
    public static class ThemeUtils
    {
        public static event EventHandler<DarkModes>? ThemeReloaded; 
        
        public static void Reload()
        {
            if (Application.Current == null)
            {
                return;
            }
            
            if (SettingsProvider.Instance.DarkMode == DarkModes.Light)
            {
                MainWindow.Instance.RequestedThemeVariant = ThemeVariant.Light;
                SetBrushSource("Brushes");
            }
            else
            {
                MainWindow.Instance.RequestedThemeVariant = ThemeVariant.Dark;
                SetBrushSource("BrushesDark");
            }
            
            ThemeReloaded?.Invoke(nameof(ThemeUtils), SettingsProvider.Instance.DarkMode);
        }

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
                    Log.Error("ThemeUtils: No active brushes resource found. Cannot switch themes.");
                }
                else
                {
                    var assemblyName = Assembly.GetEntryAssembly()?.GetName().Name;
                    Application.Current.Resources.MergedDictionaries[dictId] =
                        new ResourceInclude((Uri?)null)
                        {
                            Source = new Uri($"avares://{assemblyName}/Interface/Styles/{name}.xaml")
                        };
                }
            }
            catch (IOException e)
            {
                Log.Error($"Localization: IOError while loading locales. Details: {e.Message}");
            }
        }
    }
}