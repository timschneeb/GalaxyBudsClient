using System;
using System.Collections;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Utils;
using Window = Avalonia.Controls.Window;

namespace GalaxyBudsClient.Interface.Developer
{
    public sealed class DevOptions : Window
    {
        private readonly CheckBox _crowdsourcing;
        
        public DevOptions()
        {
            AvaloniaXamlLoader.Load(this);
            this.AttachDevTools();

            _crowdsourcing = this.FindControl<CheckBox>("Crowdsourcing");
            _crowdsourcing.IsChecked = !SettingsProvider.Instance.Experiments.Disabled;
        }
        
        private void LaunchDevTools_OnClick(object? sender, RoutedEventArgs e)
        {
            DialogLauncher.ShowDevTools();
        }

        private void LaunchTranslatorMode_OnClick(object? sender, RoutedEventArgs e)
        {
            DialogLauncher.ShowTranslatorTools();
        }
        
        private void Crowdsourcing_OnClick(object? sender, RoutedEventArgs e)
        {
            SettingsProvider.Instance.Experiments.Disabled = !_crowdsourcing.IsChecked ?? false;
        }
    }
}