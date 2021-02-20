using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace GalaxyBudsClient.Interface.Items
{
    public class IconListItem : UserControl
    {  
       
        public static readonly StyledProperty<String> TextProperty =
            AvaloniaProperty.Register<IconListItem, String>(nameof(Text));
        
        public static readonly StyledProperty<IImage?> SourceProperty =
            AvaloniaProperty.Register<IconListItem, IImage?>(nameof(Source));
        
        public IconListItem()
        {
            AvaloniaXamlLoader.Load(this);
            DataContext = this;
        }
        
        public String Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        public IImage? Source
        {
            get => GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }
    }
}
