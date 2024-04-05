using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Xaml.Interactivity;
using GalaxyBudsClient.Interface.Controls;
using GalaxyBudsClient.Utils.Interface;

namespace GalaxyBudsClient.Interface.MarkupExtensions;

/*
 * Workaround: Until UpdateSourceTrigger.Explicit is usable in Avalonia,
 * this behavior is used to update the current description of a SettingsSliderItem in case of localization changes. 
 */
public class LocalizationAwareSliderBehavior : Behavior<SettingsSliderItem>
{
    
    public static readonly StyledProperty<IValueConverter> ConverterProperty =
        AvaloniaProperty.Register<LocalizationAwareSliderBehavior, IValueConverter>(nameof(Converter));
    
    public IValueConverter Converter
    {
        get => GetValue(ConverterProperty);
        set => SetValue(ConverterProperty, value);
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
        // Handle slider updates
        if (AssociatedObject is { } sl)
        {
            sl.Description = Converter.Convert(sl.Value, typeof(string), null, CultureInfo.CurrentCulture) as string;
        }
    }
}