using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Interface.Items;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.DynamicLocalization;
using Serilog;

namespace GalaxyBudsClient.Interface.Pages
{
 	public class SettingsPage : AbstractPage
	{
		public override Pages PageType => Pages.Settings;
		
		private readonly SwitchDetailListItem _darkMode;
		private readonly MenuDetailListItem _locale;
		private readonly SwitchDetailListItem _minimizeTray;
		private readonly SwitchDetailListItem _autostart;

		public SettingsPage()
		{   
			InitializeComponent();
			_darkMode = this.FindControl<SwitchDetailListItem>("DarkModeSelect");
			_locale = this.FindControl<MenuDetailListItem>("LocaleSelect");
			_minimizeTray = this.FindControl<SwitchDetailListItem>("MinimizeTrayToggle");
			_autostart = this.FindControl<SwitchDetailListItem>("AutostartToggle");
		}
		
		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		public override void OnPageShown()
		{
			_autostart.IsChecked = AutoStartImpl.Instance.Enabled;
			_darkMode.IsChecked = SettingsProvider.Instance.DarkMode == DarkModes.Dark;
			_locale.Description = SettingsProvider.Instance.Locale.GetDescription();
			
			var localeMenuActions =
				new Dictionary<string,EventHandler<RoutedEventArgs>?>();
			
#pragma warning disable 8605
			foreach (int value in Enum.GetValues(typeof(Locales)))
#pragma warning restore 8605
			{
				if (value == (int)Locales.custom && !Loc.IsTranslatorModeEnabled())
					continue;

				var locale = (Locales)value;
				localeMenuActions[locale.GetDescription()] = (sender, args) =>
				{
					_locale.Description = locale.GetDescription();
					SettingsProvider.Instance.Locale = locale;
					Loc.Load();
				};
			}
			_locale.Items = localeMenuActions;
		}

		private void BackButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.Home);
		}

		private void PopupSettings_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.SettingsPopup);
		}

		private void DevMode_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.ShowDevTools();
		}

		private void DarkMode_OnToggled(object? sender, bool e)
		{
			SettingsProvider.Instance.DarkMode = e ? DarkModes.Dark : DarkModes.Light;

			if (Application.Current is App app)
			{
				app.RestartApp(Pages.Settings);
			}
		}

		private void Autostart_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			AutoStartImpl.Instance.Enabled = !AutoStartImpl.Instance.Enabled;
		}

		private void MinimizeToTray_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			throw new NotImplementedException();
		}
	}
}
