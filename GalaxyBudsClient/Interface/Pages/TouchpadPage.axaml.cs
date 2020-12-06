using Avalonia.Input;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Interface.Items;
using Serilog;

namespace GalaxyBudsClient.Interface.Pages
{
 	public class TouchpadPage : AbstractPage
	{
		public override Pages PageType => Pages.Touch;
		
		private readonly SwitchListItem _ambientSwitch;
		private readonly SwitchListItem _voiceFocusSwitch;
		private readonly SliderListItem _volumeSlider;
		
		public TouchpadPage()
		{   
			InitializeComponent();
			/*_ambientSwitch = this.FindControl<SwitchListItem>("AmbientToggle");
			_voiceFocusSwitch = this.FindControl<SwitchListItem>("AmbientVoiceFocusToggle");
			_volumeSlider = this.FindControl<SliderListItem>("AmbientVolume");

			_volumeSlider.SubtitleDictionary = new Dictionary<int, string>()
			{
				{ 0, "Very low" },
				{ 1, "Low" },
				{ 2, "Moderate" },
				{ 3, "High" },
				{ 4, "Extra loud" }
			};*/
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

		private void LockToggle_OnToggled(object? sender, bool e)
		{
		}

		private void DoubleTapVolume_OnToggled(object? sender, bool e)
		{
		}
	}
}
