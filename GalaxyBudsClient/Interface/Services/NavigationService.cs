using System;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;

namespace GalaxyBudsClient.Interface.Services;

public class NavigationService
{
    public static NavigationService Instance { get; } = new();
    public Frame? Frame { private get; set; }
    
    public void Navigate(Type t)
    {
        Frame?.Navigate(t);
    }

    public void NavigateFromContext(object dataContext, NavigationTransitionInfo? transitionInfo = null)
    {
        Frame?.NavigateFromObject(dataContext,
            new FluentAvalonia.UI.Navigation.FrameNavigationOptions
            {
                IsNavigationStackEnabled = true,
                TransitionInfoOverride = transitionInfo ?? new SuppressNavigationTransitionInfo()
            });
    }
}