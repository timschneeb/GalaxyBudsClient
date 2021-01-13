using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.DynamicLocalization;
using NetSparkleUpdater;
using NetSparkleUpdater.Events;
using Serilog;
using Trinet.Core.IO.Ntfs;

namespace GalaxyBudsClient.Interface.Pages
{
 	public class UpdateProgressPage : AbstractPage
	{
		public override Pages PageType => Pages.UpdateProgress;

        private TextBlock _progressText;
        private TextBlock _progressSizeText;
        private ProgressBar _progress;

        public UpdateProgressPage()
		{   
			AvaloniaXamlLoader.Load(this);

            _progressText = this.FindControl<TextBlock>("ProgressText");
            _progressSizeText = this.FindControl<TextBlock>("ProgressSizeText");
            _progress = this.FindControl<ProgressBar>("Progress");

            UpdateManager.Instance.Core.DownloadCanceled += OnDownloadCanceled;
            UpdateManager.Instance.Core.DownloadFinished += OnDownloadFinished;
            UpdateManager.Instance.Core.DownloadHadError += OnDownloadHadError;
            UpdateManager.Instance.Core.DownloadMadeProgress += OnDownloadMadeProgress;
        }

        private void OnDownloadMadeProgress(object sender, AppCastItem item, ItemDownloadProgressEventArgs args)
        {
            _progressText.Text = string.Format(Loc.Resolve("updater_dl_progress"), args.ProgressPercentage);
            _progressSizeText.Text = string.Format(Loc.Resolve("updater_dl_progress_size"),  (args.BytesReceived / 1000000f).ToString("N2"), (args.TotalBytesToReceive / 1000000f).ToString("N2"));
            _progress.Value = args.ProgressPercentage;
        }

        private void OnDownloadHadError(AppCastItem item, string path, Exception exception)
        {
            MainWindow.Instance.Pager.SwitchPage(Pages.UpdateAvailable);
            new MessageBox
            {
                Title = Loc.Resolve("updater_dl_fail_title"),
                Description = Loc.Resolve("updater_dl_fail") + exception.Message
            }.ShowDialog(MainWindow.Instance);
        }

        private async void OnDownloadFinished(AppCastItem item, string path)
        {
            _progressText.Text = Loc.Resolve("updater_dl_progress_finished");

            try
            {
                new FileInfo(path).DeleteAlternateDataStream("Zone.Identifier");
               
                Process proc = new Process
                {
                    StartInfo =
                    {
                        FileName = path,
                        UseShellExecute = true,
                        Verb = "runas"
                    }
                };
                proc.Start();
            }
            catch (Exception exception)
            {
                Log.Warning("UpdateProgressPage: Exception raised while launching installer: " + exception.Message);
            }

            if (BluetoothImpl.Instance.IsConnected)
            {
                await BluetoothImpl.Instance.DisconnectAsync();
            }

            await Task.Delay(100);
            
            Environment.Exit(0);
        }

        private void OnDownloadCanceled(AppCastItem item, string path)
        {
             Log.Information("UpdateProgressPage: Update cancellation complete");
        }

        public void BeginUpdate(AppCastItem update)
		{
            MainWindow.Instance.Pager.SwitchPage(Pages.UpdateProgress);

            _progressText.Text = Loc.Resolve("updater_dl_progress_prepare");
            _progressSizeText.Text = string.Empty;
            _progress.Value = 0;
            _progress.Minimum = 0;
            _progress.Maximum = 100;
            UpdateManager.Instance.Core.InitAndBeginDownload(update);
        }
        private void Cancel_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            _progressText.Text = Loc.Resolve("updater_dl_progress_cancelled");
            UpdateManager.Instance.Core.CancelFileDownload();
            MainWindow.Instance.Pager.SwitchPage(Pages.UpdateAvailable);
        }

        private void ManualDownload_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            OpenWebsite("https://github.com/ThePBone/GalaxyBudsClient/releases");
        }

        private void OpenWebsite(String url)
        {
            var psi = new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            };
            Process.Start(psi);
        }
	}
}
