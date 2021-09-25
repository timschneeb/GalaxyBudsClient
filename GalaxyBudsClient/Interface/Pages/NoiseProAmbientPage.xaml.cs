using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Interface.Items;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Message.Encoder;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.DynamicLocalization;
namespace GalaxyBudsClient.Interface.Pages
{
    public class NoiseProAmbientPage : AbstractPage
    {
        public override Pages PageType => Pages.NoiseControlProAmbient;
        
        private readonly SliderListItem _volumeSlider;
        
        private readonly SwitchListItem _ambientCustomize;
        private readonly SliderListItem _ambientTone;
        private readonly SliderListItem _ambientVolLeft;
        private readonly SliderListItem _ambientVolRight;

        private readonly Border _ambientCustomBorder;

        public NoiseProAmbientPage()
        {   
            AvaloniaXamlLoader.Load(this);
            _volumeSlider = this.FindControl<SliderListItem>("AmbientVolume");

            _ambientCustomize = this.FindControl<SwitchListItem>("AmbientCustomize");
            _ambientTone = this.FindControl<SliderListItem>("AmbientTone");
            _ambientVolLeft = this.FindControl<SliderListItem>("AmbientLeftVol");
            _ambientVolRight = this.FindControl<SliderListItem>("AmbientRightVol");
            _ambientCustomBorder = this.FindControl<Border>("AmbientCustomBorder");
            
            SPPMessageHandler.Instance.ExtendedStatusUpdate += OnExtendedStatusUpdate;

            EventDispatcher.Instance.EventReceived += OnEventReceived;
            Loc.LanguageUpdated += UpdateStrings;
            UpdateStrings();
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
                    case EventDispatcher.Event.AmbientVolumeUp:
                        EventDispatcher.Instance.Dispatch(EventDispatcher.Event.SetNoiseControlState,
                            NoiseControlMode.AmbientSound);
                        await MessageComposer.NoiseControl.SetMode(NoiseControlMode.AmbientSound);
                        
                        if (_ambientCustomize.IsChecked)
                        {
                            if (_ambientVolLeft.Value != _ambientVolLeft.Maximum)
                            {
                                _ambientVolLeft.Value += 1;
                            }
                            if (_ambientVolRight.Value != _ambientVolRight.Maximum)
                            {
                                _ambientVolRight.Value += 1;
                            }
                            
                            SendAmbientCustomizeState();
                        }
                        else
                        {
                            if (_volumeSlider.Value != _volumeSlider.Maximum)
                            {
                                _volumeSlider.Value += 1;
                            }

                            await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.AMBIENT_VOLUME,
                                (byte)_volumeSlider.Value);
                        }

                        break;
                    case EventDispatcher.Event.AmbientVolumeDown:
                        if (_ambientCustomize.IsChecked)
                        {
                            if (_ambientVolLeft.Value <= 0 && _ambientVolRight.Value <= 0)
                            {
                                EventDispatcher.Instance.Dispatch(EventDispatcher.Event.SetNoiseControlState,
                                    NoiseControlMode.Off);
                                await MessageComposer.NoiseControl.SetMode(NoiseControlMode.Off);
                            }
                            else
                            {
                                await MessageComposer.NoiseControl.SetMode(NoiseControlMode.AmbientSound);
                                EventDispatcher.Instance.Dispatch(EventDispatcher.Event.SetNoiseControlState,
                                    NoiseControlMode.AmbientSound);
                                if (_ambientVolLeft.Value != _ambientVolLeft.Minimum)
                                {
                                    _ambientVolLeft.Value -= 1;
                                }
                                if (_ambientVolRight.Value != _ambientVolRight.Minimum)
                                {
                                    _ambientVolRight.Value -= 1;
                                }
                                
                                SendAmbientCustomizeState();
                            }
                        }
                        else
                        {
                            if (_volumeSlider.Value <= 0)
                            {
                                EventDispatcher.Instance.Dispatch(EventDispatcher.Event.SetNoiseControlState,
                                    NoiseControlMode.Off);
                                await MessageComposer.NoiseControl.SetMode(NoiseControlMode.Off);
                            }
                            else
                            {
                                await MessageComposer.NoiseControl.SetMode(NoiseControlMode.AmbientSound);
                                EventDispatcher.Instance.Dispatch(EventDispatcher.Event.SetNoiseControlState,
                                    NoiseControlMode.AmbientSound);
                                _volumeSlider.Value -= 1;

                                await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.AMBIENT_VOLUME,
                                    (byte)_volumeSlider.Value);
                            }
                        }

                        break;
                }
            });
        }

        private void UpdateStrings()
        {
            _volumeSlider.SubtitleDictionary = new Dictionary<int, string>()
            {
                { 0, Loc.Resolve("as_scale_low") },
                { 1, Loc.Resolve("as_scale_moderate") },
                { 2, Loc.Resolve("as_scale_high") },
                { 3, Loc.Resolve("as_scale_extraloud") }
            };
            _ambientTone.SubtitleDictionary = new Dictionary<int, string>()
            {
                { 0, Loc.Resolve("nc_as_custom_tone_soft") + " +2" },
                { 1, Loc.Resolve("nc_as_custom_tone_soft") + " +1" },
                { 2, Loc.Resolve("nc_as_custom_tone_neutral") },
                { 3, Loc.Resolve("nc_as_custom_tone_clear") + " +1" },
                { 4, Loc.Resolve("nc_as_custom_tone_clear") + " +2" }
            };
            
            var volDictionary = new Dictionary<int, string>()
            {
                { 0, "-2" },
                { 1, "-1" },
                { 2, Loc.Resolve("nc_as_custom_vol_normal") },
                { 3, "+1" },
                { 4, "+2" }
            };
            _ambientVolLeft.SubtitleDictionary = volDictionary;
            _ambientVolRight.SubtitleDictionary = volDictionary;
        }

        public override void OnPageShown()
        {
            _ambientCustomBorder.IsVisible =
                BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.AmbientCustomize);
        }
        
        private void OnExtendedStatusUpdate(object? sender, ExtendedStatusUpdateParser e)
        {
            _volumeSlider.Value = e.AmbientSoundVolume;
            _ambientTone.Value = e.AmbientCustomSoundTone;
            _ambientVolLeft.Value = e.AmbientCustomVolumeLeft;
            _ambientVolRight.Value = e.AmbientCustomVolumeRight;
            _ambientCustomize.IsChecked = e.AmbientCustomVolumeOn;
            UpdateUiState(e.AmbientCustomVolumeOn);
        }

        private async void SendAmbientCustomizeState()
        {
            await BluetoothImpl.Instance.SendAsync(CustomizeAmbientEncoder.Build(_ambientCustomize.IsChecked,
                (byte)_ambientVolLeft.Value, (byte)_ambientVolRight.Value, (byte)_ambientTone.Value));
        }

        private void UpdateUiState(bool isAdvancedOn)
        {
            _volumeSlider.Parent.Parent.IsVisible = !isAdvancedOn;
            _ambientTone.Parent.IsVisible = isAdvancedOn;
            _ambientVolLeft.Parent.IsVisible = isAdvancedOn;
            _ambientVolRight.Parent.IsVisible = isAdvancedOn;
            for (var i = 1; i <= 3; i++)
            {
                this.FindControl<Separator>($"AmbSep{i}").IsVisible = isAdvancedOn;
            }
        }
        
        private void BackButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            MainWindow.Instance.Pager.SwitchPage(Pages.NoiseControlPro);
        }
		
        private async void VolumeSlider_OnChanged(object? sender, int e)
        {
            await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.AMBIENT_VOLUME, (byte)e);
        }

        private void AmbientCustomize_OnToggled(object? sender, bool e)
        {
            UpdateUiState(e);
            SendAmbientCustomizeState();
        }

        private void AmbientTone_OnChanged(object? sender, int e)
        {
            SendAmbientCustomizeState();
        }

        private void AmbientLeftVol_OnChanged(object? sender, int e)
        {
            SendAmbientCustomizeState();
        }

        private void AmbientRightVol_OnChanged(object? sender, int e)
        {
            SendAmbientCustomizeState();
        }
    }
}