using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using GalaxyBudsClient.Bluetooth.Linux;
using GalaxyBudsClient.Interface.Elements;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.DynamicLocalization;
using Serilog;

namespace GalaxyBudsClient.Interface.Pages
{
    public class FindMyGearPage : AbstractPage
    {
        public override Pages PageType => Pages.FindMyGear;

        private readonly ScanButton _scanButton;
        private readonly MuteButton _leftMuteButton;
        private readonly MuteButton _rightMuteButton;

        private readonly Image _batteryIconLeft;
        private readonly Image _batteryIconRight;
        private readonly Image _iconLeft;
        private readonly Image _iconRight;

        private readonly Label _warningText;
        private readonly Grid _warningContainer;
        
        private bool _lastWarningLeft;
        private bool _lastWarningRight;
        
        public FindMyGearPage()
        {   
            AvaloniaXamlLoader.Load(this);

            _scanButton = this.FindControl<ScanButton>("ScanButton");
            _leftMuteButton = this.FindControl<MuteButton>("LeftMuteButton");
            _rightMuteButton = this.FindControl<MuteButton>("RightMuteButton");
            
            _batteryIconLeft = this.FindControl<Image>("BatteryIconLeft");
            _batteryIconRight = this.FindControl<Image>("BatteryIconRight");
            _iconLeft = this.FindControl<Image>("LeftIcon");
            _iconRight = this.FindControl<Image>("RightIcon");

            _warningText = this.FindControl<Label>("EarbudWarningText");
            _warningContainer = this.FindControl<Grid>("EarbudWarningContainer");
            
            SPPMessageHandler.Instance.BaseUpdate += (sender, update) => UpdateDashboard(update);
            SPPMessageHandler.Instance.FindMyGearStopped += InstanceOnFindMyGearStopped;
            SPPMessageHandler.Instance.FindMyGearMuteUpdate += InstanceOnFindMyGearMuteUpdate;
            
            EventDispatcher.Instance.EventReceived += OnEventReceived;
            
            _scanButton.ScanningStatusChanged += ScanButton_OnScanningStatusChanged;
            _leftMuteButton.Toggled += LeftMuteButton_OnToggled;
            _rightMuteButton.Toggled += RightMuteButton_OnToggled;
        }

        private void OnEventReceived(EventDispatcher.Event @event, object? arg)
        {
            switch (@event)
            {
                case EventDispatcher.Event.StartFind:
                    _scanButton.IsSearching = true;
                    ScanButton_OnScanningStatusChanged(this, true);
                    break;
                case EventDispatcher.Event.StopFind:
                    _scanButton.IsSearching = false;
                    ScanButton_OnScanningStatusChanged(this, false);
                    break;
                case EventDispatcher.Event.StartStopFind:
                    _scanButton.IsSearching = !_scanButton.IsSearching;
                    ScanButton_OnScanningStatusChanged(this, _scanButton.IsSearching);
                    break;
            }
        }

        private async void RightMuteButton_OnToggled(object? sender, bool e)
        {
            await MessageComposer.FindMyGear.MuteEarbud(_leftMuteButton.IsMuted, _rightMuteButton.IsMuted);
        }

        private async void LeftMuteButton_OnToggled(object? sender, bool e)
        {
            await MessageComposer.FindMyGear.MuteEarbud(_leftMuteButton.IsMuted, _rightMuteButton.IsMuted);
        }

        private async void ScanButton_OnScanningStatusChanged(object? sender, bool e)
        {
            if (e)
            {
                await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.FIND_MY_EARBUDS_START);    
                _leftMuteButton.IsVisible = true;
                _rightMuteButton.IsVisible = true;
            }
            else
            {
                await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.FIND_MY_EARBUDS_STOP);    
                _leftMuteButton.IsVisible = false;
                _rightMuteButton.IsVisible = false;
                _leftMuteButton.IsMuted = false;
                _rightMuteButton.IsMuted = false;
            }
        }

        private void InstanceOnFindMyGearMuteUpdate(object? sender, MuteUpdateParser e)
        {
            _leftMuteButton.IsMuted = e.LeftMuted;
            _rightMuteButton.IsMuted = e.RightMuted;
        }

        private void InstanceOnFindMyGearStopped(object? sender, EventArgs e)
        {
            _scanButton.IsSearching = false;

            _leftMuteButton.IsVisible = false;
            _rightMuteButton.IsVisible = false;
            _leftMuteButton.IsMuted = false;
            _rightMuteButton.IsMuted = false;
        }
        
        public void UpdateDashboard(IBasicStatusUpdate ext)
        {
            UpdateBatteryIcons(ext.BatteryL, Devices.L);
            UpdateBatteryIcons(ext.BatteryR, Devices.R);
            UpdateIcons(ext.BatteryL, ext.BatteryR);
            EarbudWarning(ext.WearState == WearStates.L || ext.WearState == WearStates.Both,
                ext.WearState == WearStates.R || ext.WearState == WearStates.Both);
        }

        public override void OnPageShown()
        {
            _leftMuteButton.IsVisible = false;
            _rightMuteButton.IsVisible = false;
            _leftMuteButton.IsMuted = false;
            _rightMuteButton.IsMuted = false;
            
            EarbudWarning(_lastWarningLeft, _lastWarningRight);

            if (DeviceMessageCache.Instance.DebugGetAllData == null)
            {
                UpdateIcons(0,0);
            }
            else
            {
                UpdateIcons(DeviceMessageCache.Instance.DebugGetAllData.LeftAdcSOC,
                    DeviceMessageCache.Instance.DebugGetAllData.RightAdcSOC);
            }
        }

        public override async void OnPageHidden()
        {
            await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.FIND_MY_EARBUDS_STOP);
            _scanButton.IsSearching = false;
        }

        private void EarbudWarning(bool l, bool r)
        {
            _lastWarningLeft = l;
            _lastWarningRight = r;

            string notice = "";
            if (l && r)
            {
                notice = Loc.Resolve("fmg_warning_both");
            }
            else if (l)
            {
                notice = Loc.Resolve("fmg_warning_left");
            }
            else if (r)
            {
                notice = Loc.Resolve("fmg_warning_right");
            }

            if (notice == string.Empty)
            {
                _warningContainer.IsVisible = false;
            }
            else
            {
                _warningContainer.IsVisible = true;
                _warningText.Content = notice;
            }
        }


        private void UpdateIcons(double left, double right)
        {
            UpdateBatteryIcons((int) Math.Round(left), Devices.L);
            UpdateBatteryIcons((int) Math.Round(right), Devices.R);
            
            bool isLeftOnline = left > 0;
            bool isRightOnline = right > 0;
            

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

        private void UpdateBatteryIcons(int p, Devices side)
        {
            IImage? batteryIcon;
            if (p <= 0)
            {
                batteryIcon = null;
            }
            else if (p <= 25)
            {
                batteryIcon = (IImage?)Application.Current.FindResource("BatteryLow");
            }
            else if (p <= 50)
            {
                batteryIcon = (IImage?)Application.Current.FindResource("BatteryMedium");
            }
            else if (p <= 90)
            {
                batteryIcon = (IImage?)Application.Current.FindResource("BatteryHigh");
            }
            else
            {
                batteryIcon = (IImage?)Application.Current.FindResource("BatteryFull");
            }
            
            switch (side)
            {
                case Devices.L:
                    _batteryIconLeft.Source = batteryIcon;
                    break;
                case Devices.R:
                    _batteryIconRight.Source = batteryIcon;
                    break;
            }
        }

        private void BackButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            MainWindow.Instance.Pager.SwitchPage(Pages.Home);
        }
		
    }
}