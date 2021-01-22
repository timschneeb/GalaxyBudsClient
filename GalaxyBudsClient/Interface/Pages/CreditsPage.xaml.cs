using System;
using System.Diagnostics;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Interface.Items;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.DynamicLocalization;
using Serilog;

namespace GalaxyBudsClient.Interface.Pages
{
 	public class CreditsPage : AbstractPage
	{
		public override Pages PageType => Pages.Credits;
		
		private readonly DetailListItem _versionItem;
		
		public CreditsPage()
		{   
			AvaloniaXamlLoader.Load(this);
			_versionItem = this.FindControl<DetailListItem>("Version");
			_versionItem.Description = (Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "?.?.?.?") + $" (GPLv3)";
		}

		private void BackButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			if (BluetoothImpl.Instance.RegisteredDeviceValid)
			{
				MainWindow.Instance.Pager.SwitchPage(Pages.Home);
			}
			else
			{
				MainWindow.Instance.Pager.SwitchPage(Pages.Welcome);
			}
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
