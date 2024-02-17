using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Firmware;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;

namespace GalaxyBudsClient.Interface.Pages
{
    public class FirmwareTransferPage : AbstractPage
    {
        public override Pages PageType => Pages.FirmwareTransfer;

        private readonly TextBlock _progressText;
        private readonly TextBlock _progressSizeText;
        private readonly ProgressBar _progress;

        public FirmwareTransferPage()
        {   
            AvaloniaXamlLoader.Load(this);

            _progressText = this.GetControl<TextBlock>("ProgressText");
            _progressSizeText = this.GetControl<TextBlock>("ProgressSizeText");
            _progress = this.GetControl<ProgressBar>("Progress");
            
            FirmwareTransferManager.Instance.Finished += OnFinished;
            FirmwareTransferManager.Instance.Error += OnError;
            FirmwareTransferManager.Instance.ProgressChanged += OnProgressChanged;
            FirmwareTransferManager.Instance.StateChanged += OnStateChanged;
            FirmwareTransferManager.Instance.MtuChanged += OnMtuChanged;
            FirmwareTransferManager.Instance.CurrentBlockChanged += OnCurrentBlockChanged;
        }

        private void OnCurrentBlockChanged(object? sender, FirmwareBlockChangedEventArgs e)
        {
            this.GetControl<TextBlock>("StatOffset").Text = string.Format(Loc.Resolve("fw_upload_progress_stats_offset"), e.Offset, e.OffsetEnd);
            this.GetControl<TextBlock>("StatPacket").Text = string.Format(Loc.Resolve("fw_upload_progress_stats_segment"), e.SegmentId, e.SegmentSize, e.SegmentCrc32);
        }

        private void OnMtuChanged(object? sender, short e)
        {
            this.GetControl<TextBlock>("StatMTU").Text = string.Format(Loc.Resolve("fw_upload_progress_stats_mtu"), e);
        }
        
        private void OnStateChanged(object? sender, FirmwareTransferManager.States e)
        {
            switch (e)
            {
                case FirmwareTransferManager.States.InitializingSession:
                    _progressText.Text = Loc.Resolve("fw_upload_progress_session");
                    break;
            }
        }

        private void OnProgressChanged(object? sender, FirmwareProgressEventArgs e)
        {
            _progressText.Text = string.Format(Loc.Resolve("fw_upload_progress"), e.Percent);
            _progressSizeText.Text = string.Format(Loc.Resolve("fw_upload_progress_size"),
                e.CurrentEstimatedByteCount / 1000f, e.TotalByteCount / 1000f);
            _progress.Value = e.Percent;
        }

        private async void OnError(object? sender, FirmwareTransferException e)
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            { 
                _progressText.Text = Loc.Resolve("fw_upload_progress_error");
                
                await new MessageBox()
                {
                    Title = Loc.Resolve("fw_upload_progress_error"),
                    Description = e.ErrorMessage
                }.ShowDialog(MainWindow.Instance);

                await BluetoothImpl.Instance.DisconnectAsync();
            });
        }

        private async void OnFinished(object? sender, EventArgs e)
        {
            _progressText.Text = Loc.Resolve("fw_upload_progress_finished");
            
            await new MessageBox()
            {
                Title = Loc.Resolve("fw_upload_progress_finished"),
                Description = Loc.Resolve("fw_upload_progress_finished_desc")
            }.ShowDialog(MainWindow.Instance);

            await BluetoothImpl.Instance.DisconnectAsync();
        }

        public async void BeginTransfer(FirmwareBinary binary)
        {
            await FirmwareTransferManager.Instance.Install(binary);
        }

        public override void OnPageShown()
        {
            MainWindow.Instance.IsOptionsButtonVisible = false;
            _progressText.Text = Loc.Resolve($"fw_upload_progress_prepare");
            _progressSizeText.Text = string.Empty;
            _progress.Value = 0;
        }

        public override void OnPageHidden()
        {
            MainWindow.Instance.IsOptionsButtonVisible = true;
        }
    }
}