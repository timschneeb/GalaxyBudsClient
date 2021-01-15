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
            [LocalizedDescription("touchoption_custom_ambient_up")]
            AmbientVolumeUp,
            [LocalizedDescription("touchoption_custom_ambient_down")]
            AmbientVolumeDown,
            [LocalizedDescription("touchoption_custom_toggle_eq")]
            EnableEqualizer,
            [LocalizedDescription("touchoption_custom_next_eq_preset")]
            SwitchEqualizerPreset,
            [LocalizedDescription("touchoption_custom_trigger_hotkey")]
            TriggerHotkey,
            [LocalizedDescription("touchoption_custom_external_app")]
            RunExternalProgram
        }

        public Actions Action;

        public String Parameter;

        public CustomAction(Actions action, String parameter = "")
        {
            Action = action;
            Parameter = parameter;
        }
        public override string ToString()
        {
            switch (Action)
            {
                case Actions.RunExternalProgram:
                    return $"{Path.GetFileName(Parameter)}";
                case Actions.TriggerHotkey:
                    try
                    {
                        return string.Join('+', Parameter.Split(',').Select(Enum.Parse<Key>));
                    }
                    catch (Exception ex)
                    {
                        Log.Error("CustomAction.HotkeyBroadcast: Cannot parse saved key-combo: " + ex.Message);
                        Log.Error("CustomAction.HotkeyBroadcast: Caused by combo: " + Parameter);
                        return Loc.Resolve("unknown");
                    }
                    break;
            }

            return Action.GetDescription();
        }

        public string ToLongString()
        {
            if (Action == Actions.RunExternalProgram || Action == Actions.TriggerHotkey)
            {
                return $"{Action.GetDescription()} ({ToString()})";
            }

            return Action.GetDescription();
        }
    }
}
