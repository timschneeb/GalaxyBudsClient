using System.Collections.Specialized;
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
        Classes.Set("SingleItem", ItemCount == 1);
    }
}
