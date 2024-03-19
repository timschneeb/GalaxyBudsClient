using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace GalaxyBudsClient.InterfaceOld.Pages
{
 	public class BudsAppDetectedPage : AbstractPage
	{
		public override Pages PageType => Pages.BudsAppDetected;

		public BudsAppDetectedPage() {   
			AvaloniaXamlLoader.Load(this);
		}

		private void Next_OnPointerPressed(object? sender, PointerPressedEventArgs e) {
			//MainWindow.Instance.Pager.SwitchPage(Pages.DeviceSelect);
		}

		private void BackButton_OnPointerPressed(object? sender, PointerPressedEventArgs e) {
			//MainWindow.Instance.Pager.SwitchPage(Pages.Welcome);
		}
	}
}
