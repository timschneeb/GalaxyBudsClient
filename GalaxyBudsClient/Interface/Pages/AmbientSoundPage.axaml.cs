using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Interface.Items;
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
		
		public AmbientSoundPage()
		{   
			InitializeComponent();
			_ambientSwitch = this.FindControl<SwitchListItem>("AmbientToggle");
			_voiceFocusSwitch = this.FindControl<SwitchListItem>("AmbientVoiceFocusToggle");
			_volumeSlider = this.FindControl<SliderListItem>("AmbientVolume");
			
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

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		public override void OnPageShown()
		{
			Log.Debug(this.GetType().Name + " shown");
		}

		public override void OnPageHidden()
		{
			Log.Debug(this.GetType().Name + " hidden");
		}

		private void BackButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.Home);
		}
		
		private void AmbientToggle_OnToggled(object? sender, bool e)
		{
			
		}

		private void VoiceFocusToggle_OnToggled(object? sender, bool e)
		{
			
		}

		private void VolumeSlider_OnChanged(object? sender, int e)
		{
			
		}

		private void ExtraLoud_OnToggled(object? sender, bool e)
		{
			
		}
	}
}
