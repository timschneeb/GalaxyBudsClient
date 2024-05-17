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
using Avalonia.Media;
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
using Serilog;
using Application = Avalonia.Application;
using MainWindow = GalaxyBudsClient.Interface.MainWindow;

namespace GalaxyBudsClient;

public class App : Application
{
    public FluentAvaloniaTheme FluentTheme => (FluentAvaloniaTheme)Styles.Single(x => x is FluentAvaloniaTheme);
    
    public event Action? TrayIconClicked;
    public static readonly StyledProperty<NativeMenu> TrayMenuProperty =
        AvaloniaProperty.Register<App, NativeMenu>(nameof(TrayMenu),
            defaultBindingMode: BindingMode.OneWay, defaultValue: []);
    public NativeMenu TrayMenu => GetValue(TrayMenuProperty);
    
    private readonly ExperimentManager _experimentManager = new();
    
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
            
        if (Loc.IsTranslatorModeEnabled)
        {
            Settings.Data.Locale = Locales.custom;
        }
            
        Dispatcher.UIThread.Post(() =>
        { 
            LoadThemeProperties();
            Loc.Load();
        }, DispatcherPriority.Render);
            
        TrayManager.Init();
        DeviceMessageCache.Init();
        BatteryHistoryManager.Init();
        ScriptManager.Instance.RegisterUserHooks();
        
        Settings.MainSettingsPropertyChanged += OnMainSettingsPropertyChanged;
        
        Log.Information("Translator mode file location: {File}", Loc.TranslatorModeFile);
        Log.Debug("Environment: {Env}", _experimentManager.CurrentEnvironment());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        TrayManager.Init();
            
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = MainWindow.Instance;
        }
            
        if (Loc.IsTranslatorModeEnabled)
        {
            WindowLauncher.ShowTranslatorTools();
        }
            
        base.OnFrameworkInitializationCompleted();
    }
    
    private void OnMainSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(Settings.Data.Theme) or nameof(Settings.Data.AccentColor):
                LoadThemeProperties();
                break;
            case nameof(Settings.Data.Locale):
                Loc.Load();
                break;
            case nameof(Settings.Data.DynamicTrayIconMode):
            {
                var cache = DeviceMessageCache.Instance.BasicStatusUpdate;
                if (Settings.Data.DynamicTrayIconMode != DynamicTrayIconModes.Disabled && BluetoothImpl.Instance.IsConnected && cache != null)
                    WindowIconRenderer.UpdateDynamicIcon(cache);
                else
                    WindowIconRenderer.ResetIconToDefault();
                break;
            }
        }
    }

    private void LoadThemeProperties()
    {
        FluentTheme.PreferSystemTheme = Settings.Data.Theme == Themes.System;
        var color = Settings.Data.AccentColor;
        if(Color.FromUInt32(color).A == 0)
        {
            color = Settings.Data.AccentColor = Colors.Orange.ToUInt32();
        }
        FluentTheme.CustomAccentColor = Color.FromUInt32(color);
    }
        
    private void TrayIcon_OnClicked(object? sender, EventArgs e)
    {
        TrayIconClicked?.Invoke();
    }
}