using System;
#if OSX
using AppKit;
#endif
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GalaxyBudsClient.InterfaceOld.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Scripting;
using GalaxyBudsClient.Scripting.Experiment;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.Interface;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;
using Serilog;
using Application = Avalonia.Application;

namespace GalaxyBudsClient
{
    public class App : Application
    {
        public override void Initialize()
        {
            DataContext = this;
            
#if OSX
            NSApplication.Init();
            NSApplication.Notifications.ObserveDidBecomeActive((_, _) =>
            {
                Dispatcher.UIThread.InvokeAsync(delegate
                {
                    MainWindow2.Instance.BringToFront();
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
                ThemeUtils.Reload();
                Loc.Load();
            }, DispatcherPriority.Render);
            
            TrayManager.Init();
            MediaKeyRemoteImpl.Init();
            DeviceMessageCache.Init();
            ExperimentManager.Init();
           
            Log.Information("Translator mode file location: {File}", Loc.GetTranslatorModeFile());

            ScriptManager.Instance.RegisterUserHooks();
        }
        
        public override void OnFrameworkInitializationCompleted()
        {
            TrayManager.Init();
            
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = MainWindow2.Instance;
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

        public event Action? TrayIconClicked;

        public static readonly StyledProperty<NativeMenu> TrayMenuProperty =
            AvaloniaProperty.Register<App, NativeMenu>(nameof(TrayMenu),
                defaultBindingMode: BindingMode.OneWay, defaultValue: []);
        public NativeMenu TrayMenu => GetValue(TrayMenuProperty);
        
        private void TrayIcon_OnClicked(object? sender, EventArgs e)
        {
            TrayIconClicked?.Invoke();
        }
    }
}