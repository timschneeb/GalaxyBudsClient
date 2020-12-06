using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Interface.Items;
using Serilog;

namespace GalaxyBudsClient.Interface.Pages
{
 	public class SystemPage : AbstractPage
	{
		public override Pages PageType => Pages.System;
		
		private readonly SwitchListItem _ambientSwitch;
		private readonly SwitchListItem _voiceFocusSwitch;
		private readonly SliderListItem _volumeSlider;
		
		public SystemPage()
		{   
			InitializeComponent();
			_ambientSwitch = this.FindControl<SwitchListItem>("AmbientToggle");
			_voiceFocusSwitch = this.FindControl<SwitchListItem>("AmbientVoiceFocusToggle");
			_volumeSlider = this.FindControl<SliderListItem>("AmbientVolume");

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
		
		private void FactoryReset_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.FactoryReset);
		}

		private void RunSelfTest_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.SelfTest);
		}
	}
}
