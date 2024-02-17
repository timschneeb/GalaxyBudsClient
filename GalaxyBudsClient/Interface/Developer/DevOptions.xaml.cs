using System;
using System.Collections;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.Interface;
using Window = Avalonia.Controls.Window;

namespace GalaxyBudsClient.Interface.Developer
{
    public sealed class DevOptions : Window
    {
        public DevOptions()
        {
            AvaloniaXamlLoader.Load(this);
            this.AttachDevTools();
        }
        
        private void LaunchDevTools_OnClick(object? sender, RoutedEventArgs e)
        {
            DialogLauncher.ShowDevTools();
        }

        private void LaunchTranslatorMode_OnClick(object? sender, RoutedEventArgs e)
        {
            DialogLauncher.ShowTranslatorTools();
        }
    }
}