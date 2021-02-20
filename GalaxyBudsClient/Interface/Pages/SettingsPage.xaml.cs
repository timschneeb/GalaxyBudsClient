using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Interface.Items;
using GalaxyBudsClient.Message;
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
        private readonly Border _trayOptionBorder;

        public SettingsPage()
        {
            AvaloniaXamlLoader.Load(this);
            _darkMode = this.FindControl<SwitchDetailListItem>("DarkModeSelect");
            _locale = this.FindControl<MenuDetailListItem>("LocaleSelect");
            _minimizeTray = this.FindControl<SwitchDetailListItem>("MinimizeTrayToggle");
            _autostart = this.FindControl<SwitchDetailListItem>("AutostartToggle");
            _trayOptionBorder = this.FindControl<Border>("TrayOptionBorder");

            _trayOptionBorder.IsVisible = PlatformUtils.SupportsTrayIcon;
        }

        public override void OnPageShown()
        {
            _minimizeTray.IsChecked = SettingsProvider.Instance.MinimizeToTray;
            _autostart.IsChecked = AutoStartImpl.Instance.Enabled;
            _darkMode.IsChecked = SettingsProvider.Instance.DarkMode == DarkModes.Dark;
            _locale.Description = SettingsProvider.Instance.Locale.GetDescription();

            var localeMenuActions =
                new Dictionary<string, EventHandler<RoutedEventArgs>?>();

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
            if (BluetoothImpl.Instance.IsConnected)
            {
                MainWindow.Instance.Pager.SwitchPage(Pages.Home);
            }
            else
            {
                MainWindow.Instance.Pager.SwitchPage(Pages.NoConnection);
            }
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