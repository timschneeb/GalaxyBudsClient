using System;
using System.ComponentModel;

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
            [Description("Run external program...")]
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
            return Action.GetDescription();
        }
    }
}
