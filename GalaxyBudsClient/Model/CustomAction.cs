using System;
using System.IO;
using System.Linq;
using Avalonia.Input;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;
using Serilog;

namespace GalaxyBudsClient.Model
{
    public class CustomAction(CustomAction.Actions action, string parameter = "")
    {
        public enum Actions
        {
            Event,
            [LocalizedDescription("touchoption_custom_trigger_hotkey")]
            TriggerHotkey,
            [LocalizedDescription("touchoption_custom_external_app")]
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
