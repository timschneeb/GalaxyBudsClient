using System;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using FluentAvalonia.UI.Navigation;

namespace GalaxyBudsClient.Interface.Services;

public class NavigationService
{
    public static NavigationService Instance { get; } = new();
    public Frame? Frame { private get; set; }
    
    /// <summary>
    /// Navigate by type of the view model attached to the page
    /// </summary>
    public void Navigate(Type t, NavigationTransitionInfo? transitionInfo = null)
    {
        Frame?.Navigate(t, null, transitionInfo);
    }
    
    /// <summary>
    /// Navigate by short name of the page
    /// </summary>
    public void Navigate(string? name, NavigationTransitionInfo? transitionInfo = null)
    {        
        var targetType = Type.GetType($"GalaxyBudsClient.Interface.ViewModels.Pages.{name}PageViewModel");
        if (targetType != null)
            Navigate(targetType, transitionInfo);
    }

    public void NavigateFromContext(object dataContext, NavigationTransitionInfo? transitionInfo = null)
    {
        Frame?.NavigateFromObject(dataContext,
            new FrameNavigationOptions
            {
                IsNavigationStackEnabled = true,
                TransitionInfoOverride = transitionInfo ?? new SuppressNavigationTransitionInfo()
            });
    }
}