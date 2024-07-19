using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Extensions;
using Serilog;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace GalaxyBudsClient.Interface.Dialogs;

public class TraceDownloaderDialog : TaskDialog
{
    private readonly TextBlock _content;
    private readonly TaskDialogButton _closeButton;

    private readonly DeviceLogManager _deviceLogManager = new();
    
    public TraceDownloaderDialog()
    {
        _closeButton = new TaskDialogButton(Strings.Cancel, TaskDialogStandardResult.Close);

        Header = Strings.CoredumpHeader;
        Buttons = [_closeButton];
        IconSource = new SymbolIconSource { Symbol = Symbol.Bug };
        SubHeader = Strings.CoredumpDlProgressPrepare;
        XamlRoot = TopLevel.GetTopLevel(MainView.Instance);
        ShowProgressBar = true;
        Content = _content = new TextBlock
        {
            Text = string.Empty,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 600
        };
            
        SetProgressBarState(0, TaskDialogProgressState.Indeterminate);

        _deviceLogManager.ProgressUpdated += OnProgressUpdated;
        _deviceLogManager.Finished += OnFinished;
        BluetoothImpl.Instance.Disconnected += OnDisconnected;
    }

    protected override Type StyleKeyOverride => typeof(TaskDialog);
    
    protected override async void OnClosing(TaskDialogClosingEventArgs args)
    {
        BluetoothImpl.Instance.Disconnected -= OnDisconnected;
        _deviceLogManager.Finished -= OnFinished;
        _deviceLogManager.ProgressUpdated -= OnProgressUpdated;
        
        await _deviceLogManager.CancelDownload();
        base.OnClosing(args);
    }
    
    public async Task BeginDownload()
    {
        await _deviceLogManager.BeginDownloadAsync();
        await ShowAsync(true);
    }

    private void OnFinished(object? s, LogDownloadFinishedEventArgs ev)
    {
        _ = Dispatcher.UIThread.InvokeAsync(async () =>
        {
            SubHeader = Strings.CoredumpDlProgressFinished;
            SetProgressBarState(100, TaskDialogProgressState.Normal);

            var path = await TopLevel.GetTopLevel(MainView.Instance)!.OpenFolderPickerAsync(Strings.CoredumpDlSaveDialogTitle);
            if (string.IsNullOrEmpty(path))
                return;

            ev.CoreDumpPaths.ForEach(x => CopyDump(x, path));
            ev.TraceDumpPaths.ForEach(x => CopyDump(x, path));

            await new MessageBox
            {
                Title = Strings.CoredumpDlSaveSuccessTitle,
                Description = string.Format(Strings.CoredumpDlSaveSuccess,
                    ev.CoreDumpPaths.Count + ev.TraceDumpPaths.Count, path)
            }.ShowAsync();
        }, DispatcherPriority.Normal);
    }

    private void OnDisconnected(object? o, string s)
    {
        Dispatcher.UIThread.Post(() =>
        {
            SubHeader = Strings.ConnlostDisconnected;
            SetProgressBarState(100, TaskDialogProgressState.Error);
            _content.Text = string.Empty;
            _closeButton.Text = Strings.WindowClose;
        }, DispatcherPriority.Normal);
    }
        
    private void OnProgressUpdated(object? s, LogDownloadProgressEventArgs ev)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (ev.DownloadType == LogDownloadProgressEventArgs.Type.Switching)
            {
                SubHeader = Strings.CoredumpDlProgressPrepare;
                SetProgressBarState(0, TaskDialogProgressState.Indeterminate);
                _content.Text = string.Empty;
            }
            else
            {
                SubHeader = string.Format(ev.DownloadType == LogDownloadProgressEventArgs.Type.Coredump ? Strings.CoredumpDlCoreProgress : Strings.CoredumpDlTraceProgress, 
                    Math.Ceiling((double)ev.CurrentByteCount / ev.TotalByteCount * 100));
                _content.Text = string.Format(
                        Strings.CoredumpDlProgressSize,
                        ev.CurrentByteCount.ToString(),
                        ev.TotalByteCount.ToString())
                    .Replace("(", "")
                    .Replace(")", "");
                SetProgressBarState(Math.Ceiling((double)ev.CurrentByteCount / ev.TotalByteCount * 100), TaskDialogProgressState.Normal);
            }
        }, DispatcherPriority.Normal);
    }
        
    private static async void CopyDump(string source, string targetDir)
    {
        try
        {
            File.Copy(source, Path.Combine(targetDir, Path.GetFileName(source)));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "CoreDump: Failed to copy dumps");
            await new MessageBox
            {
                Title = Strings.CoredumpDlSaveFailTitle,
                Description = string.Format(Strings.CoredumpDlSaveFail, ex.Message)
            }.ShowAsync();
        }
    }
    
}