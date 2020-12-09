using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Decoder;
using GalaxyBudsClient.Interface.Items;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.DynamicLocalization;
using Serilog;

namespace GalaxyBudsClient.Interface.Pages
{
 	public class AmbientSoundPage : AbstractPage
	{
		public override Pages PageType => Pages.AmbientSound;
		
		private readonly SwitchListItem _ambientSwitch;
		private readonly SwitchListItem _voiceFocusSwitch;
		private readonly SliderListItem _volumeSlider;
		private readonly SwitchDetailListItem _extraLoud;
		
		private readonly Border _voiceFocusBorder;
		private readonly Border _extraLoudBorder;
		
		public AmbientSoundPage()
		{   
			AvaloniaXamlLoader.Load(this);
			_ambientSwitch = this.FindControl<SwitchListItem>("AmbientToggle");
			_voiceFocusSwitch = this.FindControl<SwitchListItem>("AmbientVoiceFocusToggle");
			_volumeSlider = this.FindControl<SliderListItem>("AmbientVolume");
			_extraLoud = this.FindControl<SwitchDetailListItem>("AmbientExtraLoud");
			
			_voiceFocusBorder = this.FindControl<Border>("AmbientVoiceFocusBorder");
			_extraLoudBorder = this.FindControl<Border>("AmbientExtraLoudBorder");

			SPPMessageHandler.Instance.AmbientEnabledUpdateResponse += (sender, b) => _ambientSwitch.IsChecked = b; 
			SPPMessageHandler.Instance.ExtendedStatusUpdate += OnExtendedStatusUpdate;
			
			Loc.LanguageUpdated += UpdateStrings;
			UpdateStrings();
		}

		private void UpdateStrings()
		{
			_volumeSlider.SubtitleDictionary = new Dictionary<int, string>()
			{
				{ 0, Loc.Resolve("as_scale_very_low") },
				{ 1, Loc.Resolve("as_scale_low") },
				{ 2, Loc.Resolve("as_scale_moderate") },
				{ 3, Loc.Resolve("as_scale_high") },
				{ 4, Loc.Resolve("as_scale_extraloud") }
			};
		}

		public override void OnPageShown()
		{
			_voiceFocusBorder.IsVisible = BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.AmbientVoiceFocus);
			_extraLoudBorder.IsVisible = BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.AmbientExtraLoud);
			
			if (BluetoothImpl.Instance.ActiveModel != Models.BudsPlus)
			{
				_volumeSlider.Maximum = 4;
			}
		}
		

		private void OnExtendedStatusUpdate(object? sender, ExtendedStatusUpdateParser e)
		{ 
			if (BluetoothImpl.Instance.ActiveModel == Models.BudsPlus)
			{
				_extraLoud.IsChecked = e.ExtraHighAmbientEnabled;
				_volumeSlider.Maximum = e.ExtraHighAmbientEnabled ? 3 : 2;
			}
			else
			{
				_voiceFocusSwitch.IsChecked = e.AmbientSoundMode == AmbientType.VoiceFocus;
			}
			
			_ambientSwitch.IsChecked = e.AmbientSoundEnabled;
			_volumeSlider.Value = e.AmbientSoundVolume;
		}
		
		private void BackButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.Home);
		}
		
		private async void AmbientToggle_OnToggled(object? sender, bool e)
		{
			await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.MSG_ID_SET_AMBIENT_MODE, e);
		}

		private async void VoiceFocusToggle_OnToggled(object? sender, bool e)
		{
			var type = _voiceFocusSwitch.IsChecked ? AmbientType.VoiceFocus : AmbientType.Default;
			await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.MSG_ID_AMBIENT_VOICE_FOCUS, e);
		}

		private async void VolumeSlider_OnChanged(object? sender, int e)
		{
			await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.MSG_ID_AMBIENT_VOLUME, (byte)e);
		}

		private async void ExtraLoud_OnToggled(object? sender, bool e)
		{
			if (!BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.AmbientExtraLoud))
			{
				MainWindow.Instance.ShowUnsupportedFeaturePage(
					string.Format(
						Loc.Resolve("adv_required_firmware_later"), 
						BluetoothImpl.Instance.DeviceSpec.RecommendedFwVersion(IDeviceSpec.Feature.AmbientExtraLoud)));
				return;
			}
			
			_volumeSlider.Maximum = e ? 3 : 2;
			if (e || _volumeSlider.Value >= 3)
				_volumeSlider.Value = _volumeSlider.Maximum;
			
			await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.MSG_ID_EXTRA_HIGH_AMBIENT, e);
			await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.MSG_ID_AMBIENT_VOLUME, (byte)_volumeSlider.Value);
		}
	}
}
