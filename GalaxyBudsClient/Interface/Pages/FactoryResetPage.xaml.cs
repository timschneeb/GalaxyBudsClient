using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.Elements;
using GalaxyBudsClient.Interface.Items;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;
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

			_pageHeader = this.GetControl<PageHeader>("PageHeader");
			_resetButton = this.GetControl<IconListItem>("FactoryReset");
			
			SppMessageHandler.Instance.ResetResponse += InstanceOnResetResponse;
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

			BluetoothImpl.Instance.UnregisterDevice();
			MainWindow.Instance.Pager.SwitchPage(Pages.Welcome);
		}

		public override void OnPageShown()
		{
			_pageHeader.BackButtonVisible = true;
			_pageHeader.LoadingSpinnerVisible = false;
			
			this.GetControl<IconListItem>("FactoryReset").Source =
				(IImage?)Application.Current?.FindResource($"Neutral{BluetoothImpl.Instance.DeviceSpec.IconResourceKey}");
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

			await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.RESET);
		}
	}
}
