using System;
using System.IO;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Utils.DynamicLocalization;
using Serilog;

namespace GalaxyBudsClient.Utils
{
    public static class ThemeUtils
    {
        public static Styles FluentDark = new Styles
        {
            new StyleInclude(new Uri("avares://ControlCatalog/Styles"))
            {
                Source = new Uri("avares://Avalonia.Themes.Fluent/FluentDark.xaml")
            },
        };

        public static Styles FluentLight = new Styles
        {
            new StyleInclude(new Uri("avares://ControlCatalog/Styles"))
            {
                Source = new Uri("avares://Avalonia.Themes.Fluent/FluentLight.xaml")
            },
        };

        public static void Reload()
        {
            if (SettingsProvider.Instance.DarkMode == DarkModes.Light)
            {
                Application.Current.Styles[1] = FluentLight;
                SetBrushSource("Brushes");
            }
            else
            {
                Application.Current.Styles[1] = FluentDark;
                SetBrushSource("BrushesDark");
            }
        }

        private static void SetBrushSource(string name)
        {
            try
            {
                int dictId = ResourceIndexer.Find("Brushes-");

                if (dictId == -1)
                {
                    Log.Error("ThemeUtils: No active brushes resource found. Cannot switch themes.");
                }
                else
                {
                    Application.Current.Resources.MergedDictionaries[dictId] =
                        new ResourceInclude {Source = new Uri($"avares://GalaxyBudsClient/Interface/Styles/{name}.xaml")};
                }
            }
            catch (IOException e)
            {
                Log.Error($"Localization: IOError while loading locales. Details: {e.Message}");
            }
        }
    }
}