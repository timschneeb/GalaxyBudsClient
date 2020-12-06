using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using ToggleSwitch = GalaxyBudsClient.Interface.Elements.ToggleSwitch;

namespace GalaxyBudsClient.Interface.Items
{
    public class SwitchDetailListItem : UserControl
    {
        public static readonly StyledProperty<String> TextProperty =
            AvaloniaProperty.Register<SwitchDetailListItem, String>(nameof(Text));
        
        public static readonly StyledProperty<String> DescriptionProperty =
            AvaloniaProperty.Register<SwitchDetailListItem, String>(nameof(Description));
        
        public event EventHandler<bool>? Toggled;

        private readonly ToggleSwitch _toggle;
        
        public SwitchDetailListItem()
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

        public String Description
        {
            get => GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
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
