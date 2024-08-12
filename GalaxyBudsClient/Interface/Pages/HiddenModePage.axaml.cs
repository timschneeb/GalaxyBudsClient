using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.ViewModels.Pages;
using GalaxyBudsClient.Message.Encoder;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Platform;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace GalaxyBudsClient.Interface.Pages;

public partial class HiddenModePage : BasePage<HiddenModePageViewModel>
{
    public HiddenModePage()
    {
        InitializeComponent();
    }

    private void OnHiddenTerminalClicked(object? sender, RoutedEventArgs e)
    {
        _ = HiddenModeTerminalDialog.OpenDialogAsync();
    }

    private void OnPowerMenuClicked(object? sender, RoutedEventArgs e)
    {
        _ = new TaskDialog {
            Header = Strings.HiddenModePower,
            SubHeader = Strings.PowerMenuHint,
                
            Commands = new List<TaskDialogCommand> {
                new()
                {
                    Text = Strings.PowerMenuShutdown,
                    IconSource = new SymbolIconSource { Symbol = Symbol.Power },
                    Command = new MiniCommand(o =>
                    {
                        _ = BluetoothImpl.Instance.SendAsync(new HiddenCmdDataEncoder
                        {
                            CommandId = "005B" // TWU_AT_CMD_ID_POWER_OFF
                        });
                    })
                },
                new()
                {
                    Text = Strings.PowerMenuReboot,
                    IconSource = new SymbolIconSource { Symbol = Symbol.ArrowClockwise },
                    Command = new MiniCommand(o =>
                    {
                        _ = BluetoothImpl.Instance.SendAsync(new HiddenCmdDataEncoder
                        {
                            CommandId = "9999" // SWREBOOT
                        });
                    })
                }, 
                new()
                {
                    Text = Strings.PowerMenuSleep,
                    IconSource = new SymbolIconSource { Symbol = Symbol.WeatherMoon },
                    Command = new MiniCommand(o =>
                    {
                        _ = BluetoothImpl.Instance.SendAsync(new HiddenCmdDataEncoder
                        {
                            CommandId = "004A" // TWU_AT_CMD_ID_SLEEP_MODE_SET
                        });
                    })
                },
            },
            Buttons = new List<TaskDialogButton> { TaskDialogButton.CloseButton },
            IconSource = new SymbolIconSource { Symbol = Symbol.Power },
            XamlRoot = TopLevel.GetTopLevel(MainView.Instance),
        }.ShowAsync(true);
    }
}