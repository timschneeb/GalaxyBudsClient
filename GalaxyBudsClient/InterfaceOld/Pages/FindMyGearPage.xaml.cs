using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using GalaxyBudsClient.InterfaceOld.Elements;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;

namespace GalaxyBudsClient.InterfaceOld.Pages
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

            _scanButton = this.GetControl<ScanButton>("ScanButton");
            _leftMuteButton = this.GetControl<MuteButton>("LeftMuteButton");
            _rightMuteButton = this.GetControl<MuteButton>("RightMuteButton");
            
            _batteryIconLeft = this.GetControl<Image>("BatteryIconLeft");
            _batteryIconRight = this.GetControl<Image>("BatteryIconRight");
            _iconLeft = this.GetControl<Image>("LeftIcon");
            _iconRight = this.GetControl<Image>("RightIcon");

            _warningText = this.GetControl<Label>("EarbudWarningText");
            _warningContainer = this.GetControl<Grid>("EarbudWarningContainer");
            
            SppMessageHandler.Instance.BaseUpdate += (sender, update) => UpdateDashboard(update);
            SppMessageHandler.Instance.FindMyGearStopped += InstanceOnFindMyGearStopped;
            SppMessageHandler.Instance.FindMyGearMuteUpdate += InstanceOnFindMyGearMuteUpdate;
            
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
                await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.FIND_MY_EARBUDS_START);    
                _leftMuteButton.IsVisible = true;
                _rightMuteButton.IsVisible = true;
            }
            else
            {
                await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.FIND_MY_EARBUDS_STOP);    
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

        private void UpdateDashboard(IBasicStatusUpdate ext)
        {
            UpdateBatteryIcons(ext.BatteryL, Devices.L);
            UpdateBatteryIcons(ext.BatteryR, Devices.R);
            UpdateIcons(ext.BatteryL, ext.BatteryR);
            EarbudWarning(ext.WearState is WearStates.L or WearStates.Both,
                ext.WearState is WearStates.R or WearStates.Both);
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
            await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.FIND_MY_EARBUDS_STOP);
            _scanButton.IsSearching = false;
        }

        private void EarbudWarning(bool l, bool r)
        {
            _lastWarningLeft = l;
            _lastWarningRight = r;

            var notice = "";
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
            
            var isLeftOnline = left > 0;
            var isRightOnline = right > 0;

            var type = BluetoothImpl.Instance.DeviceSpec.IconResourceKey;
            
            var leftSourceName = $"Left{type}{(isLeftOnline ? "Connected" : "Disconnected")}";
            _iconLeft.Source = (IImage?)Application.Current?.FindResource(leftSourceName);
            var rightSourceName = $"Right{type}{(isRightOnline ? "Connected" : "Disconnected")}";
            _iconRight.Source = (IImage?)Application.Current?.FindResource(rightSourceName);
        }

        private void UpdateBatteryIcons(int p, Devices side)
        {
            var batteryIcon = p switch
            {
                <= 0 => null,
                <= 25 => (IImage?)Application.Current?.FindResource("BatteryLow"),
                <= 50 => (IImage?)Application.Current?.FindResource("BatteryMedium"),
                <= 90 => (IImage?)Application.Current?.FindResource("BatteryHigh"),
                _ => (IImage?)Application.Current?.FindResource("BatteryFull")
            };

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