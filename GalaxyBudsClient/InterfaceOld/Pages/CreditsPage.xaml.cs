using System;
using System.Diagnostics;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.InterfaceOld.Items;
using GalaxyBudsClient.Platform;

namespace GalaxyBudsClient.InterfaceOld.Pages
{
 	public class CreditsPage : AbstractPage
	{
		public override Pages PageType => Pages.Credits;

		public CreditsPage()
		{
			AvaloniaXamlLoader.Load(this);
			var versionItem = this.GetControl<DetailListItem>("Version");
			versionItem.Description = (Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "?.?.?.?") + $" (GPLv3)";
		}

		private void BackButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(BluetoothImpl.Instance.RegisteredDeviceValid
				? Pages.Home
				: Pages.Welcome);
		}

		private void OpenWebsite(String url)
		{
			var psi = new ProcessStartInfo
			{
				FileName = url,
				UseShellExecute = true
			};
			Process.Start(psi);
		}

		private void Telegram_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			OpenWebsite("https://t.me/ThePBone");
		}

		private void Website_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			OpenWebsite("https://timschneeberger.me");
		}

		private void GitHub_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			OpenWebsite("https://github.com/ThePBone/GalaxyBudsClient");
		}

		private void Sponsor_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			OpenWebsite("https://ko-fi.com/thepbone");
		}
	}
}
