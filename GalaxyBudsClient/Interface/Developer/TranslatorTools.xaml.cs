using System;
using System.Collections;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.InterfaceOld.Dialogs;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;

namespace GalaxyBudsClient.Interface.Developer
{
    public sealed class TranslatorTools : Window
    {
        private class ViewModel
        {
            public IEnumerable LocaleSource => Enum.GetValues(typeof(Locales)).Cast<Locales>().ToList();
            public IEnumerable ModelSource => Enum.GetValues(typeof(Model.Constants.Models)).Cast<Model.Constants.Models>().ToList();
        }

        private readonly CheckBox _ignoreConnLoss;
        private readonly CheckBox _dummyDevices;
        private readonly ComboBox _locales;
        private readonly TextBox _xamlPath;

        private readonly ViewModel _vm = new();

        public TranslatorTools()
        {
            DataContext = _vm;
            AvaloniaXamlLoader.Load(this);
            this.AttachDevTools();

            _ignoreConnLoss = this.GetControl<CheckBox>("IgnoreConnLoss");
            _dummyDevices = this.GetControl<CheckBox>("DummyDevices");
            _locales = this.GetControl<ComboBox>("Locales");
            _xamlPath = this.GetControl<TextBox>("XamlPath");

            _locales.SelectedItem = Settings.Instance.Locale;
            _xamlPath.Text = Loc.GetTranslatorModeFile();
            _ignoreConnLoss.IsChecked = BluetoothImpl.Instance.SuppressDisconnectionEvents;
            _dummyDevices.IsChecked = MainWindow.Instance.DeviceSelectionPage.EnableDummyDevices;
            
            Loc.ErrorDetected += (title, content) =>
            {
                var td = new TaskDialog
                {
                    Header = title, 
                    Buttons = { TaskDialogButton.CloseButton }, 
                    IconSource = new FluentIcons.Avalonia.Fluent.SymbolIconSource { Symbol = FluentIcons.Common.Symbol.Warning },
                    Content = new TextBlock
                    {
                        TextWrapping = TextWrapping.Wrap,
                        Text = content,
                        MaxWidth = 450
                    },
                    XamlRoot = this
                };

                _ = td.ShowAsync();
            };
        }
        
        private void ReloadXaml_OnClick(object? sender, RoutedEventArgs e)
        {
            if (_locales.SelectedItem is Locales locale)
            {
                Settings.Instance.Locale = locale;
            }

            Loc.Load();
        }

        private void IgnoreConnLoss_OnChecked(object? sender, RoutedEventArgs e)
        {
            BluetoothImpl.Instance.SuppressDisconnectionEvents = _ignoreConnLoss.IsChecked ?? false;
        }

        private void DummyDevices_OnChecked(object? sender, RoutedEventArgs e)
        {
            MainWindow.Instance.DeviceSelectionPage.EnableDummyDevices = _dummyDevices.IsChecked ?? false;
        }
    }
}