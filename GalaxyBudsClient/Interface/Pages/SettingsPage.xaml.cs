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
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;
using Serilog;

namespace GalaxyBudsClient.Interface.Pages
{
    public class SettingsPage : AbstractPage
    {
        public override Pages PageType => Pages.Settings;

        private readonly SwitchDetailListItem _darkMode;
        private readonly MenuDetailListItem _locale;
        
        public SettingsPage()
        {
            AvaloniaXamlLoader.Load(this);
            _darkMode = this.GetControl<SwitchDetailListItem>("DarkModeSelect");
            _locale = this.GetControl<MenuDetailListItem>("LocaleSelect");
        }

        public override void OnPageShown()
        {
            this.GetControl<DetailListItem>("TraySettings").IsVisible = PlatformUtils.SupportsTrayIcon || PlatformUtils.SupportsAutoboot;
            
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
            MainWindow.Instance.Pager.SwitchPage(BluetoothImpl.Instance.IsConnected ? Pages.Home : Pages.NoConnection);
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

        private void Crowdsourcing_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            MainWindow.Instance.Pager.SwitchPage(Pages.SettingsCrowdsourcing);
        }

        private void TraySettings_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            MainWindow.Instance.Pager.SwitchPage(Pages.SettingsTray);
        }
    }
}