using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Extensions;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;
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
        _closeButton = new TaskDialogButton(Loc.Resolve("cancel"), TaskDialogStandardResult.Close);

        Header = Loc.Resolve("coredump_header");
        Buttons = [_closeButton];
        IconSource = new SymbolIconSource { Symbol = Symbol.Bug };
        SubHeader = Loc.Resolve("coredump_dl_progress_prepare");
        XamlRoot = MainWindow.Instance;
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
            SubHeader = Loc.Resolve("coredump_dl_progress_finished");
            SetProgressBarState(100, TaskDialogProgressState.Normal);

            var path = await MainWindow.Instance.OpenFolderPickerAsync(
                Loc.Resolve("coredump_dl_save_dialog_title"));
            if (string.IsNullOrEmpty(path))
                return;

            ev.CoreDumpPaths.ForEach(x => CopyDump(x, path));
            ev.TraceDumpPaths.ForEach(x => CopyDump(x, path));

            await new MessageBox
            {
                Title = Loc.Resolve("coredump_dl_save_success_title"),
                Description = string.Format(Loc.Resolve("coredump_dl_save_success"),
                    ev.CoreDumpPaths.Count + ev.TraceDumpPaths.Count, path)
            }.ShowAsync();
        }, DispatcherPriority.Normal);
    }

    private void OnDisconnected(object? o, string s)
    {
        Dispatcher.UIThread.Post(() =>
        {
            SubHeader = Loc.Resolve("connlost_disconnected");
            SetProgressBarState(100, TaskDialogProgressState.Error);
            _content.Text = string.Empty;
            _closeButton.Text = Loc.Resolve("window_close");
        }, DispatcherPriority.Normal);
    }
        
    private void OnProgressUpdated(object? s, LogDownloadProgressEventArgs ev)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (ev.DownloadType == LogDownloadProgressEventArgs.Type.Switching)
            {
                SubHeader = Loc.Resolve("coredump_dl_progress_prepare");
                SetProgressBarState(0, TaskDialogProgressState.Indeterminate);
                _content.Text = string.Empty;
            }
            else
            {
                SubHeader = string.Format(Loc.Resolve($"coredump_dl_{(ev.DownloadType == LogDownloadProgressEventArgs.Type.Coredump ? "coredump" : "trace")}_progress"), 
                    Math.Ceiling((double)ev.CurrentByteCount / ev.TotalByteCount * 100));
                _content.Text = string.Format(
                        Loc.Resolve("coredump_dl_progress_size"),
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
                Title = Loc.Resolve("coredump_dl_save_fail_title"),
                Description = string.Format(Loc.Resolve("coredump_dl_save_fail"), ex.Message)
            }.ShowAsync();
        }
    }
    
}