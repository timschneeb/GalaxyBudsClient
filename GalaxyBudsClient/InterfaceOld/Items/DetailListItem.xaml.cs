using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GalaxyBudsClient.InterfaceOld.Items
{
    public class DetailListItem : UserControl
    {
        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<DetailListItem, string>(nameof(Text));
        
        public static readonly StyledProperty<string> DescriptionProperty =
            AvaloniaProperty.Register<DetailListItem, string>(nameof(Description));
        
        public DetailListItem()
        {
            AvaloniaXamlLoader.Load(this);
            DataContext = this;
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
    }
}
