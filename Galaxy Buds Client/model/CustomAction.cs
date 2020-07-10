using System;
using System.ComponentModel;
using System.Linq;

namespace Galaxy_Buds_Client.model
{
    public class CustomAction
    {
        public enum Actions
        {
            [Description("Ambient Volume Up")]
            AmbientVolumeUp,
            [Description("Ambient Volume Down")]
            AmbientVolumeDown,
            [Description("Toggle Equalizer")]
            EnableEqualizer,
            [Description("Next Equalizer Preset")]
            SwitchEqualizerPreset,
            [Description("Press Hotkey...")]
            Hotkey,
            [Description("Run External Program...")]
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
                return $"{Action.GetDescription()} ({Parameter})";
            if(Action == Actions.Hotkey && Parameter.Contains(";"))
                return $"{Action.GetDescription()} ({Parameter.Split(';')[0]})";
            return Action.GetDescription();
        }
    }
}
