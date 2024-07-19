using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface;
using GalaxyBudsClient.Interface.Developer;
using GalaxyBudsClient.Platform;

namespace GalaxyBudsClient.Utils.Interface;

public static class WindowLauncher
{
    private static DevTools? _devTools;
    private static TranslatorTools? _translatorTools;
    private static ContentDialog? _devToolsViewDialog;
    private static ContentDialog? _translatorToolsViewDialog;

    public static void ShowDevTools()
    {
        if (PlatformUtils.IsDesktop)
            ShowAsSingleInstanceOnDesktop(ref _devTools);
        else
            ShowAsDialogOnMobile<DevToolsView>(ref _devToolsViewDialog);
    }

    public static void ShowTranslatorTools()
    {
        if (PlatformUtils.IsDesktop)
            ShowAsSingleInstanceOnDesktop(ref _translatorTools);
        else
            ShowAsDialogOnMobile<TranslatorToolsView>(ref _translatorToolsViewDialog);
    }

    public static void ShowAsSingleInstanceOnDesktop<T>(ref T? target) where T : Window, new()
    {
        target ??= new T();
        
        try
        {
            target.Show(MainWindow.Instance);
        }
        catch (InvalidOperationException)
        {
            // Old window object has been closed and cannot be reused 
            target = new T();
            target.Show(MainWindow.Instance);
        }
    }

    private static void ShowAsDialogOnMobile<T>(ref ContentDialog? target) where T : Control, new()
    {
        // Mobile platforms do not support multiple windows, so we use a ContentDialog instead
        if (target == null)
        {
            target = new ContentDialog
            {
                Content = new T(),
                PrimaryButtonText = Strings.WindowClose,
                DefaultButton = ContentDialogButton.Primary
            };

            target.SecondaryButtonClick += (dialog, _) => dialog.Hide();
        }

        _ = target.ShowAsync(TopLevel.GetTopLevel(MainView.Instance));
    }
}