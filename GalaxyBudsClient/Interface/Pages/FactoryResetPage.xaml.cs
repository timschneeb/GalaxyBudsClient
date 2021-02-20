using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.Elements;
using GalaxyBudsClient.Interface.Items;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.DynamicLocalization;
using Serilog;

namespace GalaxyBudsClient.Interface.Pages
{
 	public class FactoryResetPage : AbstractPage
	{
		public override Pages PageType => Pages.FactoryReset;

		private readonly PageHeader _pageHeader;
		private readonly IconListItem _resetButton;

		public FactoryResetPage()
		{   
			AvaloniaXamlLoader.Load(this);

			_pageHeader = this.FindControl<PageHeader>("PageHeader");
			_resetButton = this.FindControl<IconListItem>("FactoryReset");
			
			SPPMessageHandler.Instance.ResetResponse += InstanceOnResetResponse;
		}

		private void InstanceOnResetResponse(object? sender, int e)
		{
			_pageHeader.BackButtonVisible = true;
			_pageHeader.LoadingSpinnerVisible = false;

			_resetButton.Text = Loc.Resolve("factory_confirm");
			_resetButton.IsEnabled = false;

			if (e != 0)
			{
				new MessageBox()
				{
					Title = Loc.Resolve("factory_error_title"),
					Description = Loc.Resolve("factory_error")
				}.ShowDialog(MainWindow.Instance);
				return;
			}

			BluetoothImpl.Instance.UnregisterDevice()
				.ContinueWith((_) => MainWindow.Instance.Pager.SwitchPage(Pages.Welcome));
		}

		public override void OnPageShown()
		{
			_pageHeader.BackButtonVisible = true;
			_pageHeader.LoadingSpinnerVisible = false;
		}

		private void BackButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.System);
		}

		private async void FactoryReset_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			_pageHeader.BackButtonVisible = false;
			_pageHeader.LoadingSpinnerVisible = true;
			_resetButton.Text = Loc.Resolve("system_waiting_for_device");
			_resetButton.IsEnabled = false;

			await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.RESET);
		}
	}
}
