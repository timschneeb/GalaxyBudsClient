using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace GalaxyBudsClient.Interface.Elements
{
    public class MuteButton : UserControl
    {

        public event EventHandler<bool>? Toggled;
        
        public static readonly StyledProperty<bool> IsMutedProperty =
            AvaloniaProperty.Register<ToggleSwitch, bool>(nameof(IsMuted), false);
        
        public bool IsMuted
        {
            get => GetValue(IsMutedProperty);
            set => SetValue(IsMutedProperty, value);
        }

        public MuteButton()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            IsMuted = !IsMuted;
            Toggled?.Invoke(this, IsMuted);
        }
    }
}
