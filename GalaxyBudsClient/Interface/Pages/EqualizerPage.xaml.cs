using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GalaxyBudsClient.Interface.Items;

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
 	public class EqualizerPage : AbstractPage
	{
		public override Pages PageType => Pages.Equalizer;
        public bool EqualizerEnabled => _eqSwitch.IsChecked;
        
		private readonly SwitchListItem _eqSwitch;
		private readonly SliderListItem _presetSlider;
		private readonly SliderListItem _stereoPan;
		
		public EqualizerPage()
		{   
			AvaloniaXamlLoader.Load(this);
			_eqSwitch = this.FindControl<SwitchListItem>("EqToggle");
			_presetSlider = this.FindControl<SliderListItem>("EqPreset");
			_stereoPan = this.FindControl<SliderListItem>("StereoPan");
			
			SPPMessageHandler.Instance.ExtendedStatusUpdate += InstanceOnExtendedStatusUpdate;
			
			EventDispatcher.Instance.EventReceived += OnEventReceived;

			Loc.LanguageUpdated += UpdateStrings;
			UpdateStrings();
		}

		public override void OnPageShown()
		{
			_stereoPan.Parent!.IsVisible = BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.StereoPan);
		}

		private void OnEventReceived(EventDispatcher.Event e, object? arg)
        {
            Dispatcher.UIThread.Post(async () =>
            {
                switch (e)
                {
                    case EventDispatcher.Event.EqualizerToggle:
	                    await MessageComposer.SetEqualizer(!_eqSwitch.IsChecked, (EqPreset)_presetSlider.Value, false);
	                    _eqSwitch.Toggle();
                        break;
                    case EventDispatcher.Event.EqualizerNextPreset:
	                    _eqSwitch.IsChecked = true;
	                    var newVal = _presetSlider.Value + 1;
	                    if (newVal >= 5)
		                    newVal = 0;

	                    _presetSlider.Value = newVal;
	                    await MessageComposer.SetEqualizer(_eqSwitch.IsChecked, (EqPreset)_presetSlider.Value, false);
	                    break;
                }
            });
        }
		
		private void InstanceOnExtendedStatusUpdate(object? sender, ExtendedStatusUpdateParser e)
		{
			if (BluetoothImpl.Instance.ActiveModel == Models.Buds)
			{
				_eqSwitch.IsChecked = e.EqualizerEnabled;
				
				var preset = e.EqualizerMode;
				if (preset >= 5)
				{
					preset -= 5;
				}

				_presetSlider.Value = preset;
			}
			else
			{
				_eqSwitch.IsChecked = e.EqualizerMode != 0;
				if (e.EqualizerMode == 0)
				{
					_presetSlider.Value = 2;
				}
				else
				{
					_presetSlider.Value = e.EqualizerMode - 1;
				}
			}

			_stereoPan.Value = e.HearingEnhancements;
		}

		private void UpdateStrings()
		{
			_presetSlider.SubtitleDictionary = new Dictionary<int, string>()
			{
				{ 0, Loc.Resolve("eq_bass") },
				{ 1, Loc.Resolve("eq_soft") },
				{ 2, Loc.Resolve("eq_dynamic") },
				{ 3, Loc.Resolve("eq_clear") },
				{ 4, Loc.Resolve("eq_treble") }
			};

			var sp = new Dictionary<int, string>();
			for (var i = 0; i <= 32; i++)
			{
				var progress = (int) ((float) i / _stereoPan.Maximum * 100.0f);
				if(progress == 50)
				{
					sp[i] = Loc.Resolve("eq_stereo_balance_neutral");
				}
				else{
					sp[i] = string.Format(Loc.Resolve("eq_stereo_balance_value"), 100 - progress, progress);
				}
			}
			_stereoPan.SubtitleDictionary = sp;
		}

		private void BackButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.Home);
		}

		private async void EqToggle_OnToggled(object? sender, bool e)
		{
			await MessageComposer.SetEqualizer(_eqSwitch.IsChecked, (EqPreset)_presetSlider.Value, false);
		}

		private async void EqPreset_OnChanged(object? sender, int e)
		{
			await MessageComposer.SetEqualizer(_eqSwitch.IsChecked, (EqPreset)_presetSlider.Value, false);
		}

		private async void StereoPan_OnChanged(object? sender, int e)
		{
			await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.SET_HEARING_ENHANCEMENTS, (byte)e);
		}
	}
}
