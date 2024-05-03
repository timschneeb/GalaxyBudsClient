using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Media;
using FluentAvalonia.UI.Windowing;
using GalaxyBudsClient.Model.Config;

namespace GalaxyBudsClient.Interface.StyledWindow;

public class StyledAppWindow : AppWindow, IStyledWindow
{
    protected StyledAppWindow()
    {
        Settings.MainSettingsPropertyChanged += OnMainSettingsPropertyChanged;
    }

    public IReadOnlyList<WindowTransparencyLevel> DefaultTransparencyLevelHint =>
        Array.Empty<WindowTransparencyLevel>();

    public virtual void ApplyBackgroundBrush(IBrush? brush)
    {
        if(brush == null)
            ClearValue(BackgroundProperty);
        else
            Background = brush;
    }
    
    protected override void OnOpened(EventArgs e)
    {
        (this as IStyledWindow).ApplyTheme(this);
        base.OnOpened(e);
    }
    
    private void OnMainSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(e.PropertyName is nameof(Settings.Data.Theme) or nameof(Settings.Data.BlurStrength))
        {
            (this as IStyledWindow).ApplyTheme(this);
        }
    }
}