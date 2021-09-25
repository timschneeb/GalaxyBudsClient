using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Interface.Items;
using GalaxyBudsClient.Interop.TrayIcon;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.DynamicLocalization;
using Serilog;

namespace GalaxyBudsClient.Interface.Pages
{
    public class NoiseProPage : AbstractPage
    {
        public override Pages PageType => Pages.NoiseControlPro;
        
        public bool AmbientEnabled => _ambientSwitch.IsChecked;
        public bool AncEnabled => _ancSwitch.IsChecked;
        
        private readonly SwitchListItem _ambientSwitch;
        
        private readonly SwitchListItem _ancSwitch;
        private readonly SwitchDetailListItem _ancLevel;       
        
        private readonly SwitchDetailListItem _voiceDetect;
        private readonly MenuDetailListItem _voiceDetectTimeout;
        private readonly Border _voiceBorder;

        public NoiseProPage()
        {   
            AvaloniaXamlLoader.Load(this);
            _ambientSwitch = this.FindControl<SwitchListItem>("AmbientToggle");
            
            _ancSwitch = this.FindControl<SwitchListItem>("AncToggle");
            _ancLevel = this.FindControl<SwitchDetailListItem>("AncLevelToggle");

            _voiceDetect = this.FindControl<SwitchDetailListItem>("VoiceDetect");
            _voiceDetectTimeout = this.FindControl<MenuDetailListItem>("VoiceDetectTimeout");
            _voiceBorder = this.FindControl<Border>("VoiceDetectBorder");
            
            SPPMessageHandler.Instance.NoiseControlUpdateResponse += (sender, mode) => SetNoiseControlState(mode);
            SPPMessageHandler.Instance.ExtendedStatusUpdate += OnExtendedStatusUpdate;

            EventDispatcher.Instance.EventReceived += OnEventReceived;
            Loc.LanguageUpdated += UpdateVoiceDetectTimeout;
            
            UpdateVoiceDetectTimeout();
        }
        
        private void SetNoiseControlState(NoiseControlMode mode)
        {
            switch (mode)
            {
                case NoiseControlMode.Off:
                    _ancSwitch.IsChecked = false;
                    _ambientSwitch.IsChecked = false;
                    break;
                case NoiseControlMode.AmbientSound:
                    _ancSwitch.IsChecked = false;
                    _ambientSwitch.IsChecked = true;
                    break;
                case NoiseControlMode.NoiseReduction:
                    _ancSwitch.IsChecked = true;
                    _ambientSwitch.IsChecked = false;
                    break;
            }
        }

        private void OnEventReceived(EventDispatcher.Event e, object? arg)
        {
            if (!BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.NoiseControl))
            {
                return;
            }
            
            Avalonia.Threading.Dispatcher.UIThread.Post(async () =>
            {
                switch (e)
                {
                    case EventDispatcher.Event.ToggleConversationDetect:
                        _voiceDetect.Toggle();
                        await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.SET_DETECT_CONVERSATIONS, _voiceDetect.IsChecked);
                        break;
                    case EventDispatcher.Event.AncToggle:
                        _ancSwitch.Toggle();
                        AncToggle_OnToggled(this, _ancSwitch.IsChecked);
                        break;
                    case EventDispatcher.Event.SwitchAncSensitivity:
                        _ancLevel.Toggle();
                        await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.NOISE_REDUCTION_LEVEL, _ancLevel.IsChecked);
                        break;
                    case EventDispatcher.Event.AmbientToggle:
                        _ambientSwitch.Toggle();
                        AmbientToggle_OnToggled(this, _ambientSwitch.IsChecked);
                        break;
                    case EventDispatcher.Event.SetNoiseControlState:
                        if (arg == null)
                        {
                            break;
                        }
                        
                        SetNoiseControlState((NoiseControlMode)arg);
                        break;
                }
            });
        }

        public override void OnPageShown()
        {
            _voiceBorder.IsVisible =
                BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.DetectConversations);
            this.FindControl<Border>("AncLevelToggleBorder").IsVisible =
                BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.AncNoiseReductionLevels);
            this.FindControl<Separator>("AncLevelToggleSeparator").IsVisible =
                BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.AncNoiseReductionLevels);
        }
        
        private void OnExtendedStatusUpdate(object? sender, ExtendedStatusUpdateParser e)
        {
            _ambientSwitch.IsChecked = e.NoiseControlMode == NoiseControlMode.AmbientSound;
            _ancSwitch.IsChecked = e.NoiseControlMode == NoiseControlMode.NoiseReduction;
            _ancLevel.IsChecked = e.NoiseReductionLevel == 1;
            _voiceDetect.IsChecked = e.DetectConversations;

            byte seconds;
            switch (e.DetectConversationsDuration)
            {
                case 1:
                    seconds = 10;
                    break;
                case 2:
                    seconds = 15;
                    break;
                default:
                    seconds = 5;
                    break;
            }
            _voiceDetectTimeout.Description = _voiceDetectTimeout.Description =
                string.Format(Loc.Resolve("nc_voicedetect_timeout_item"), seconds); 
        }
		
        private void UpdateVoiceDetectTimeout()
        {
            var items = new Dictionary<string, EventHandler<RoutedEventArgs>?>{};
            foreach (var sec in new (byte, byte)[]{(0, 5), (1, 10), (2, 15)})
            {
                items.Add(string.Format(Loc.Resolve("nc_voicedetect_timeout_item"), sec.Item2), async (sender, args) =>
                {
                    _voiceDetectTimeout.Description =
                        string.Format(Loc.Resolve("nc_voicedetect_timeout_item"), sec.Item2);
                    await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.SET_DETECT_CONVERSATIONS_DURATION, sec.Item1);
                });
            }

            _voiceDetectTimeout.Items = items;
        }
        
        private void BackButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            MainWindow.Instance.Pager.SwitchPage(Pages.Home);
        }
		
        private async void AmbientToggle_OnToggled(object? sender, bool e)
        {
            if (e)
            {
                _ancSwitch.IsChecked = false;
                await MessageComposer.NoiseControl.SetMode(NoiseControlMode.AmbientSound);
            }
            else
            {
                await MessageComposer.NoiseControl.SetMode(NoiseControlMode.Off);
            }
        }
        
        private async void AncToggle_OnToggled(object? sender, bool e)
        {
            if (e)
            {
                _ambientSwitch.IsChecked = false;
                await MessageComposer.NoiseControl.SetMode(NoiseControlMode.NoiseReduction);
            }
            else
            {
                await MessageComposer.NoiseControl.SetMode(NoiseControlMode.Off);
            }
        }

        private async void AncLevelToggle_OnToggled(object? sender, bool e)
        {
            await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.NOISE_REDUCTION_LEVEL, e);
        }

        private async void VoiceDetect_OnToggled(object? sender, bool e)
        {
            await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.SET_DETECT_CONVERSATIONS, e);
        }

        private void AmbientSettings_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            MainWindow.Instance.Pager.SwitchPage(Pages.NoiseControlProAmbient);
        }
    }
}