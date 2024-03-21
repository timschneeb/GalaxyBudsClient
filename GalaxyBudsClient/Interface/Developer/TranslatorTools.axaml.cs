using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;

namespace GalaxyBudsClient.Interface.Developer
{
    public partial class TranslatorTools : Window
    {
        public TranslatorTools()
        {
            InitializeComponent();
            
            Locales.SelectedItem = Settings.Instance.Locale;
            XamlPath.Text = Loc.GetTranslatorModeFile();
            IgnoreConnLoss.IsChecked = BluetoothImpl.Instance.SuppressDisconnectionEvents;
            // TODO DummyDevices.IsChecked = MainWindow2.Instance.DeviceSelectionPage.EnableDummyDevices;
            
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
            if (Locales.SelectedItem is Locales locale)
            {
                Settings.Instance.Locale = locale;
            }

            Loc.Load();
        }

        private void IgnoreConnLoss_OnChecked(object? sender, RoutedEventArgs e)
        {
            BluetoothImpl.Instance.SuppressDisconnectionEvents = IgnoreConnLoss.IsChecked ?? false;
        }

        private void DummyDevices_OnChecked(object? sender, RoutedEventArgs e)
        {
            // TODO MainWindow2.Instance.DeviceSelectionPage.EnableDummyDevices = DummyDevices.IsChecked ?? false;
        }
    }
}