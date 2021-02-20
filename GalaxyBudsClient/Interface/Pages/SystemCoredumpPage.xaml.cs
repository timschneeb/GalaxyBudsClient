using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.DynamicLocalization;
using NetSparkleUpdater;
using NetSparkleUpdater.Events;
using Serilog;

namespace GalaxyBudsClient.Interface.Pages
{
    public class SystemCoredumpPage : AbstractPage
    {
        public override Pages PageType => Pages.SystemCoredump;

        private readonly TextBlock _progressText;
        private readonly TextBlock _progressSizeText;
        private readonly ProgressBar _progress;

        public SystemCoredumpPage()
        {   
            AvaloniaXamlLoader.Load(this);

            _progressText = this.FindControl<TextBlock>("ProgressText");
            _progressSizeText = this.FindControl<TextBlock>("ProgressSizeText");
            _progress = this.FindControl<ProgressBar>("Progress");
            
            DeviceLogManager.Instance.ProgressUpdated += OnProgressUpdated;
            DeviceLogManager.Instance.Finished += OnFinished;
        }

        private void OnFinished(object? sender, LogDownloadFinishedEventArgs e)
        {
            Dispatcher.UIThread.Post(async () =>
            {
                _progressText.Text = Loc.Resolve("coredump_dl_progress_finished");

                OpenFolderDialog dlg = new OpenFolderDialog()
                {
                    Title = Loc.Resolve("coredump_dl_save_dialog_title")
                };
                string? path = await dlg.ShowAsync(MainWindow.Instance);
    
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }

                e.CoreDumpPaths.ForEach(x => CopyFile(x, path));
                e.TraceDumpPaths.ForEach(x => CopyFile(x, path));
        
                await new MessageBox()
                {
                    Title = Loc.Resolve("coredump_dl_save_success_title"),
                    Description = string.Format(Loc.Resolve("coredump_dl_save_success"), e.CoreDumpPaths.Count + e.TraceDumpPaths.Count, path)
                }.ShowDialog(MainWindow.Instance);
            });
        }

        private void OnProgressUpdated(object? sender, LogDownloadProgressEventArgs e)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (e.DownloadType == LogDownloadProgressEventArgs.Type._Switching)
                {
                    _progressText.Text = Loc.Resolve($"coredump_dl_progress_prepare");
                    _progressSizeText.Text = string.Empty;
                    _progress.Value = 0;
                }
                else
                {
                    _progressText.Text = string.Format(Loc.Resolve($"coredump_dl_{(e.DownloadType == LogDownloadProgressEventArgs.Type.Coredump ? "coredump" : "trace")}_progress"), Math.Ceiling((double)e.CurrentByteCount / e.TotalByteCount * 100));
                    _progressSizeText.Text = string.Format(Loc.Resolve("coredump_dl_progress_size"), e.CurrentByteCount.ToString(), e.TotalByteCount.ToString());
                    _progress.Value = Math.Ceiling(((double)e.CurrentByteCount / e.TotalByteCount) * 100);
                }
            });
        }

        public override async void OnPageShown()
        {
            _progressText.Text = Loc.Resolve($"coredump_dl_progress_prepare");
            _progressSizeText.Text = string.Empty;
            _progress.Value = 0;
            await Task.Delay(1000);
            await DeviceLogManager.Instance.BeginDownloadAsync();
        }
        
        private async void Cancel_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            await DeviceLogManager.Instance.CancelDownload();
            MainWindow.Instance.Pager.SwitchPage(Pages.System);
        }
        
        private void CopyFile(string source, string targetDir)
        {
            try
            {
                File.Copy(source, Path.Combine(targetDir, Path.GetFileName(source)));
            }
            catch (Exception ex)
            {
                Log.Error($"CoredumpPage: Failed to copy dumps: {ex}");
                new MessageBox()
                {
                    Title = Loc.Resolve("coredump_dl_save_fail_title"),
                    Description = string.Format(Loc.Resolve("coredump_dl_save_fail"), ex.Message)
                }.ShowDialog(MainWindow.Instance);
            }
        }
    }
}