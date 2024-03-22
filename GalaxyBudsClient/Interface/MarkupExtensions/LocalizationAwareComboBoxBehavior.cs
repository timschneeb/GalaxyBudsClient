using System;
using System.Linq;
using System.Runtime.Serialization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;
using GalaxyBudsClient.Interface.Controls;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;

namespace GalaxyBudsClient.Interface.MarkupExtensions;

/*
 * Workaround: Until UpdateSourceTrigger.Explicit is usable in Avalonia,
 * this behavior is used to update the source of a ComboBox in case of localization changes. 
 */
public class LocalizationAwareComboBoxBehavior : Behavior<Control>
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
        if (AssociatedObject is SettingsComboBoxItem cb)
        {
            var selected = cb.SelectedValue;
            cb.ItemsSource = Enum.GetValues(EnumType)
                .OfType<object>()
                .Where(obj => EnumType.GetMember(EnumType.GetEnumName((int)obj) ?? string.Empty)[0]
                    .GetCustomAttributes(typeof(IgnoreDataMemberAttribute), false)
                    .Length <= 0).ToArray();
            cb.SelectedValue = selected;
        }
    }
}