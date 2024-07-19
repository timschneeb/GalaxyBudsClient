using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;

namespace GalaxyBudsClient.Interface.MarkupExtensions;

/// <summary>
/// Behavior that hides the control if the app is running on non-desktop platforms.
/// </summary>
public class RequiresDesktopBehavior : Behavior<Control>
{
    public static readonly StyledProperty<bool> NotProperty =
        AvaloniaProperty.Register<RequiresDesktopBehavior, bool>(nameof(Not));
    
    public bool Not
    {
        get => GetValue(NotProperty);
        set => SetValue(NotProperty, value);
    }
    
    /// <inheritdoc />
    protected override void OnAttachedToVisualTree()
    {
        UpdateState();
    }

    private bool State => (PlatformUtils.IsDesktop && !Not) ||
                          (!PlatformUtils.IsDesktop && Not);
    protected void UpdateState()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (AssociatedObject is null)
                return;
        
            AssociatedObject.IsVisible = State;
        }, DispatcherPriority.Normal);
    }
}