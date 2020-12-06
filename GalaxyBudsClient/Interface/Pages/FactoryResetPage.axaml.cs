using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Serilog;

namespace GalaxyBudsClient.Interface.Pages
{
 	public class FactoryResetPage : AbstractPage
	{
		public override Pages PageType => Pages.FactoryReset;
		
		public FactoryResetPage()
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
			MainWindow.Instance.Pager.SwitchPage(Pages.System);
		}

		private void FactoryReset_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			
		}
	}
}
