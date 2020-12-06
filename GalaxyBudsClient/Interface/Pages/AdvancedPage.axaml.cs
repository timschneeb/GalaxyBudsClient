using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Interface.Items;
using Serilog;

namespace GalaxyBudsClient.Interface.Pages
{
 	public class AdvancedPage : AbstractPage
	{
		public override Pages PageType => Pages.Advanced;
		
		private readonly SwitchListItem _ambientSwitch;
		private readonly SwitchListItem _voiceFocusSwitch;
		private readonly SliderListItem _volumeSlider;
		
		public AdvancedPage()
		{   
			InitializeComponent();
			_ambientSwitch = this.FindControl<SwitchListItem>("AmbientToggle");
			_voiceFocusSwitch = this.FindControl<SwitchListItem>("AmbientVoiceFocusToggle");
			_volumeSlider = this.FindControl<SliderListItem>("AmbientVolume");
			
			// Loc.LanguageUpdated += UpdateStrings;
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

		private void SeamlessConnection_OnToggled(object? sender, bool e)
		{
		}

		private void ResumeSensor_OnToggled(object? sender, bool e)
		{
		}

		private void Sidetone_OnToggled(object? sender, bool e)
		{
		}

		private void Passthrough_OnToggled(object? sender, bool e)
		{
		}

		private void GamingMode_OnToggled(object? sender, bool e)
		{
		}
	}
}
