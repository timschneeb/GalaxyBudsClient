using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using ToggleSwitch = GalaxyBudsClient.InterfaceOld.Elements.ToggleSwitch;

namespace GalaxyBudsClient.InterfaceOld.Items
{
    public class SwitchDetailListItem : UserControl
    {
        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<SwitchDetailListItem, string>(nameof(Text));
        
        public static readonly StyledProperty<string> DescriptionProperty =
            AvaloniaProperty.Register<SwitchDetailListItem, string>(nameof(Description));
        
        public event EventHandler<bool>? Toggled;

        private readonly ToggleSwitch _toggle;
        
        public SwitchDetailListItem()
        {
            AvaloniaXamlLoader.Load(this);
            _toggle = this.GetControl<ToggleSwitch>("Toggle");
            DataContext = this;
        }

        public bool IsChecked
        {
            get => _toggle.IsChecked;
            set => _toggle.IsChecked = value;
        }

        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public string Description
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
