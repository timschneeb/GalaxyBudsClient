using System;
using System.ComponentModel;
using Avalonia.Media;
using GalaxyBudsClient.Utils;

namespace GalaxyBudsClient.Interface.StyledWindow;

public class StyledWindow : Avalonia.Controls.Window, IStyledWindow
{
    protected StyledWindow()
    {
        Settings.Instance.PropertyChanged += OnMainSettingsPropertyChanged;
    }

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
        if(e.PropertyName is nameof(Settings.Instance.Theme) or nameof(Settings.Instance.BlurStrength))
        {
            (this as IStyledWindow).ApplyTheme(this);
        }
    }
}