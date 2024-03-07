using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Controls.Primitives;
using GalaxyBudsClient.Utils.Interface;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace GalaxyBudsClient.Interface.Services;

public static class DialogHelper
{
    // TODO localization
    public static async Task ShowUnableToOpenLinkDialog(Uri uri)
    {
        var copyLinkButton = new TaskDialogCommand
        {
            Text = "Copy Link",
            IconSource = new SymbolIconSource { Symbol = Symbol.Link },
            Description = uri.ToString(),
            ClosesOnInvoked = false
        };

        var td = new TaskDialog
        {
            Content = "It looks like your platform doesn't support Process.Start " +
                      "and we are unable to open a link.",
            SubHeader = "Oh No!",
            Commands =
            {
                copyLinkButton
            },
            Buttons =
            {
                TaskDialogButton.OKButton
            },
            IconSource = new SymbolIconSource { Symbol = Symbol.Important }
        };

        copyLinkButton.Click += async (s, __) =>
        {
            await ClipboardService.SetTextAsync(uri.ToString());

            var flyout = new Flyout
            {
                Content = "Copied!"
            };

            var comHost = td.FindDescendantOfType<TaskDialogCommandHost>();
            if(comHost == null)
                return;

            FlyoutBase.SetAttachedFlyout(comHost, flyout);
            FlyoutBase.ShowAttachedFlyout(comHost);

            DispatcherTimer.RunOnce(() => flyout.Hide(), TimeSpan.FromSeconds(1));
        };

        var app = Application.Current!.ApplicationLifetime;
        if (app is IClassicDesktopStyleApplicationLifetime desktop)
        {
            td.XamlRoot = desktop.MainWindow;
        }
        else if (app is ISingleViewApplicationLifetime single)
        {
            td.XamlRoot = TopLevel.GetTopLevel(single.MainView);
        }

        await td.ShowAsync(true);
    }
    
    public static async Task ShowDeveloperOptions()
    {
        var root = GetVisualRoot();
        
        var bluetoothInspector = new TaskDialogCommand
        {
            Text = "Bluetooth packet inspector",
            IconSource = new SymbolIconSource { Symbol = Symbol.BluetoothSearching },
            Description = "Inspect and send custom messages via Bluetooth",
            Command = new MiniCommand((_) => { DialogLauncher.ShowDevTools(root as Window); })
        }; 
        
        var translatorMode = new TaskDialogCommand
        {
            Text = "Translation utilities",
            IconSource = new SymbolIconSource { Symbol = Symbol.Translate },
            Description = "Hot-reload XAML dictionaries. Please refer to the wiki on GitHub for instructions.",
            Command = new MiniCommand((_) => { DialogLauncher.ShowTranslatorTools(root as Window); })
        };

        var td = new TaskDialog
        {
            SubHeader = Loc.Resolve("settings_devmode"),
            Commands =
            {
                bluetoothInspector,
                translatorMode
            },
            Buttons = { TaskDialogButton.CloseButton },
            IconSource = new SymbolIconSource { Symbol = Symbol.WindowDevTools },
            XamlRoot = root
        };

        await td.ShowAsync(true);
    }

    private static Visual? GetVisualRoot() =>
        Application.Current!.ApplicationLifetime switch
        {
            IClassicDesktopStyleApplicationLifetime desktop => desktop.MainWindow,
            ISingleViewApplicationLifetime single => TopLevel.GetTopLevel(single.MainView),
            _ => (Visual?)null
        };
}
