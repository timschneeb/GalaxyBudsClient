using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using GalaxyBudsClient.Decoder;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.DynamicLocalization;
using Serilog;

namespace GalaxyBudsClient.Interface.Dialogs
{
    public sealed class BudsPopup : Window
    {
        private bool _isHeaderHidden;
        public bool HideHeader
        {
            get => _isHeaderHidden;
            set
            {
                _isHeaderHidden = value;
                if (value)
                {
                    Height = 205 - 35;
                    //HeaderRow.Height = new GridLength(0);
                }
                else
                {
                    Height = 205;
                    //HeaderRow.Height = new GridLength(35);
                }
            }
        }

        public EventHandler? ClickedEventHandler { get; set; }
        
        private readonly Label _header;
        private readonly Label _batteryL;
        private readonly Label _batteryR;
        private readonly Label _batteryC;
        private readonly Label _caseLabel;

        private readonly Image _iconLeft;
        private readonly Image _iconRight;

        public BudsPopup() : this(Models.Buds, 0, 0, 0)
        {
            Log.Warning("BudsPopup: instantiated with no arguments");
        }

        public BudsPopup(Models model, int left, int right, int box)
        {
            AvaloniaXamlLoader.Load(this);
            this.AttachDevTools();

            _header = this.FindControl<Label>("Header");
            _batteryL = this.FindControl<Label>("BatteryL");
            _batteryR = this.FindControl<Label>("BatteryR");
            _batteryC = this.FindControl<Label>("BatteryC");
            _caseLabel = this.FindControl<Label>("CaseLabel");
            
            _iconLeft = this.FindControl<Image>("ImageLeft");
            _iconRight = this.FindControl<Image>("ImageRight");
            
            string modifier = string.Empty;
            if (model == Models.BudsPlus) modifier = "+";
            else if (model == Models.BudsLive) modifier = " Live";

            string name = Environment.UserName.Split(' ')[0];

            string title = SettingsProvider.Instance.Popup.CustomTitle == string.Empty
                ? Loc.Resolve("connpopup_title")
                : SettingsProvider.Instance.Popup.CustomTitle;

            _header.Content = string.Format(title, name, modifier);
            UpdateContent(left, right, box);
            


            SPPMessageHandler.Instance.BaseUpdate += InstanceOnBaseUpdate;
        }

        private void InstanceOnBaseUpdate(object? sender, IBasicStatusUpdate e)
        {
            UpdateContent(e.BatteryL, e.BatteryR, e.BatteryCase);
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

        private void Window_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            ClickedEventHandler?.Invoke(this, EventArgs.Empty);
            Close();
        }

        private void OnOpened(object? sender, EventArgs e)
        {
            var workArea = (Screens.Primary ?? Screens.All[0]).WorkingArea;

            int padding = 20;
            switch (SettingsProvider.Instance.Popup.Placement)
            {
                case PopupPlacement.TopLeft:
                    Position = new PixelPoint(workArea.X + padding, workArea.Y + padding);
                    break;
                case PopupPlacement.TopCenter:
                    Position = new PixelPoint((int) ((workArea.Width / 2f) - (this.Width / 2) + workArea.X), workArea.Y + padding);
                    break;
                case PopupPlacement.TopRight:
                    Position = new PixelPoint((int) (workArea.Width - this.Width + workArea.X - padding), workArea.Y + padding);
                    break;
                case PopupPlacement.BottomLeft:
                    Position = new PixelPoint(workArea.X + padding, (int) (workArea.Height - this.Height + workArea.Y - padding));
                    break;
                case PopupPlacement.BottomCenter:
                    Position = new PixelPoint((int) ((workArea.Width / 2f) - (this.Width / 2) + workArea.X), (int) (workArea.Height - this.Height + workArea.Y - padding));
                    break;
                case PopupPlacement.BottomRight:
                    Position = new PixelPoint((int) ((workArea.Width - this.Width) + workArea.X - padding), (int) (workArea.Height - this.Height + workArea.Y - padding));
                    break;
            }
        }
    }
}