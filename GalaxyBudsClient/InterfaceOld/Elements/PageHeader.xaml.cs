using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace GalaxyBudsClient.InterfaceOld.Elements
{
    public class PageHeader : UserControl
    {
        public static readonly StyledProperty<bool> LoadingSpinnerVisibleProperty =
            AvaloniaProperty.Register<PageHeader, bool>(nameof(LoadingSpinnerVisible), false);
        
        public static readonly StyledProperty<bool> BackButtonVisibleProperty =
            AvaloniaProperty.Register<PageHeader, bool>(nameof(BackButtonVisible), true);

        public static readonly StyledProperty<String> TitleProperty =
            AvaloniaProperty.Register<PageHeader, String>(nameof(Title));

        public event EventHandler<PointerPressedEventArgs>? BackPressed;
        
        public PageHeader()
        {
            AvaloniaXamlLoader.Load(this);
            DataContext = this;
        }
        
        public bool LoadingSpinnerVisible
        {
            get => GetValue(LoadingSpinnerVisibleProperty);
            set => SetValue(LoadingSpinnerVisibleProperty, value);
        }

        public bool BackButtonVisible
        {
            get => GetValue(BackButtonVisibleProperty);
            set => SetValue(BackButtonVisibleProperty, value);
        }

        public String Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        private void BackButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (BackButtonVisible)
            {
                BackPressed?.Invoke(this, e);
            }
        }
    }
}
