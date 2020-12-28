using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Decoder;
using GalaxyBudsClient.Interface.Elements;
using GalaxyBudsClient.Interface.Items;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.DynamicLocalization;
using Serilog;

namespace GalaxyBudsClient.Interface.Pages
{
 	public class SystemPage : AbstractPage
	{
		public override Pages PageType => Pages.System;
		
		private string Waiting => Loc.Resolve("system_waiting_for_device");
		private string Left => Loc.Resolve("left");
		private string Right => Loc.Resolve("right");
		
		private readonly PageHeader _pageHeader;

		public SystemPage()
		{   
			AvaloniaXamlLoader.Load(this);
			_pageHeader = this.FindControl<PageHeader>("PageHeader");
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

		private void SystemInfo_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.SystemInfo);
		}

		private async void PairingMode_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.UNK_PAIRING_MODE);
			await BluetoothImpl.Instance.DisconnectAsync();
		}
	}
}
