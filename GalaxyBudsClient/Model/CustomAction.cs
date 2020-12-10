using System;
using System.IO;

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
            if(Action == Actions.RunExternalProgram)
                return $"{Path.GetFileName(Parameter)}";
            return Action.GetDescription();
        }

        public string ToLongString()
        {
            if (Action == Actions.RunExternalProgram)
                return $"{Action.GetDescription()} ({Path.GetFileName(Parameter)})";
            return Action.GetDescription();
        }
    }
}
