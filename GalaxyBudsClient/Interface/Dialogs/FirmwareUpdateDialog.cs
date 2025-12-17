using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Firmware;
using GalaxyBudsClient.Platform;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace GalaxyBudsClient.Interface.Dialogs;

public class FirmwareUpdateDialog : TaskDialog
{
    private readonly TextBlock _progressSize;
    private readonly TextBlock _offset;
    private readonly TextBlock _packet;
    private readonly TextBlock _mtu;

    public FirmwareUpdateDialog()
    {
        var closeButton = new TaskDialogButton(Strings.Cancel, TaskDialogStandardResult.Close);
        _progressSize = new TextBlock { Text = Strings.FwUploadProgressPrepare, TextWrapping = TextWrapping.Wrap };
        _offset = new TextBlock { Text = string.Empty, TextWrapping = TextWrapping.Wrap };
        _packet = new TextBlock { Text = string.Empty, TextWrapping = TextWrapping.Wrap };
        _mtu = new TextBlock { Text = string.Empty, TextWrapping = TextWrapping.Wrap };
        
        Header = Strings.FwUploadHeader;
        Buttons = [closeButton];
        IconSource = new SymbolIconSource { Symbol = Symbol.ArrowDownload };
        Footer = new StackPanel
        {
            Margin = new Thickness(0, 4, 0, 0),
            Spacing = 1, 
            Children = { _progressSize, _offset, _packet, _mtu }
        };
        FooterVisibility = TaskDialogFooterVisibility.Auto;
        XamlRoot = TopLevel.GetTopLevel(MainView.Instance);
        ShowProgressBar = true;
        
        FirmwareTransferManager.Instance.Finished += OnFinished;
        FirmwareTransferManager.Instance.Error += OnError;
        FirmwareTransferManager.Instance.ProgressChanged += OnProgressChanged;
        FirmwareTransferManager.Instance.StateChanged += OnStateChanged;
        FirmwareTransferManager.Instance.MtuChanged += OnMtuChanged;
        FirmwareTransferManager.Instance.CurrentBlockChanged += OnCurrentBlockChanged;
    }

    protected override Type StyleKeyOverride => typeof(TaskDialog);

    protected override void OnClosing(TaskDialogClosingEventArgs args)
    {
        FirmwareTransferManager.Instance.Finished -= OnFinished;
        FirmwareTransferManager.Instance.Error -= OnError;
        FirmwareTransferManager.Instance.ProgressChanged -= OnProgressChanged;
        FirmwareTransferManager.Instance.StateChanged -= OnStateChanged;
        FirmwareTransferManager.Instance.MtuChanged -= OnMtuChanged;
        FirmwareTransferManager.Instance.CurrentBlockChanged -= OnCurrentBlockChanged;
        
        _ = FirmwareTransferManager.Instance.CancelAsync();
        base.OnClosing(args);
    }

    private void OnCurrentBlockChanged(object? sender, FirmwareBlockChangedEventArgs e)
    {
        _offset.Text = string.Format(Strings.FwUploadProgressStatsOffset, e.Offset, e.OffsetEnd);
        _packet.Text = string.Format(Strings.FwUploadProgressStatsSegment, e.SegmentId, e.SegmentSize, e.SegmentCrc32);
    }

    private void OnMtuChanged(object? sender, short e)
    {
        _mtu.Text = string.Format(Strings.FwUploadProgressStatsMtu, e);
    }

    private void OnStateChanged(object? sender, FirmwareTransferManager.States e)
    {
        switch (e)
        {
            case FirmwareTransferManager.States.InitializingSession:
                SubHeader = Strings.FwUploadProgressSession;
                SetProgressBarState(0, TaskDialogProgressState.Indeterminate);
                break;
        }
    }

    private void OnProgressChanged(object? sender, FirmwareProgressEventArgs e)
    {
        SetProgressBarState(e.Percent, TaskDialogProgressState.Normal);
        
        SubHeader = string.Format(Strings.FwUploadProgress, e.Percent);
        _progressSize.Text = string.Format(Strings.FwUploadProgressSize,
                (e.CurrentEstimatedByteCount / 1000f).ToString("F1"),
                (e.TotalByteCount / 1000f).ToString("F1"))
            .Replace("(", "")
            .Replace(")", "");
    }

    private async void OnError(object? sender, FirmwareTransferException e)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Hide();
            
            _ = new MessageBox
            {
                Title = Strings.FwUploadProgressError,
                Description = e.ErrorMessage
            }.ShowAsync();

            _ = BluetoothImpl.Instance.DisconnectAsync();
        });
    }

    private void OnFinished(object? sender, EventArgs e)
    {
        Hide();
        
        _ = new MessageBox
        {
            Title = Strings.FwUploadProgressFinished,
            Description = Strings.FwUploadProgressFinishedDesc
        }.ShowAsync();

        _ = BluetoothImpl.Instance.DisconnectAsync();
    }

    public async Task BeginTransfer(FirmwareBinary binary)
    {
        await FirmwareTransferManager.Instance.Install(binary);
    }
}