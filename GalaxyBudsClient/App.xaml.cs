using System;
using System.Linq;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GalaxyBudsClient.Interface.Developer;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Scripting;
using GalaxyBudsClient.Scripting.Experiment;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.DynamicLocalization;
using Serilog;
using Application = Avalonia.Application;
using Environment = System.Environment;

namespace GalaxyBudsClient
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            
            /* Clean everything from the old run up */
            if (PlatformUtils.IsWindows)
            {
#if WindowsNoARM
                ThePBone.Interop.Win32.TrayIcon.ResourceLoader.ClearCache();
#endif
            }
            
            if (Loc.IsTranslatorModeEnabled())
            {
                SettingsProvider.Instance.Locale = Locales.custom;
            }
            
            ThemeUtils.Reload();
            Loc.Load();
            
            MediaKeyRemoteImpl.Init();
            DeviceMessageCache.Init();
            UpdateManager.Init();
            ExperimentManager.Init();
           
            Log.Information($"Translator mode file location: {Loc.GetTranslatorModeFile()}");

            ScriptManager.Instance.RegisterUserHooks();
        }
        
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = MainWindow.Instance;
                desktop.Exit += (sender, args) =>
                {
                    SettingsProvider.Instance.FirstLaunch = false;
                    NotifyIconImpl.Shutdown();
                };
            }
            
            if (Loc.IsTranslatorModeEnabled())
            {
                DialogLauncher.ShowTranslatorTools();
            }
            
            base.OnFrameworkInitializationCompleted();
        }

        public void RestartApp(AbstractPage.Pages target)
        {

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                MainWindow.Instance.DisableApplicationExit = true;
                MainWindow.Instance.OverrideMinimizeTray = true;
                MainWindow.Instance.Close();
                MainWindow.Kill();

                ThemeUtils.Reload();

                desktop.MainWindow = MainWindow.Instance;
                desktop.MainWindow.Show();
                
                MainWindow.Instance.Pager.SwitchPage(target);
                
                /* Restore crucial information */
                SPPMessageHandler.Instance.DispatchEvent(DeviceMessageCache.Instance.ExtendedStatusUpdate);
                SPPMessageHandler.Instance.DispatchEvent(DeviceMessageCache.Instance.StatusUpdate);
            }
        }
    }
}