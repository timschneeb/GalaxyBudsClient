using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Serilog;
using ToggleSwitch = GalaxyBudsClient.Interface.Elements.ToggleSwitch;

namespace GalaxyBudsClient.Interface.Pages
{
 	public class HomePage : AbstractPage
	{
		public override Pages PageType => Pages.Home;
		
		private readonly ToggleSwitch _ancSwitch;

		public HomePage()
		{   
			InitializeComponent();
			_ancSwitch = this.FindControl<ToggleSwitch>("AncToggle");
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

		private void AncBorder_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			_ancSwitch.Toggle();
		}

		private void FindMyBuds_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.FindMyGear);
		}

		private void Ambient_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.AmbientSound);
		}

		private void Touch_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.Touch);
		}

		private void Equalizer_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.Equalizer);
		}

		private void Advanced_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.Advanced);
		}

		private void System_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.System);
		}
	}
}
