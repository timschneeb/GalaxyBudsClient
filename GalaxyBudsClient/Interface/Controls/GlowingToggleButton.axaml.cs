using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace GalaxyBudsClient.Interface.Controls
{
    public partial class GlowingToggleButton : UserControl
    {
        public GlowingToggleButton()
        {
            InitializeComponent();
            Button.IsCheckedChanged += (_, _) => IsChecked = Button.IsChecked;
        }

        public static readonly RoutedEvent<RoutedEventArgs> IsCheckedChangedEvent = 
            RoutedEvent.Register<GlowingToggleButton, RoutedEventArgs>(nameof(IsCheckedChanged), RoutingStrategies.Bubble);

        public static readonly StyledProperty<bool?> IsCheckedProperty = 
            ToggleButton.IsCheckedProperty.AddOwner<GlowingToggleButton>();
        
        public static readonly StyledProperty<string?> TextProperty = 
            TextBlock.TextProperty.AddOwner<GlowingToggleButton>();
    
        public event EventHandler<RoutedEventArgs>? IsCheckedChanged
        {
            add => AddHandler(IsCheckedChangedEvent, value);
            remove => RemoveHandler(IsCheckedChangedEvent, value);
        }

        public bool? IsChecked
        {
            get => GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }
        
        public string? Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
    
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == IsCheckedProperty)
            {
                Button.IsChecked = IsChecked;
                GlowButton.Tag = IsChecked == true ? "Glowing" : string.Empty;
                RaiseEvent(new RoutedEventArgs(IsCheckedChangedEvent));
            }
            else if (change.Property == TextProperty)
            {
                Button.Content = Text;
            }
            else
            {
                base.OnPropertyChanged(change);
            }
        }
    }
}
