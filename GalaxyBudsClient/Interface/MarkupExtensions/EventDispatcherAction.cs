using Avalonia;
using Avalonia.Xaml.Interactivity;
using GalaxyBudsClient.Model;

namespace GalaxyBudsClient.Interface.MarkupExtensions;


public class EventDispatcherAction : AvaloniaObject, IAction
{
    public static readonly StyledProperty<Event> EventProperty =
        AvaloniaProperty.Register<EventDispatcherAction, Event>(nameof(Event));
    
    public Event Event
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