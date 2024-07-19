using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Platform;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace GalaxyBudsClient.Interface.Dialogs;

public class SelfTestDialog : TaskDialog
{
    private readonly TextBlock _content;
    private readonly CancellationTokenSource _cancelToken = new();

    public SelfTestDialog()
    {
        var closeButton = new TaskDialogButton(Strings.Cancel, TaskDialogStandardResult.Close);
        
        Header = Strings.SelftestHeader;
        Buttons = [closeButton];
        IconSource = new SymbolIconSource { Symbol = Symbol.Beaker };
        Content = _content = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            Text = Strings.SystemWaitingForDevice,
            MaxWidth = 600
        };
        XamlRoot = TopLevel.GetTopLevel(MainView.Instance);
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

            SppMessageReceiver.Instance.SelfTestResponse += OnSelfTestResponse;
            await BluetoothImpl.Instance.SendRequestAsync(MsgIds.SELF_TEST);

            // Wait for 10 seconds for the self test response
            await Task.Delay(10000, _cancelToken.Token);
            
            // If the self test response was not received, show an error message
            OnSelfTestResponse(null, null);
        }
        catch (TaskCanceledException) {}
    }
    
    protected override void OnClosing(TaskDialogClosingEventArgs args)
    {
        SppMessageReceiver.Instance.SelfTestResponse -= OnSelfTestResponse;
        base.OnClosing(args);
    }
    
    private static TaskDialogCommand ResultWithSideAsTaskItem(string test, bool lPass, bool rPass)
    {
        return new TaskDialogCommand
        {
            Text = test,
            IconSource = new SymbolIconSource
            {
                Symbol = !lPass || !rPass ? Symbol.Warning : Symbol.Checkmark
            },
            Description = $"{Strings.Left}: {(lPass ? Strings.SelftestPass : Strings.SelftestFail)}, " +
                          $"{Strings.Right}: {(rPass ? Strings.SelftestPass : Strings.SelftestFail)}",
            ClosesOnInvoked = false
        };
    }
        
    private static TaskDialogCommand ResultAsTaskItem(string test, bool pass)
    {
        return new TaskDialogCommand
        {
            Text = test,
            IconSource = new SymbolIconSource
            {
                Symbol = !pass ? Symbol.Warning : Symbol.Checkmark
            },
            Description = pass ? Strings.SelftestPass : Strings.SelftestFail,
            ClosesOnInvoked = false
        };
    }
    
    private void OnSelfTestResponse(object? s, SelfTestDecoder? parser)
    {
        _cancelToken.Cancel();
        Buttons[0].Text = Strings.WindowClose;

        if (parser == null)
        {
            SetProgressBarState(100, TaskDialogProgressState.Error);
            _content.Text = Strings.SystemNoResponse;
        }
        else
        {
            // Open a new dialog to display the results because Commands can't be updated after the dialog is shown
            _ = new TaskDialog
            {
                Header = parser is not { AllChecks: true } ? Strings.SelftestFailLong : Strings.SelftestPassLong,
                Buttons = Buttons,
                IconSource = IconSource,
                XamlRoot = TopLevel.GetTopLevel(MainView.Instance),
                Commands = new[]
                {
                    ResultAsTaskItem(Strings.SystemHwver, parser.HardwareVersion),
                    ResultAsTaskItem(Strings.SystemSwver, parser.SoftwareVersion),
                    ResultAsTaskItem(Strings.SystemTouchver, parser.TouchFirmwareVersion),
                    ResultWithSideAsTaskItem(Strings.SystemBtaddr, parser.LeftBluetoothAddress, parser.RightBluetoothAddress),
                    ResultWithSideAsTaskItem(Strings.SystemProximity, parser.LeftProximity, parser.RightProximity),
                    ResultWithSideAsTaskItem(Strings.SystemThermo, parser.LeftThermistor, parser.RightThermistor),
                    ResultWithSideAsTaskItem(Strings.SystemAdcSoc, parser.LeftAdcSoc, parser.RightAdcSoc),
                    ResultWithSideAsTaskItem(Strings.SystemAdcVoltage, parser.LeftAdcVCell, parser.RightAdcVCell),
                    ResultWithSideAsTaskItem(Strings.SystemAdcCurrent, parser.LeftAdcCurrent, parser.RightAdcCurrent),
                    ResultWithSideAsTaskItem(Strings.SystemHall, parser.LeftHall, parser.RightHall),
                    ResultWithSideAsTaskItem(Strings.SystemAccel, parser.AllLeftAccelerator, parser.AllRightAccelerator)
                }
            }.ShowAsync(true);
            Hide();
        }
    }
}