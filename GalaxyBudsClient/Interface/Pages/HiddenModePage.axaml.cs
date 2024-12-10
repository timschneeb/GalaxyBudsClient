using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.MarkupExtensions;
using GalaxyBudsClient.Interface.ViewModels.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Message.Encoder;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
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

    /// <summary>
    /// Prompt the user to change a string value. Receives the current value from the device and sends the new value.
    /// </summary>
    /// <param name="title">Title</param>
    /// <param name="cmdIdSet">Command id of the setter which accepts a parameter</param>
    /// <param name="cmdIdGet">Command id of the getter</param>
    /// <param name="atCommandPrefix">Format: '+SERIALNO:' Used to filter & identify relevant responses from the earbuds</param>
    /// <param name="length">Expected input length</param>
    /// <param name="useDidSpecialCase">Use special input mode for DIDs</param>
    private static async Task PromptValueChangeAsync(string title, string cmdIdSet, string cmdIdGet,
        string atCommandPrefix, int? length, bool useDidSpecialCase = false)
    {
        ComboBox? header = null;

        // DID: add preset selector
        if (useDidSpecialCase)
        {
            header = new ComboBox
            {
                HorizontalAlignment = HorizontalAlignment.Stretch, 
                ItemsSource = (IEnumerable?)new DeviceIdsBindingSource().ProvideValue(null!)
            };
        }

        var dialog = new InputDialog(header) { Title = title };

        if (header != null)
        {
            header.SelectionChanged += (_, _) =>
            {
                if (header.SelectedValue is DeviceIds id)
                {
                    dialog.Text = ((int)id).ToString("X6");
                }
            };
        }

        SppMessageReceiver.Instance.HiddenCmdData += OnHiddenCmdDataReceived;
        await BluetoothImpl.Instance.SendAsync(new HiddenCmdDataEncoder
        {
            CommandId = cmdIdGet
        });

        var result = await dialog.OpenDialogAsync(TopLevel.GetTopLevel(MainView.Instance));

        if (result != null && length != null && result.Length != length)
        {
            await new MessageBox
            {
                Title = Strings.Error,
                Description = string.Format(Strings.HiddenModeInputWrongLength, length)
            }.ShowAsync();
        }
        else if (result != null)
        {
            await BluetoothImpl.Instance.SendAsync(new HiddenCmdDataEncoder
            {
                CommandId = cmdIdSet,
                Parameter = result
            });
        }

        SppMessageReceiver.Instance.HiddenCmdData -= OnHiddenCmdDataReceived;
        return;

        void OnHiddenCmdDataReceived(object? _, HiddenCmdDataDecoder dec)
        {
            // atCommandPrefix could be "+SERIALNO:"
            if (!dec.Content.Contains(atCommandPrefix + "1")) // 1 = response with value
                return;

            // Example input: R+SERIALNO:1,RFAR11JXXXX\n\nOK\n
            var msg = dec.Content
                          .Split(',').ElementAtOrDefault(1)?
                          .Split('\n', StringSplitOptions.TrimEntries).FirstOrDefault()
                      ?? "<unknown>";

            if (useDidSpecialCase)
            {
                var cleanMsg = msg.Replace("0x", "00");
                dialog.Text = cleanMsg;
                header!.SelectedValue =
                    header.ItemsSource?.Cast<DeviceIds>().FirstOrDefault(x => ((int)x).ToString("X6") == cleanMsg);
            }
            else
            {
                dialog.Text = msg;
            }
        }
    }
    
    private async void OnChangeSerialClicked(object? sender, RoutedEventArgs e)
    {
        await PromptValueChangeAsync(
            title: Strings.HiddenModeSerialChange, 
            cmdIdSet: "000B", cmdIdGet: "000C", atCommandPrefix: "+SERIALNO:", 11);
    }

    private async void OnChangeDidClicked(object? sender, RoutedEventArgs e)
    {
        await PromptValueChangeAsync(
            title: Strings.HiddenModeDidChange, 
            cmdIdSet: "00AD", cmdIdGet: "00AE", atCommandPrefix: "+DEVICEID:", 6, true);
    }
    
    private async void OnChangeSkuClicked(object? sender, RoutedEventArgs e)
    {
        await PromptValueChangeAsync(
            title: Strings.HiddenModeSkuChange, 
            cmdIdSet: "007D", cmdIdGet: "007E", atCommandPrefix: "+SKUCODEC:", 14);
    }

    private async void OnChangePeerAddressClicked(object? sender, RoutedEventArgs e)
    {
        await PromptValueChangeAsync(
            title: Strings.HiddenModeBtPeerAddrChange, 
            cmdIdSet: "0030", cmdIdGet: "0031", atCommandPrefix: "+BTIDTEST:", 12);
    }

    private async void OnChangeLocalAddressClicked(object? sender, RoutedEventArgs e)
    {
        await PromptValueChangeAsync(
            title: Strings.HiddenModeBtLocalAddrChange, 
            cmdIdSet: "0047", cmdIdGet: "0048", atCommandPrefix: "+BTIDTEST:", 12);
    }
}