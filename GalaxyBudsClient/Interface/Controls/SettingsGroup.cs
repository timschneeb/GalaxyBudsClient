using System;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;

namespace GalaxyBudsClient.Interface.Controls;

/// <summary>
/// Similar to a SettingsExpander, but without the expander control
/// </summary>
public class SettingsGroup : ItemsControl
{
    public SettingsGroup()
    {
        // Initially, no items are attached, so hide the group
        IsVisible = false;
        ItemsView.CollectionChanged += ItemsCollectionChanged;
    }

    private void ItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        IsVisible = ItemCount > 0;

        foreach (var control in e.NewItems?.OfType<Control>() ?? Enumerable.Empty<Control>())
        {
            control.GetPropertyChangedObservable(IsVisibleProperty)
                .Subscribe(_ => UpdateCorners());
        }
        
        UpdateCorners();
    }
    
    private void UpdateCorners()
    {
        // Ugly workaround to fix rounded corners if first/last children are hidden.
        // ClipToBounds doesn't work with rounded corners at the moment.
        
        // Handle single items
        var visibleItems = Items.OfType<Control>().Where(x => x.IsVisible).ToList();
        if(visibleItems.Count == 1 && visibleItems.First() is {} single)
        {
            single.Classes.Set("SingleItem", true);
            single.Classes.Set("First", false);
            single.Classes.Set("Last", false);
            return;
        }
        
        var firstItemFound = false;
        foreach (var item in Items.OfType<Control>())
        {
            // We already handle single items, so check for ItemCount > 1
            if (!firstItemFound)
            {
                firstItemFound = true;
                item.Classes.Set("First", item.IsVisible && ItemCount > 1);
            }
            item.Classes.Set("Last", false);
            item.Classes.Set("SingleItem", false);
        }

        var lastVisibleItem = Items.OfType<Control>().LastOrDefault(x => x.IsVisible);
        lastVisibleItem?.Classes.Set("Last", ItemCount > 1);
    }
}
