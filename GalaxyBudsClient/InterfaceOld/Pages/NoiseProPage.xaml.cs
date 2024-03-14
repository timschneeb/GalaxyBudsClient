using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.InterfaceOld.Items;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;

namespace GalaxyBudsClient.InterfaceOld.Pages
{
    public class NoiseProPage : AbstractPage
    {
        public override Pages PageType => Pages.NoiseControlPro;
        
        public bool AmbientEnabled => _ambientSwitch.IsChecked;
        public bool AncEnabled => _ancSwitch.IsChecked;
        
        private readonly SwitchListItem _ambientSwitch;
        
        private readonly SwitchListItem _ancSwitch;
        private readonly SwitchDetailListItem _ancOne;
        private readonly SwitchDetailListItem _ancLevel;       
        
        private readonly SwitchDetailListItem _voiceDetect;
        private readonly MenuDetailListItem _voiceDetectTimeout;
        private readonly Border _voiceBorder;

        public NoiseProPage()
        {   
            AvaloniaXamlLoader.Load(this);
            _ambientSwitch = this.GetControl<SwitchListItem>("AmbientToggle");
            
            _ancSwitch = this.GetControl<SwitchListItem>("AncToggle");
            _ancOne = this.GetControl<SwitchDetailListItem>("AncOneToggle");
            _ancLevel = this.GetControl<SwitchDetailListItem>("AncLevelToggle");

            _voiceDetect = this.GetControl<SwitchDetailListItem>("VoiceDetect");
            _voiceDetectTimeout = this.GetControl<MenuDetailListItem>("VoiceDetectTimeout");
            _voiceBorder = this.GetControl<Border>("VoiceDetectBorder");
            
            SppMessageHandler.Instance.NoiseControlUpdateResponse += (sender, mode) => SetNoiseControlState(mode);
            SppMessageHandler.Instance.ExtendedStatusUpdate += OnExtendedStatusUpdate;

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
            if (!BluetoothImpl.Instance.DeviceSpec.Supports(Features.NoiseControl))
            {
                return;
            }
            
            Avalonia.Threading.Dispatcher.UIThread.Post(async () =>
            {
                switch (e)
                {
                    case EventDispatcher.Event.ToggleConversationDetect:
                        _voiceDetect.Toggle();
                        await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.SET_DETECT_CONVERSATIONS, _voiceDetect.IsChecked);
                        break;
                    case EventDispatcher.Event.AncToggle:
                        _ancSwitch.Toggle();
                        AncToggle_OnToggled(this, _ancSwitch.IsChecked);
                        break;
                    case EventDispatcher.Event.SwitchAncSensitivity:
                        _ancLevel.Toggle();
                        await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.NOISE_REDUCTION_LEVEL, _ancLevel.IsChecked);
                        break;
                    case EventDispatcher.Event.SwitchAncOne:
                        _ancOne.Toggle();
                        await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.SET_ANC_WITH_ONE_EARBUD, _ancOne.IsChecked);
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
                        
                        /* TODO: important: in new UI implementation, make sure to send Bluetooth messages directly here
                           Something like this:
                            if(BluetoothImpl.Instance.DeviceSpec.Supports(Features.NoiseControl))
                                await MessageComposer.NoiseControl.SetMode(NoiseControlMode.AmbientSound);
                            else
                                await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.SET_AMBIENT_MODE, true);
                         */
                        
                        
                        SetNoiseControlState((NoiseControlMode)arg);
                        break;
                }
            });
        }

        public override void OnPageShown()
        {
            _voiceBorder.IsVisible =
                BluetoothImpl.Instance.DeviceSpec.Supports(Features.DetectConversations);
            this.GetControl<Border>("AncLevelToggleBorder").IsVisible =
                this.GetControl<Separator>("AncLevelToggleSeparator").IsVisible =
                BluetoothImpl.Instance.DeviceSpec.Supports(Features.AncNoiseReductionLevels);
            this.GetControl<Border>("AncOneToggleBorder").IsVisible =
                this.GetControl<Separator>("AncOneToggleSeparator").IsVisible =
                    BluetoothImpl.Instance.DeviceSpec.Supports(Features.AncWithOneEarbud);
        }
        
        private void OnExtendedStatusUpdate(object? sender, ExtendedStatusUpdateParser e)
        {
            _ambientSwitch.IsChecked = e.NoiseControlMode == NoiseControlMode.AmbientSound;
            _ancSwitch.IsChecked = e.NoiseControlMode == NoiseControlMode.NoiseReduction;
            _ancLevel.IsChecked = e.NoiseReductionLevel == 1;
            _ancOne.IsChecked = e.AncWithOneEarbud;
            _voiceDetect.IsChecked = e.DetectConversations;

            byte seconds = e.DetectConversationsDuration switch
            {
                1 => 10,
                2 => 15,
                _ => 5
            };
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
                    await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.SET_DETECT_CONVERSATIONS_DURATION, sec.Item1);
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
            await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.NOISE_REDUCTION_LEVEL, e);
        }

        private async void AncOneToggle_OnToggled(object? sender, bool e)
        {
            await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.SET_ANC_WITH_ONE_EARBUD, e);
        }

        private async void VoiceDetect_OnToggled(object? sender, bool e)
        {
            await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.SET_DETECT_CONVERSATIONS, e);
        }

        private void AmbientSettings_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            MainWindow.Instance.Pager.SwitchPage(Pages.NoiseControlProAmbient);
        }
    }
}