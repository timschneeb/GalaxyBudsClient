using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Interface.Items;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Utils;
using Serilog;

namespace GalaxyBudsClient.Interface.Pages
{
 	public class BudsAppDetectedPage : AbstractPage
	{
		public override Pages PageType => Pages.BudsAppDetected;

		public BudsAppDetectedPage() {   
			AvaloniaXamlLoader.Load(this);
		}

		private void Next_OnPointerPressed(object? sender, PointerPressedEventArgs e) {
			MainWindow.Instance.Pager.SwitchPage(Pages.DeviceSelect);
		}

		private void BackButton_OnPointerPressed(object? sender, PointerPressedEventArgs e) {
			MainWindow.Instance.Pager.SwitchPage(Pages.Welcome);
		}
	}
}
