using System;
using System.IO;
using System.Linq;
using Avalonia.Input;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Utils.Interface;
using Serilog;

namespace GalaxyBudsClient.Model;

public class CustomAction(CustomAction.Actions action, string parameter = "")
{
    public enum Actions
    {
        [LocalizedDescription(Keys.TouchoptionCustomTriggerEvent)]
        Event,
        [LocalizedDescription(Keys.TouchoptionCustomTriggerHotkey)]
        TriggerHotkey,
        [LocalizedDescription(Keys.TouchoptionCustomExternalApp)]
        RunExternalProgram
    }

    public readonly Actions Action = action;

    public readonly string Parameter = parameter;

    public Event Event
    {
        get
        {
            try
            {
                return Enum.Parse<Event>(Parameter);
            }
            catch (Exception)
            {
                return Event.None;
            }
        }
    }

    public CustomAction(Event @event) : this(Actions.Event, @event.ToString())
    {
    }
        
    public override string ToString()
    {
        switch (Action)
        {
            case Actions.Event:
                return Event.GetDescription();
            case Actions.RunExternalProgram:
                return $"{Path.GetFileName(Parameter)}";
            case Actions.TriggerHotkey:
                try
                {
                    return string.Join('+', Parameter.Split(',').Select(Enum.Parse<Key>));
                }
                catch (Exception ex)
                {
                    Log.Error("CustomAction.HotkeyBroadcast: Cannot parse saved key-combo: {Message}", ex.Message);
                    Log.Error("CustomAction.HotkeyBroadcast: Caused by combo: {Parameter}", Parameter);
                    return Strings.Unknown;
                }
        }

        return Action.GetDescription();
    }

    public string ToLongString()
    {
        switch (Action)
        {
            case Actions.RunExternalProgram:
            case Actions.TriggerHotkey:
                return $"{Action.GetDescription()} ({ToString()})";
            case Actions.Event:
                return Event.GetDescription();
            default:
                return Action.GetDescription();
        }
    }
}