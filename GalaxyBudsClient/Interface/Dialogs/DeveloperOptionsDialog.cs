using System;
using System.Collections.Generic;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Utils.Interface;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace GalaxyBudsClient.Interface.Dialogs;

public class DeveloperOptionsDialog : TaskDialog
{
    public DeveloperOptionsDialog()
    {
        SubHeader = Strings.SettingsDevmode;
        Commands = new List<TaskDialogCommand> {
            new()
            {
                Text = "Bluetooth packet inspector",
                IconSource = new SymbolIconSource { Symbol = Symbol.BluetoothSearching },
                Description = "Inspect and send custom messages via Bluetooth",
                Command = new MiniCommand(_ => { WindowLauncher.ShowDevTools(MainWindow.Instance); })
            },
            new()
            {
                Text = "Translation utilities",
                IconSource = new SymbolIconSource { Symbol = Symbol.Translate },
                Description = "Hot-reload XAML dictionaries. Please refer to the wiki on GitHub for instructions.",
                Command = new MiniCommand(_ => { WindowLauncher.ShowTranslatorTools(MainWindow.Instance); })
            }
        };
        Buttons = new List<TaskDialogButton> { TaskDialogButton.CloseButton };
        IconSource = new SymbolIconSource { Symbol = Symbol.WindowDevTools };
        XamlRoot = MainWindow.Instance;
    }

    protected override Type StyleKeyOverride => typeof(TaskDialog);
}