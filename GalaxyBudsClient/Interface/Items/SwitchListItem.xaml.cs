using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using ToggleSwitch = GalaxyBudsClient.Interface.Elements.ToggleSwitch;

namespace GalaxyBudsClient.Interface.Items
{
    public class SwitchListItem : UserControl
    {
        public static readonly StyledProperty<String> TextProperty =
            AvaloniaProperty.Register<SwitchListItem, String>(nameof(Text));
        
        public event EventHandler<bool>? Toggled;

        private ToggleSwitch _toggle;
        
        public SwitchListItem()
        {
            AvaloniaXamlLoader.Load(this);
            _toggle = this.FindControl<ToggleSwitch>("Toggle");
            DataContext = this;
        }
        
        public bool IsChecked
        {
            get => _toggle.IsChecked;
            set => _toggle.IsChecked = value;
        }

        public String Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public void Toggle()
        {
            _toggle.Toggle();
        }
        
        private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            Toggle();
            Toggled?.Invoke(this, _toggle.IsChecked);
        }
    }
}
