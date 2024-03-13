using Avalonia;
using Avalonia.Xaml.Interactivity;
using GalaxyBudsClient.Model;

namespace GalaxyBudsClient.Interface.MarkupExtensions;


public class EventDispatcherAction : AvaloniaObject, IAction
{
    public static readonly StyledProperty<EventDispatcher.Event> EventProperty =
        AvaloniaProperty.Register<EventDispatcherAction, EventDispatcher.Event>(nameof(Event));
    
    public EventDispatcher.Event Event
    {
        get => GetValue(EventProperty);
        set => SetValue(EventProperty, value);
    }

    public object Execute(object? sender, object? parameter)
    {
        EventDispatcher.Instance.Dispatch(Event);
        return true;
    }
}