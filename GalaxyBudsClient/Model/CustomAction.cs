using System;
using System.IO;
using System.Linq;
using Avalonia.Input;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Generated.Model.Attributes;
using GalaxyBudsClient.Model.Attributes;
using Serilog;

namespace GalaxyBudsClient.Model;

public class CustomAction(CustomAction.Actions action, string parameter = "")
{
    [CompiledEnum]
    public enum Actions
    {
        [LocalizableDescription(Keys.TouchoptionCustomTriggerEvent)]
        Event,
        [LocalizableDescription(Keys.TouchoptionCustomTriggerHotkey)]
        TriggerHotkey,
        [LocalizableDescription(Keys.TouchoptionCustomExternalApp)]
        RunExternalProgram
    }

    public readonly Actions Action = action;

    public readonly string Parameter = parameter;

    public Event Event => EventExtensions.TryParse(Parameter, out var value) ? value : Event.None;

    public CustomAction(Event @event) : this(Actions.Event, @event.ToString())
    {
    }
        
    public override string ToString()
    {
        switch (Action)
        {
            case Actions.Event:
                return Event.GetLocalizedDescription();
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
        return Action.GetLocalizedDescription();
    }
}