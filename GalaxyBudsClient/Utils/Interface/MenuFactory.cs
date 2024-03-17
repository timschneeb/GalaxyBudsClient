using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace GalaxyBudsClient.Utils.Interface
{
    [Obsolete("Will be removed during the rewrite")]
     public static class MenuFactory
    {
        private const string MenuStyle = "RoundMenuStyle";

        public static ContextMenu BuildContextMenu(Dictionary<string, EventHandler<RoutedEventArgs>?> content, Control? placementTarget = null, bool embedSeparators = true)
        {
            var menu = new ContextMenu {Placement = PlacementMode.BottomEdgeAlignedLeft, PlacementTarget = placementTarget};
            menu.Classes.Add(MenuStyle);
            menu.ItemsSource = BuildMenu(content, embedSeparators);
            return menu;
        }
        
        public static List<TemplatedControl> BuildMenu(Dictionary<string, EventHandler<RoutedEventArgs>?> content, bool embedSeparators = true)
        {
            var items = new List<TemplatedControl>();
            foreach(KeyValuePair<string, EventHandler<RoutedEventArgs>?> entry in content)
            {
                items.Add(BuildMenuItem(entry.Key, entry.Value));
                if (embedSeparators && !content.Last().Equals(entry))
                {
                    items.Add(BuildSeparator());
                }
            }
            return items;
        }
        
        public static MenuItem BuildMenuItem(string header, EventHandler<RoutedEventArgs>? onClickHandler = null)
        {
            var item = new MenuItem {Header = header};
            item.Classes.Add(MenuStyle);
            if (onClickHandler != null)
            {
                item.Click += onClickHandler;
            }

            return item;
        }
        
        public static Separator BuildSeparator()
        {
            var sep = new Separator();
            sep.Classes.Add(MenuStyle);
            return sep;
        }
    }
}