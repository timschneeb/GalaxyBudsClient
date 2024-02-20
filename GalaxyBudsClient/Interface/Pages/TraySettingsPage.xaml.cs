using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Interface.Items;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.Interface;

namespace GalaxyBudsClient.Interface.Pages
{
 	public class TraySettingsPage : AbstractPage
	{
		public override Pages PageType => Pages.SettingsTray;
		
		private readonly SwitchDetailListItem _minimizeTray;
		private readonly MenuDetailListItem _dynTrayMode;
		private readonly SwitchDetailListItem _autostart;
		
		public TraySettingsPage()
		{   
			AvaloniaXamlLoader.Load(this);
			
			_minimizeTray = this.GetControl<SwitchDetailListItem>("MinimizeTrayToggle");
			_dynTrayMode = this.GetControl<MenuDetailListItem>("DynTrayMode");
			_autostart = this.GetControl<SwitchDetailListItem>("AutostartToggle");
			
			this.GetControl<Separator>("AutostartToggleSeparator").IsVisible = PlatformUtils.SupportsTrayIcon && PlatformUtils.SupportsAutoboot;
			this.GetControl<Separator>("MinimizeTrayToggleSeparator").IsVisible = PlatformUtils.SupportsTrayIcon;

			_dynTrayMode.IsVisible = PlatformUtils.SupportsTrayIcon;
			_minimizeTray.IsVisible = PlatformUtils.SupportsTrayIcon;
			_autostart.IsVisible = PlatformUtils.SupportsTrayIcon && PlatformUtils.SupportsAutoboot;
		}

		public override void OnPageShown()
		{
			_minimizeTray.IsChecked = SettingsProvider.Instance.MinimizeToTray;
			_autostart.IsChecked = AutoStartImpl.Instance.Enabled;
			
			UpdateMenus();
			UpdateMenuDescriptions();
		}
		
		private void UpdateMenuDescriptions()
		{
			_dynTrayMode.Description = SettingsProvider.Instance.DynamicTrayIconMode.GetDescription();
		}
		
		private void UpdateMenus()
		{
			var menuActions = new Dictionary<string,EventHandler<RoutedEventArgs>?>();
#pragma warning disable 8605
			foreach (int value in Enum.GetValues(typeof(DynamicTrayIconModes)))
#pragma warning restore 8605
			{
				var mode = (DynamicTrayIconModes)value;
				menuActions[mode.GetDescription()] = (sender, args) =>
				{
					SettingsProvider.Instance.DynamicTrayIconMode = mode;
					UpdateMenuDescriptions();
					if(mode == DynamicTrayIconModes.Disabled)
						WindowIconRenderer.ResetIconToDefault();
					else
					{
						var cache = DeviceMessageCache.Instance.BasicStatusUpdate;
						if (BluetoothImpl.Instance.IsConnected && cache != null)
						{
							WindowIconRenderer.UpdateDynamicIcon(cache);
						}
						else
						{
							WindowIconRenderer.ResetIconToDefault();
						}
					}
				};
			}
			_dynTrayMode.Items = menuActions;
		}
	
		private void BackButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.Settings);
		}
		
		private void Autostart_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			if (!_minimizeTray.IsChecked && _autostart.IsChecked)
			{
				_minimizeTray.Toggle();
				SettingsProvider.Instance.MinimizeToTray = _minimizeTray.IsChecked;
			}

			AutoStartImpl.Instance.Enabled = _autostart.IsChecked;
		}

		private void MinimizeToTray_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			if (!_minimizeTray.IsChecked && _autostart.IsChecked)
			{
				_autostart.Toggle();
				AutoStartImpl.Instance.Enabled = _autostart.IsChecked;
			}
			SettingsProvider.Instance.MinimizeToTray = _minimizeTray.IsChecked;
		}
	}
}
