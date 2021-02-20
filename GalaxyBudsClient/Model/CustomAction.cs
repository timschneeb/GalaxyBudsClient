using System;
using System.IO;
using System.Linq;
using Avalonia.Input;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Utils.DynamicLocalization;
using Serilog;

namespace GalaxyBudsClient.Model
{
    public class CustomAction
    {
        public enum Actions
        {
            Event,
            [LocalizedDescription("touchoption_custom_trigger_hotkey")]
            TriggerHotkey,
            [LocalizedDescription("touchoption_custom_external_app")]
            RunExternalProgram
        }

        public Actions Action;

        public string Parameter;

        public EventDispatcher.Event Event
        {
            get
            {
                try
                {
                    return Enum.Parse<EventDispatcher.Event>(Parameter);
                }
                catch (Exception)
                {
                    return EventDispatcher.Event.None;
                }
            }
        }

        public CustomAction(Actions action, string parameter = "")
        {
            Action = action;
            Parameter = parameter;
        }
        
        public CustomAction(EventDispatcher.Event @event)
        {
            Action = Actions.Event;
            Parameter = @event.ToString();
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
                        Log.Error($"CustomAction.HotkeyBroadcast: Cannot parse saved key-combo: {ex.Message}");
                        Log.Error($"CustomAction.HotkeyBroadcast: Caused by combo: {Parameter}");
                        return Loc.Resolve("unknown");
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
}
