using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace GalaxyBudsClient.Interface.Elements
{
    public class ScanButton : UserControl
    {
        public bool IsSearching
        {
            get => _isSearching; 
            set
            {
                _isSearching = value;
                _button.Tag = _glow.Tag = IsSearching ? "on" : "off";
            }
        }

        private readonly Image _button;
        private readonly Image _glow;
        private bool _isSearching = false;

        public event EventHandler<bool>? ScanningStatusChanged; 
        
        public ScanButton()
        {
            AvaloniaXamlLoader.Load(this);
            _button = this.FindControl<Image>("Button");
            _glow = this.FindControl<Image>("Glow");
        }

        private void Button_OnTapped(object? sender, RoutedEventArgs e)
        {
            IsSearching = !IsSearching;
            ScanningStatusChanged?.Invoke(this, IsSearching);
        }
    }
}
