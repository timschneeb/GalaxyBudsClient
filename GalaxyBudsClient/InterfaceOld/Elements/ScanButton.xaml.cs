using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace GalaxyBudsClient.InterfaceOld.Elements
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
            _button = this.GetControl<Image>("Button");
            _glow = this.GetControl<Image>("Glow");
        }

        private void Button_OnTapped(object? sender, TappedEventArgs? e)
        {
            IsSearching = !IsSearching;
            ScanningStatusChanged?.Invoke(this, IsSearching);
        }
    }
}
