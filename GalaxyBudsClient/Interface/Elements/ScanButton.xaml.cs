using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace GalaxyBudsClient.Interface.Elements
{
    public class ScanButton : UserControl
    {
        private bool _searching = false;
        private Image _button;
        private Image _glow;
        
        public ScanButton()
        {
            AvaloniaXamlLoader.Load(this);
            _button = this.FindControl<Image>("Button");
            _glow = this.FindControl<Image>("Glow");
        }

        private void Button_OnTapped(object? sender, RoutedEventArgs e)
        {
            _searching = !_searching;
            _button.Tag = _glow.Tag = _searching ? "on" : "off";
        }
    }
}
