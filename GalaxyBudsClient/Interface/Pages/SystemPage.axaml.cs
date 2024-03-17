using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.Services;
using GalaxyBudsClient.Interface.ViewModels.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;
using Symbol = FluentIcons.Common.Symbol;

namespace GalaxyBudsClient.Interface.Pages;

public partial class SystemPage : BasePage<SystemPageViewModel>
{
    public SystemPage()
    {
        InitializeComponent();
        
        
        AddHandler(SettingsExpanderItem.ClickEvent, OnSettingsItemClicked);
    }

    private void OnSettingsItemClicked(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not SettingsExpanderItem item)
            return;
        
        NavigationService.Instance.Navigate(item.Name);
    }
    
    /* TODO re-add disclaimer for FW flasher 
       if (!Settings.Instance.FirmwareWarningAccepted)
       {
           var result = await new QuestionBox()
           {
               Title = Loc.Resolve("fw_disclaimer"),
               Description = Loc.Resolve("fw_disclaimer_desc"),
               MinWidth = 600,
               MaxWidth = 600
           }.ShowDialog<bool>(MainWindow.Instance);

           Settings.Instance.FirmwareWarningAccepted = result;
           if (result)
           {
               MainWindow.Instance.Pager.SwitchPage(Pages.FirmwareSelect);
           }
       }
       else
       {
           MainWindow.Instance.Pager.SwitchPage(Pages.FirmwareSelect);
       }
     */

    private async void OnFactoryResetClicked(object? sender, RoutedEventArgs e)
    {
        var cd = new ContentDialog
        {
            Title = Loc.Resolve("factory_header"),
            Content = Loc.Resolve("factory_confirmation"),
            PrimaryButtonText = Loc.Resolve("continue_button"),
            SecondaryButtonText = Loc.Resolve("cancel"),
            DefaultButton = ContentDialogButton.Primary,
        };

        cd.PrimaryButtonClick += OnFactoryResetConfirmed;
        cd.SecondaryButtonClick += (_, _) => cd.Hide();
        await cd.ShowAsync(MainWindow2.Instance);
        cd.PrimaryButtonClick -= OnFactoryResetConfirmed;
    }

    private async void OnFactoryResetConfirmed(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var cancelToken = new CancellationTokenSource();
        sender.Title = Loc.Resolve("system_waiting_for_device");
        sender.IsPrimaryButtonEnabled = false;
        sender.IsSecondaryButtonEnabled = false;
        
        var defer = args.GetDeferral();
        SppMessageHandler.Instance.ResetResponse += OnResetResponse;
        await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.RESET);
        
        // Wait for 10 seconds for the reset response
        await Task.Delay(10000, cancelToken.Token);
        
        SppMessageHandler.Instance.ResetResponse -= OnResetResponse;
        // If the reset response was not received, show an error message
        OnResetResponse(null, -1);
        return;
        
        void OnResetResponse(object? s, int code)
        {
            cancelToken.Cancel();
            defer.Complete();
            
            if (code != 0)
            {
                var info = code == -1 ? Loc.Resolve("system_no_response") : code.ToString();
                
                _ = new MessageBox
                {
                    Title = Loc.Resolve("factory_error_title"),
                    Description = Loc.Resolve("factory_error").Replace("{0}", info)
                }.ShowAsync(MainWindow2.Instance);
                return;
            }

            // TODO re-enter setup wizard?
            BluetoothImpl.Instance.UnregisterDevice();
        }
    }

    private async void OnPairingModeClicked(object? sender, RoutedEventArgs e)
    {
        await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.UNK_PAIRING_MODE);
        await BluetoothImpl.Instance.DisconnectAsync();
        await new MessageBox
        {
            Title = Loc.Resolve("connlost_disconnected"),
            Description = Loc.Resolve("pairingmode_done")
        }.ShowAsync();
    }

    private void OnTraceDumpDownloadClicked(object? sender, RoutedEventArgs e)
    {
        // TODO
    }

    private async void OnSelfTestClicked(object? sender, RoutedEventArgs e)
    {
        var closeButton = new TaskDialogButton(Loc.Resolve("cancel"), TaskDialogStandardResult.Close);
        var cancelToken = new CancellationTokenSource();

        var td = new TaskDialog
        {
            Header = Loc.Resolve("selftest_header"),
            Buttons = [closeButton],
            IconSource = new FluentIcons.Avalonia.Fluent.SymbolIconSource() { Symbol = Symbol.Beaker },
            Content = new TextBlock()
            {
                TextWrapping = TextWrapping.Wrap,
                Text = Loc.Resolve("system_waiting_for_device"),
                MaxWidth = 600
            },
            XamlRoot = MainWindow2.Instance,
            ShowProgressBar = true,
        };
            
        try
        {
            td.SetProgressBarState(100, TaskDialogProgressState.Indeterminate);
            _ = td.ShowAsync(true).ContinueWith(a =>
            {
                if (a.Result as TaskDialogStandardResult? == TaskDialogStandardResult.Close)
                    cancelToken.Cancel();
            }, cancelToken.Token);

            SppMessageHandler.Instance.SelfTestResponse += OnSelfTestResponse;
            await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.SELF_TEST);

            // Wait for 10 seconds for the self test response
            await Task.Delay(10000, cancelToken.Token);
        }
        catch (TaskCanceledException) {}
      
        SppMessageHandler.Instance.SelfTestResponse -= OnSelfTestResponse;

        // If the self test response was not received, show an error message
        if (td.IsVisible)
            OnSelfTestResponse(null, null);
        return;

        TaskDialogCommand ResultWithSideAsTaskItem(string testKey, bool lPass, bool rPass)
        {
            return new TaskDialogCommand
            {
                Text = Loc.Resolve(testKey),
                IconSource = new FluentIcons.Avalonia.Fluent.SymbolIconSource
                {
                    Symbol = !lPass || !rPass ? Symbol.Warning : Symbol.Checkmark
                },
                Description = $"{Loc.Resolve("left")}: {Loc.Resolve(lPass ? "selftest_pass" : "selftest_fail")}, " +
                              $"{Loc.Resolve("right")}: {Loc.Resolve(rPass ? "selftest_pass" : "selftest_fail")}",
                ClosesOnInvoked = false
            };
        }
        
        TaskDialogCommand ResultAsTaskItem(string testKey, bool pass)
        {
            return new TaskDialogCommand
            {
                Text = Loc.Resolve(testKey),
                IconSource = new FluentIcons.Avalonia.Fluent.SymbolIconSource
                {
                    Symbol = !pass ? Symbol.Warning : Symbol.Checkmark
                },
                Description = Loc.Resolve(pass ? "selftest_pass" : "selftest_fail"),
                ClosesOnInvoked = false
            };
        }
        
        void OnSelfTestResponse(object? s, SelfTestParser? parser)
        {
            cancelToken.Cancel();
            td.Buttons[0].Text = Loc.Resolve("window_close");

            if (parser == null)
            {
                td.SetProgressBarState(100, TaskDialogProgressState.Error);
                ((TextBlock)td.Content).Text = Loc.Resolve("system_no_response");
            }
            else
            {
                _ = new TaskDialog
                {
                    Header = Loc.Resolve(parser is not { AllChecks: true } ? "selftest_fail_long" : "selftest_pass_long"),
                    Buttons = td.Buttons,
                    IconSource = td.IconSource,
                    XamlRoot = MainWindow2.Instance,
                    Commands = new[]
                    {
                        ResultAsTaskItem("system_hwver", parser.HardwareVersion),
                        ResultAsTaskItem("system_swver", parser.SoftwareVersion),
                        ResultAsTaskItem("system_touchver", parser.TouchFirmwareVersion),
                        ResultWithSideAsTaskItem("system_btaddr", parser.LeftBluetoothAddress, parser.RightBluetoothAddress),
                        ResultWithSideAsTaskItem("system_proximity", parser.LeftProximity, parser.RightProximity),
                        ResultWithSideAsTaskItem("system_thermo", parser.LeftThermistor, parser.RightThermistor),
                        ResultWithSideAsTaskItem("system_adc_soc", parser.LeftAdcSOC, parser.RightAdcSOC),
                        ResultWithSideAsTaskItem("system_adc_voltage", parser.LeftAdcVCell, parser.RightAdcVCell),
                        ResultWithSideAsTaskItem("system_adc_current", parser.LeftAdcCurrent, parser.RightAdcCurrent),
                        ResultWithSideAsTaskItem("system_hall", parser.LeftHall, parser.RightHall),
                        ResultWithSideAsTaskItem("system_accel", parser.AllLeftAccelerator, parser.AllRightAccelerator)
                    }
                }.ShowAsync(true);
                td.Hide();
            }
        }
    }
}