using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace GalaxyBudsClient.InterfaceOld.Items
{
    public class IconListItem : UserControl
    {  
       
        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<IconListItem, string>(nameof(Text));
        
        public static readonly StyledProperty<IImage?> SourceProperty =
            AvaloniaProperty.Register<IconListItem, IImage?>(nameof(Source));
        
        public IconListItem()
        {
            AvaloniaXamlLoader.Load(this);
            DataContext = this;
        }
        
        public string Text
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
