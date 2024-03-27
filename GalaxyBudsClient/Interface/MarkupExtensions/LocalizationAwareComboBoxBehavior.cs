using System;
using System.Linq;
using Avalonia;
using Avalonia.Xaml.Interactivity;
using GalaxyBudsClient.Interface.Controls;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;

namespace GalaxyBudsClient.Interface.MarkupExtensions;

/*
 * Workaround: Until UpdateSourceTrigger.Explicit is usable in Avalonia,
 * this behavior is used to update the source of a ComboBox in case of localization changes. 
 */
public class LocalizationAwareComboBoxBehavior : Behavior<SettingsComboBoxItem>
{
    
    public static readonly StyledProperty<Type> EnumTypeProperty =
        AvaloniaProperty.Register<LocalizationAwareComboBoxBehavior, Type>(nameof(EnumType));
    
    public Type EnumType
    {
        get => GetValue(EnumTypeProperty);
        set => SetValue(EnumTypeProperty, value);
    }
    
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
        cb.ItemsSource = Enum.GetValues(EnumType!)
            .OfType<Enum>()
            .Where(obj => obj.IsPlatformConditionMet())
            .Where(obj => !obj.IsMemberIgnored())
            .ToArray();
        cb.SelectedValue = selected;
    }
}