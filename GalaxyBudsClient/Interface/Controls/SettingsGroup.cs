using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using FluentAvalonia.UI.Controls;

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
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        var isItem = item is SettingsExpanderItem;
        recycleKey = isItem ? null : nameof(SettingsExpanderItem);
        return !isItem;
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var cont = this.FindDataTemplate(item, ItemTemplate)?.Build(item);

        if (cont is SettingsExpanderItem sei)
        {
            sei.DataContext = item;
            typeof(SettingsExpanderItem)
                .GetProperty("IsContainerFromTemplate")?.SetValue(sei, true, null);
            return sei;
        }

        return new SettingsExpanderItem();
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        var sei = container as SettingsExpanderItem;

        // If the container was created from a DataTemplate, do NOT call PrepareContainer or it will
        // do another template lookup and then put a item within an item as it sets the normal
        // ContentControl properties. Items created from a DataTemplate are assumed to be
        // initialized, to be sure the DataContext is set in CreateContainer
        if ((bool?)typeof(SettingsExpanderItem).GetProperty("IsContainerFromTemplate")?.GetValue(sei) == false)
            base.PrepareContainerForItemOverride(container, item, index);
    }
}
