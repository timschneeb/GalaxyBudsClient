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
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.DynamicLocalization;
using Application = Avalonia.Application;
using Grid = Avalonia.Controls.Grid;
using Image = Avalonia.Controls.Image;
using Label = Avalonia.Controls.Label;
using Window = Avalonia.Controls.Window;

namespace GalaxyBudsClient.Interface.Dialogs
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

            _outerBorder = this.FindControl<Border>("OuterBorder");
            
            _header = this.FindControl<Label>("Header");
            _batteryL = this.FindControl<Label>("BatteryL");
            _batteryR = this.FindControl<Label>("BatteryR");
            _batteryC = this.FindControl<Label>("BatteryC");
            _caseLabel = this.FindControl<Label>("CaseLabel");
            
            _iconLeft = this.FindControl<Image>("ImageLeft");
            _iconRight = this.FindControl<Image>("ImageRight");
            
            var cachedStatus = DeviceMessageCache.Instance.BasicStatusUpdate;
            UpdateContent(cachedStatus?.BatteryL ?? 0, cachedStatus?.BatteryR ?? 0, cachedStatus?.BatteryCase ?? 0);
            
            SPPMessageHandler.Instance.BaseUpdate += InstanceOnBaseUpdate;
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
            _batteryL.Content = $"{bl}%"; 
            _batteryR.Content = $"{br}%";
            _batteryC.Content = $"{bc}%";
            
            bool isLeftOnline = bl > 0;
            bool isRightOnline = br > 0;
            bool isCaseOnline = bc > 0 && BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.CaseBattery);
            
            _batteryL.IsVisible = isLeftOnline;
            _batteryR.IsVisible = isRightOnline;
            _batteryC.IsVisible = isCaseOnline;
            _caseLabel.IsVisible = isCaseOnline;

            string type = BluetoothImpl.Instance.ActiveModel == Models.BudsLive ? "Bean" : "Bud";

            if (isLeftOnline)
            {
                _iconLeft.Source = (IImage?)Application.Current.FindResource($"Left{type}Connected");
            }
            else
            {
                _iconLeft.Source = (IImage?)Application.Current.FindResource($"Left{type}Disconnected");
            }

            if (isRightOnline)
            {
                _iconRight.Source = (IImage?)Application.Current.FindResource($"Right{type}Connected");
            }
            else
            {
                _iconRight.Source = (IImage?)Application.Current.FindResource($"Right{type}Disconnected");
            }
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
            Activate();
            Focus();
            
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
            /* Load strings */
            string modifier = string.Empty;
            if (BluetoothImpl.Instance.ActiveModel == Models.BudsPlus) modifier = "+";
            else if (BluetoothImpl.Instance.ActiveModel == Models.BudsLive) modifier = " Live";
            else if (BluetoothImpl.Instance.ActiveModel == Models.BudsPro) modifier = " Pro";

            string name = Environment.UserName.Split(' ')[0];

            string title = SettingsProvider.Instance.Popup.CustomTitle == string.Empty
                ? Loc.Resolve("connpopup_title")
                : SettingsProvider.Instance.Popup.CustomTitle;

            _header.Content = string.Format(title, name, modifier);
            
            /* Header */
            var grid = this.FindControl<Grid>("Grid");
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
            int padding = 20;
            var scaling = PlatformImpl.DesktopScaling;
            switch (SettingsProvider.Instance.Popup.Placement)
            {
                case PopupPlacement.TopLeft:
                    Position = new PixelPoint(workArea.X + padding, workArea.Y + padding);
                    break;
                case PopupPlacement.TopCenter:
                    Position = new PixelPoint((int) ((workArea.Width / 2f) - (this.Width  * scaling / 2) + workArea.X), workArea.Y + padding);
                    break;
                case PopupPlacement.TopRight:
                    Position = new PixelPoint((int) (workArea.Width - this.Width  * scaling + workArea.X - padding), workArea.Y + padding);
                    break;
                case PopupPlacement.BottomLeft:
                    Position = new PixelPoint(workArea.X + padding, (int) (workArea.Height - this.Height * scaling + workArea.Y - padding));
                    break;
                case PopupPlacement.BottomCenter:
                    Position = new PixelPoint((int) ((workArea.Width / 2f) - (this.Width * scaling / 2) + workArea.X), (int) (workArea.Height - this.Height * scaling + workArea.Y - padding));
                    break;
                case PopupPlacement.BottomRight:
                    Position = new PixelPoint((int) ((workArea.Width - this.Width * scaling) + workArea.X - padding), (int) (workArea.Height - this.Height * scaling + workArea.Y - padding));
                    break;
            }
        }
    }
}