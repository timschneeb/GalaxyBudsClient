#if OSX
using AppKit;
#endif
using System;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using FluentAvalonia.Styling;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Scripting;
using GalaxyBudsClient.Scripting.Experiment;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.Interface;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;
using Serilog;
using Application = Avalonia.Application;

namespace GalaxyBudsClient;

public class App : Application
{
    public FluentAvaloniaTheme FluentTheme => (FluentAvaloniaTheme)Styles.Single(x => x is FluentAvaloniaTheme);
    
    public event Action? TrayIconClicked;
    public static readonly StyledProperty<NativeMenu> TrayMenuProperty =
        AvaloniaProperty.Register<App, NativeMenu>(nameof(TrayMenu),
            defaultBindingMode: BindingMode.OneWay, defaultValue: []);
    public NativeMenu TrayMenu => GetValue(TrayMenuProperty);
    
    public override void Initialize()
    {
        DataContext = this;
            
#if OSX
        NSApplication.Init();
        NSApplication.Notifications.ObserveDidBecomeActive((_, _) =>
        {
            Dispatcher.UIThread.InvokeAsync(delegate
            {
                MainWindow.Instance.BringToFront();
            });
        });
#endif

        AvaloniaXamlLoader.Load(this);
            
        if (Loc.IsTranslatorModeEnabled())
        {
            Settings.Instance.Locale = Locales.custom;
        }
            
        Dispatcher.UIThread.Post(() =>
        { 
            LoadThemeProperties();
            Loc.Load();
        }, DispatcherPriority.Render);
            
        TrayManager.Init();
        MediaKeyRemote.Init();
        DeviceMessageCache.Init();
        ExperimentManager.Init();
        ScriptManager.Instance.RegisterUserHooks();
        
        Settings.Instance.PropertyChanged += OnMainSettingsPropertyChanged;
        
        Log.Information("Translator mode file location: {File}", Loc.GetTranslatorModeFile());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        TrayManager.Init();
            
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = MainWindow.Instance;
            desktop.Exit += (_, _) =>
            {
                Settings.Instance.FirstLaunch = false;
            };
        }
            
        if (Loc.IsTranslatorModeEnabled())
        {
            WindowLauncher.ShowTranslatorTools();
        }
            
        base.OnFrameworkInitializationCompleted();
    }
    
    private void OnMainSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(e.PropertyName is nameof(Settings.Instance.DarkMode) or nameof(Settings.Instance.AccentColor))
        {
            LoadThemeProperties();
        }
    }

    private void LoadThemeProperties()
    {
        FluentTheme.PreferSystemTheme = Settings.Instance.DarkMode == DarkModes.System;
        var color = Settings.Instance.AccentColor;
        if (color.A == 0)
        {
            color = Settings.Instance.AccentColor = AccentColorParser.DefaultColor;
        }
        FluentTheme.CustomAccentColor = color;
    }
        
    private void TrayIcon_OnClicked(object? sender, EventArgs e)
    {
        TrayIconClicked?.Invoke();
    }
}