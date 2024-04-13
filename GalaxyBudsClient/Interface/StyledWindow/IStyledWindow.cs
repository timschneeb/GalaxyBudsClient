using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using FluentAvalonia.UI.Media;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Utils;

namespace GalaxyBudsClient.Interface.StyledWindow;

public interface IStyledWindow : IThemeVariantHost
{
    protected IReadOnlyList<WindowTransparencyLevel> DefaultTransparencyLevelHint { get; }
    protected void ApplyBackgroundBrush(IBrush? brush);

    public static ThemeVariant? GetThemeVariant()
    {
        return LegacySettings.Instance.Theme switch
        {
            Themes.Light => ThemeVariant.Light,
            Themes.Dark => ThemeVariant.Dark,
            Themes.DarkBlur => ThemeVariant.Dark,
            Themes.DarkMica => ThemeVariant.Dark,
            _ => null
        };
    }

    protected static bool IsSolid() => LegacySettings.Instance.Theme is Themes.Light or Themes.Dark;
    
    public void ApplyTheme(TopLevel host)
    {
        host.RequestedThemeVariant = GetThemeVariant();
            
        if (!IsSolid())
        {
            TryEnableMicaEffect(host);
        }
        else
        {
            host.TransparencyLevelHint = DefaultTransparencyLevelHint;
            ApplyBackgroundBrush(null);
            host.ClearValue(TopLevel.TransparencyBackgroundFallbackProperty);
        }
    }
    
    private void TryEnableMicaEffect(TopLevel host)
    {
        host.TransparencyBackgroundFallback = Brushes.Transparent;
        if (LegacySettings.Instance.Theme is Themes.DarkMica)
        {
            host.TransparencyLevelHint = new[]
            {
                WindowTransparencyLevel.Mica, WindowTransparencyLevel.AcrylicBlur, WindowTransparencyLevel.Blur
            };
        }
        else
        {
            host.TransparencyLevelHint = new[]
            {
                WindowTransparencyLevel.AcrylicBlur, WindowTransparencyLevel.Blur
            };
        }

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
        color = color.WithAlpha(LegacySettings.Instance.BlurStrength);

        ApplyBackgroundBrush(new ImmutableSolidColorBrush(color));
    } 
}