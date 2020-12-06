using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Utils;
using MenuEntries = System.Collections.Generic.Dictionary<string, System.EventHandler<Avalonia.Interactivity.RoutedEventArgs>?>;

namespace GalaxyBudsClient.Interface.Items
{
    public class MenuListItem : UserControl
    {
        public static readonly StyledProperty<String> TextProperty =
            AvaloniaProperty.Register<MenuListItem, String>(nameof(Text));
        
        public static readonly StyledProperty<String> DescriptionProperty =
            AvaloniaProperty.Register<MenuListItem, String>(nameof(Description));
        
        public static readonly StyledProperty<MenuEntries?> ItemsProperty =
            AvaloniaProperty.Register<MenuListItem, MenuEntries?>(nameof(Items));

        private ContextMenu? _ctxMenu;
        
        public MenuListItem()
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
        
        public MenuEntries? Items
        {
            get => GetValue(ItemsProperty);
            set
            {
                SetValue(ItemsProperty, value);
                if (value == null)
                {
                    ContextMenu = null;
                }
                else
                {
                    _ctxMenu = MenuFactory.BuildContextMenu(value, this);
                }
            }
        }

        private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            _ctxMenu?.Open(this);
        }
    }
}
