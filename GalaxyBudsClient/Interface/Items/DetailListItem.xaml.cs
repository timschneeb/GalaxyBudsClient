using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GalaxyBudsClient.Interface.Items
{
    public class DetailListItem : UserControl
    {
        public static readonly StyledProperty<String> TextProperty =
            AvaloniaProperty.Register<DetailListItem, String>(nameof(Text));
        
        public static readonly StyledProperty<String> DescriptionProperty =
            AvaloniaProperty.Register<DetailListItem, String>(nameof(Description));
        
        public DetailListItem()
        {
            AvaloniaXamlLoader.Load(this);
            DataContext = this;
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
    }
}
