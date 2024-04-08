using Avalonia.Xaml.Interactivity;
using GalaxyBudsClient.Interface.Controls;
using GalaxyBudsClient.Utils.Interface;

namespace GalaxyBudsClient.Interface.MarkupExtensions;

/*
 * Workaround: Until UpdateSourceTrigger.Explicit is usable in Avalonia,
 * this behavior is used to update the source of a ComboBox in case of localization changes. 
 */
public class LocalizationAwareComboBoxBehavior : Behavior<SettingsComboBoxItem>
{
    /// <inheritdoc />
    protected override void OnAttachedToVisualTree()
    {
        UpdateState();
        Loc.LanguageUpdated += OnLanguageUpdated;
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree()
    {
        Loc.LanguageUpdated -= OnLanguageUpdated;
    }
    
    private void OnLanguageUpdated() => UpdateState();
    
    private void UpdateState()
    {
        // Handle combo box updates
        if (AssociatedObject is not { } cb)
            return;
        
        var selected = cb.SelectedValue;
        
        // Workaround: trigger update by setting ItemsSource to null and back
        var bak = cb.ItemsSource;
        cb.ItemsSource = null;
        cb.ItemsSource = bak;
        cb.SelectedValue = selected;
    }
}