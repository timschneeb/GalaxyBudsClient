using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Xaml.Interactivity;
using GalaxyBudsClient.Interface.Services;

namespace GalaxyBudsClient.Interface.MarkupExtensions;


public class NavigationToPageAction : AvaloniaObject, IAction
{
    public static readonly StyledProperty<Type> PageTypeProperty =
        AvaloniaProperty.Register<NavigationToPageAction, Type>(nameof(PageType));
    
    public Type PageType
    {
        get => GetValue(PageTypeProperty);
        set => SetValue(PageTypeProperty, value);
    }

    public virtual object Execute(object? sender, object? parameter)
    {
        NavigationService.Instance.Navigate(PageType);
        return true;
    }
}