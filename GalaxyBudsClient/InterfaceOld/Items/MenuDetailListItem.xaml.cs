using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Utils.Interface;
using MenuEntries = System.Collections.Generic.Dictionary<string, System.EventHandler<Avalonia.Interactivity.RoutedEventArgs>?>;

namespace GalaxyBudsClient.InterfaceOld.Items
{
    public class MenuDetailListItem : UserControl
    {
        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<MenuDetailListItem, string>(nameof(Text));
        
        public static readonly StyledProperty<string> DescriptionProperty =
            AvaloniaProperty.Register<MenuDetailListItem, string>(nameof(Description));

        public static readonly StyledProperty<MenuEntries?> ItemsProperty =
            AvaloniaProperty.Register<MenuDetailListItem, MenuEntries?>(nameof(Items));

        private ContextMenu? _ctxMenu;
        
        public MenuDetailListItem()
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
                    _ctxMenu.MaxWidth = double.MaxValue;
                }
            }
        }

        private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            _ctxMenu?.Open(this);
        }
    }
}
