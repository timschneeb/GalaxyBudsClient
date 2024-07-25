using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface;
using GalaxyBudsClient.Interface.Developer;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Platform;
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace GalaxyBudsClient.Utils.Interface;

public static class Dialogs
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
        if(!PlatformUtils.IsDesktop)
            return;
        
        target ??= new T();
        
        try
        {
            target.Show();
        }
        catch (InvalidOperationException)
        {
            // Old window object has been closed and cannot be reused 
            target = new T();
            target.Show();
        }
    }

    public static async Task<bool> RequireFullVersion(string? customTitle = null)
    {
#if RequestFullVersion
        if (PlatformUtils.IsDesktop)
            return true;

        var res = await new QuestionBox()
        {
            ButtonText = "Open on Google Play",
            CloseButtonText = "Close",
            Title = customTitle ?? "Not available in demo",
            Description = "Advanced features are not available in the demo version of this app.\n\n" +
                          "Please support me and download the full version from Google Play to use this feature."
        }.ShowAsync();
        
        if (res)
        {
            PlatformImpl.DesktopServices.OpenUri("https://play.google.com/store/apps/details?id=me.timschneeberger.galaxybudsclient");
        }
        return false;
#else
        return true;
#endif
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