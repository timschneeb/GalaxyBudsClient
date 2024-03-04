using System;
using System.Threading.Tasks;
using System.Timers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.Extensions;
using Application = Avalonia.Application;
using Grid = Avalonia.Controls.Grid;
using Image = Avalonia.Controls.Image;
using Label = Avalonia.Controls.Label;
using Window = Avalonia.Controls.Window;

namespace GalaxyBudsClient.InterfaceOld.Dialogs
{
    public sealed class BudsPopup : Window
    {
        public EventHandler? ClickedEventHandler { get; set; }

        private readonly Border _outerBorder;
        
        private readonly Label _header;
        private readonly Label _batteryL;
        private readonly Label _batteryR;
        private readonly Label _batteryC;
        private readonly Label _caseLabel;

        private readonly Image _iconLeft;
        private readonly Image _iconRight;

        private readonly Timer _timer = new Timer(3000){AutoReset = false};
        
        public BudsPopup() 
        {
            AvaloniaXamlLoader.Load(this);
            this.AttachDevTools();

            _outerBorder = this.GetControl<Border>("OuterBorder");
            
            _header = this.GetControl<Label>("Header");
            _batteryL = this.GetControl<Label>("BatteryL");
            _batteryR = this.GetControl<Label>("BatteryR");
            _batteryC = this.GetControl<Label>("BatteryC");
            _caseLabel = this.GetControl<Label>("CaseLabel");
            
            _iconLeft = this.GetControl<Image>("ImageLeft");
            _iconRight = this.GetControl<Image>("ImageRight");
            
            var cachedStatus = DeviceMessageCache.Instance.BasicStatusUpdate;
            UpdateContent(cachedStatus?.BatteryL ?? 0, cachedStatus?.BatteryR ?? 0, cachedStatus?.BatteryCase ?? 0);
            
            SppMessageHandler.Instance.BaseUpdate += InstanceOnBaseUpdate;
            _timer.Elapsed += (sender, args) =>
            {
                Dispatcher.UIThread.Post(Hide, DispatcherPriority.Render);
            };
        }

        private void InstanceOnBaseUpdate(object? sender, IBasicStatusUpdate e)
        {
            UpdateContent(e.BatteryL, e.BatteryR, e.BatteryCase);
        }

        public void RearmTimer()
        {
            _timer.Stop();
            _timer.Start();
        }
        
        private void UpdateContent(int bl, int br, int bc)
        {
            if (bc > 100)
            {
                bc = DeviceMessageCache.Instance.BasicStatusUpdateWithValidCase?.BatteryCase ?? bc;
            }
            
            _batteryL.Content = $"{bl}%"; 
            _batteryR.Content = $"{br}%";
            _batteryC.Content = $"{bc}%";
            
            bool isLeftOnline = bl > 0;
            bool isRightOnline = br > 0;
            bool isCaseOnline = bc > 0 && bc <= 100 && BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.CaseBattery);
            
            _batteryL.IsVisible = isLeftOnline;
            _batteryR.IsVisible = isRightOnline;
            _batteryC.IsVisible = isCaseOnline;
            _caseLabel.IsVisible = isCaseOnline;
     
            var type = BluetoothImpl.Instance.DeviceSpec.IconResourceKey;

            var leftSourceName = $"Left{type}{(isLeftOnline ? "Connected" : "Disconnected")}";
            _iconLeft.Source = (IImage?)Application.Current?.FindResource(leftSourceName);
            var rightSourceName = $"Right{type}{(isRightOnline ? "Connected" : "Disconnected")}";
            _iconRight.Source = (IImage?)Application.Current?.FindResource(rightSourceName);
        }

        public override void Hide()
        {  
            /* Close window instead */
            _timer.Stop();
            _outerBorder.Tag = "hiding";
            Task.Delay(400).ContinueWith((_) => { Dispatcher.UIThread.InvokeAsync(Close); });
        }

        public override void Show()
        {
            base.Show();
            
            UpdateSettings();
            _outerBorder.Tag = "showing";
            
            _timer.Start();
        }

        private void Window_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            ClickedEventHandler?.Invoke(this, EventArgs.Empty);
            Hide();
        }

        public void UpdateSettings()
        {
            _header.Content = BluetoothImpl.Instance.DeviceName;

            /* Header */
            var grid = this.GetControl<Grid>("Grid");
            if (SettingsProvider.Instance.Popup.Compact)
            {
                MaxHeight = 205 - 35;
                Height = 205 - 35;
                grid.RowDefinitions[1].Height = new GridLength(0);
            }
            else
            {
                MaxHeight = 205;
                Height = 205;
                grid.RowDefinitions[1].Height = new GridLength(35);
            }
            
            /* Window positioning */
            var workArea = (Screens.Primary ?? Screens.All[0]).WorkingArea;
            const int padding = 20;
            
            var scaling = PlatformImpl?.GetPropertyValue<double>("DesktopScaling") ?? 1.0;
            Position = SettingsProvider.Instance.Popup.Placement switch
            {
                PopupPlacement.TopLeft => new PixelPoint(workArea.X + padding, workArea.Y + padding),
                PopupPlacement.TopCenter => new PixelPoint(
                    (int)((workArea.Width / 2f) - (this.Width * scaling / 2) + workArea.X), workArea.Y + padding),
                PopupPlacement.TopRight => new PixelPoint(
                    (int)(workArea.Width - this.Width * scaling + workArea.X - padding), workArea.Y + padding),
                PopupPlacement.BottomLeft => new PixelPoint(workArea.X + padding,
                    (int)(workArea.Height - this.Height * scaling + workArea.Y - padding)),
                PopupPlacement.BottomCenter => new PixelPoint(
                    (int)((workArea.Width / 2f) - (this.Width * scaling / 2) + workArea.X),
                    (int)(workArea.Height - this.Height * scaling + workArea.Y - padding)),
                PopupPlacement.BottomRight => new PixelPoint(
                    (int)((workArea.Width - this.Width * scaling) + workArea.X - padding),
                    (int)(workArea.Height - this.Height * scaling + workArea.Y - padding)),
                _ => Position
            };
        }
    }
}
