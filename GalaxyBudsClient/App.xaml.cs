using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using GalaxyBudsClient.Interface.Developer;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.DynamicLocalization;

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
#if Windows
                ThePBone.Interop.Win32.TrayIcon.ResourceLoader.ClearCache();
#endif
            }
            
            if (Loc.IsTranslatorModeEnabled())
            {
                SettingsProvider.Instance.Locale = Locales.custom;
                new TranslatorTools().Show();
            }
            
            ThemeUtils.Reload();
            Loc.Load();
            DeviceMessageCache.Init();
            UpdateManager.Init();
        }
        
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = MainWindow.Instance;
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