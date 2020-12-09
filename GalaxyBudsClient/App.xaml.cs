using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using GalaxyBudsClient.Interface.Developer;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.DynamicLocalization;

namespace GalaxyBudsClient
{
    public class App : Application
    {
        public static Styles FluentDark = new Styles
        {
            new StyleInclude(new Uri("avares://ControlCatalog/Styles"))
            {
                Source = new Uri("avares://Avalonia.Themes.Fluent/Accents/FluentDark.xaml")
            },
        };

        public static Styles FluentLight = new Styles
        {
            new StyleInclude(new Uri("avares://ControlCatalog/Styles"))
            {
                Source = new Uri("avares://Avalonia.Themes.Fluent/Accents/FluentLight.xaml")
            },
        };

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            
            if (Loc.IsTranslatorModeEnabled())
            {
                SettingsProvider.Instance.Locale = Locales.custom;
                new TranslatorTools().Show();
            }
            
            Loc.Load();
            DeviceMessageCache.Init();
        }
        
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = MainWindow.Instance;
            }
            
            base.OnFrameworkInitializationCompleted();
        }
    }
}