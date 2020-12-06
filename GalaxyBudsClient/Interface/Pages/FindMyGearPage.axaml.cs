using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Serilog;

namespace GalaxyBudsClient.Interface.Pages
{
 	public class FindMyGearPage : AbstractPage
	{
		public override Pages PageType => Pages.FindMyGear;
		
		public FindMyGearPage()
		{   
			InitializeComponent();
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
		
	}
}
