using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace GalaxyBudsClient.Interface.Dialogs;

public class SelfTestDialog : TaskDialog
{
    private readonly TextBlock _content;
    private readonly CancellationTokenSource _cancelToken = new();

    public SelfTestDialog()
    {
        var closeButton = new TaskDialogButton(Loc.Resolve("cancel"), TaskDialogStandardResult.Close);
        
        Header = Loc.Resolve("selftest_header");
        Buttons = [closeButton];
        IconSource = new SymbolIconSource { Symbol = Symbol.Beaker };
        Content = _content = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            Text = Loc.Resolve("system_waiting_for_device"),
            MaxWidth = 600
        };
        XamlRoot = MainWindow.Instance;
        ShowProgressBar = true;
        
        SetProgressBarState(100, TaskDialogProgressState.Indeterminate);
    }

    protected override Type StyleKeyOverride => typeof(TaskDialog);

    public async Task ExecuteTestAsync()
    {
        try
        {
            _ = ShowAsync(true).ContinueWith(a =>
            {
                if (a.Result as TaskDialogStandardResult? == TaskDialogStandardResult.Close)
                    _cancelToken.Cancel();
            }, _cancelToken.Token);

            SppMessageHandler.Instance.SelfTestResponse += OnSelfTestResponse;
            await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.SELF_TEST);

            // Wait for 10 seconds for the self test response
            await Task.Delay(10000, _cancelToken.Token);
            
            // If the self test response was not received, show an error message
            OnSelfTestResponse(null, null);
        }
        catch (TaskCanceledException) {}
    }
    
    protected override void OnClosing(TaskDialogClosingEventArgs args)
    {
        SppMessageHandler.Instance.SelfTestResponse -= OnSelfTestResponse;
        base.OnClosing(args);
    }
    
    private static TaskDialogCommand ResultWithSideAsTaskItem(string testKey, bool lPass, bool rPass)
    {
        return new TaskDialogCommand
        {
            Text = Loc.Resolve(testKey),
            IconSource = new SymbolIconSource
            {
                Symbol = !lPass || !rPass ? Symbol.Warning : Symbol.Checkmark
            },
            Description = $"{Loc.Resolve("left")}: {Loc.Resolve(lPass ? "selftest_pass" : "selftest_fail")}, " +
                          $"{Loc.Resolve("right")}: {Loc.Resolve(rPass ? "selftest_pass" : "selftest_fail")}",
            ClosesOnInvoked = false
        };
    }
        
    private static TaskDialogCommand ResultAsTaskItem(string testKey, bool pass)
    {
        return new TaskDialogCommand
        {
            Text = Loc.Resolve(testKey),
            IconSource = new SymbolIconSource
            {
                Symbol = !pass ? Symbol.Warning : Symbol.Checkmark
            },
            Description = Loc.Resolve(pass ? "selftest_pass" : "selftest_fail"),
            ClosesOnInvoked = false
        };
    }
    
    private void OnSelfTestResponse(object? s, SelfTestParser? parser)
    {
        _cancelToken.Cancel();
        Buttons[0].Text = Loc.Resolve("window_close");

        if (parser == null)
        {
            SetProgressBarState(100, TaskDialogProgressState.Error);
            _content.Text = Loc.Resolve("system_no_response");
        }
        else
        {
            // Open a new dialog to display the results because Commands can't be updated after the dialog is shown
            _ = new TaskDialog
            {
                Header = Loc.Resolve(parser is not { AllChecks: true } ? "selftest_fail_long" : "selftest_pass_long"),
                Buttons = Buttons,
                IconSource = IconSource,
                XamlRoot = MainWindow.Instance,
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
            Hide();
        }
    }
}