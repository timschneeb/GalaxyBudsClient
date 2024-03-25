using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using FluentAvalonia.UI.Media;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Utils;

namespace GalaxyBudsClient.Interface;

public class StyledWindow : Window
{
    protected StyledWindow()
    {
        Settings.Instance.PropertyChanged += OnMainSettingsPropertyChanged;
    }

    protected virtual IReadOnlyList<WindowTransparencyLevel> DefaultTransparencyLevelHint => Array.Empty<WindowTransparencyLevel>();
    protected virtual void ApplyBackgroundBrush(IBrush? brush)
    {
        if(brush == null)
            ClearValue(BackgroundProperty);
        else
            Background = brush;
    }
    
    protected override void OnOpened(EventArgs e)
    {
        ApplyTheme();
        base.OnOpened(e);
    }

    private static ThemeVariant? GetThemeVariant()
    {
        return Settings.Instance.Theme switch
        {
            Themes.Light => ThemeVariant.Light,
            Themes.Dark => ThemeVariant.Dark,
            Themes.DarkBlur => ThemeVariant.Dark,
            _ => null
        };
    }
    
    private void OnMainSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(e.PropertyName is nameof(Settings.Instance.Theme) or nameof(Settings.Instance.BlurStrength))
        {
            ApplyTheme();
        }
    }

    private void ApplyTheme()
    {
        RequestedThemeVariant = GetThemeVariant();
            
        if (Settings.Instance.Theme == Themes.DarkBlur)
        {
            TryEnableMicaEffect();
        }
        else
        {
            TransparencyLevelHint = DefaultTransparencyLevelHint;
            ApplyBackgroundBrush(null);
            ClearValue(TransparencyBackgroundFallbackProperty);
        }
    }
    
    private void TryEnableMicaEffect()
    {
        // TODO test on Windows
        
        TransparencyBackgroundFallback = Brushes.Transparent;
        TransparencyLevelHint = new[]
            { WindowTransparencyLevel.Mica, WindowTransparencyLevel.AcrylicBlur, WindowTransparencyLevel.Blur };
        
        // The background colors for the Mica brush are still based around SolidBackgroundFillColorBase resource
        // BUT since we can't control the actual Mica brush color, we have to use the window background to create
        // the same effect. However, we can't use SolidBackgroundFillColorBase directly since its opaque, and if
        // we set the opacity the color become lighter than we want. So we take the normal color, darken it and 
        // apply the opacity until we get the roughly the correct color
        // NOTE that the effect still doesn't look right, but it suffices. Ideally we need access to the Mica
        // CompositionBrush to properly change the color but I don't know if we can do that or not
        var color = this.TryFindResource("SolidBackgroundFillColorBase",
            ThemeVariant.Dark, out var value) ? (Color2)(Color)value! : new Color2(32, 32, 32);

        color = color.LightenPercent(-0.8f);
        color = color.WithAlpha(Settings.Instance.BlurStrength);

        ApplyBackgroundBrush(new ImmutableSolidColorBrush(color));
    } 
}