using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Interface.Items;
using GalaxyBudsClient.Utils.DynamicLocalization;
using Serilog;

namespace GalaxyBudsClient.Interface.Pages
{
 	public class EqualizerPage : AbstractPage
	{
		public override Pages PageType => Pages.Equalizer;
		
		private readonly SwitchListItem _eqSwitch;
		private readonly SliderListItem _presetSlider;
		
		public EqualizerPage()
		{   
			AvaloniaXamlLoader.Load(this);
			_eqSwitch = this.FindControl<SwitchListItem>("EqToggle");
			_presetSlider = this.FindControl<SliderListItem>("EqPreset");
			
			Loc.LanguageUpdated += UpdateStrings;
			UpdateStrings();
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

		private void EqToggle_OnToggled(object? sender, bool e)
		{
		}

		private void EqPreset_OnChanged(object? sender, int e)
		{
		}
	}
}
