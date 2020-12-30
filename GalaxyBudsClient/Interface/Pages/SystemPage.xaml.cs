using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Interface.Elements;
using GalaxyBudsClient.Interface.Items;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.DynamicLocalization;
using Org.BouncyCastle.Crypto.Parameters;
using Serilog;

namespace GalaxyBudsClient.Interface.Pages
{
 	public class SystemPage : AbstractPage
	{
		public override Pages PageType => Pages.System;
		
		public SystemPage()
		{   
			AvaloniaXamlLoader.Load(this);
		}

		public override void OnPageShown()
		{
			this.FindControl<Control>("TraceCoreDumpSeparator").IsVisible =
				BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.FragmentedMessages);
			this.FindControl<Control>("TraceCoreDump").IsVisible =
				BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.FragmentedMessages);
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

		private void DownloadTraceCoredump_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.SystemCoredump);
		}
	}
}
