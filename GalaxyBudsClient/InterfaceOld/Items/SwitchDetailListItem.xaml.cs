using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GalaxyBudsClient.InterfaceOld.Items
{
    public class SwitchDetailListItem : UserControl
    {
        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<SwitchDetailListItem, string>(nameof(Text));
        
        public static readonly StyledProperty<string> DescriptionProperty =
            AvaloniaProperty.Register<SwitchDetailListItem, string>(nameof(Description));
        
        public SwitchDetailListItem()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public bool IsChecked { set; get; }

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
    }
}
