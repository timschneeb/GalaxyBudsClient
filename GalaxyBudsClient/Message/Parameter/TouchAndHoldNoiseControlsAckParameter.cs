using System.IO;
using Features = GalaxyBudsClient.Model.Specifications.Features;

namespace GalaxyBudsClient.Message.Parameter;

public class TouchAndHoldNoiseControlsAckParameter : MessageAsDictionary, IAckParameter
{
    public bool LeftActiveNoiseCanceling { get; }
    public bool LeftAmbientSound { get; }
    public bool LeftOff { get; }
    public bool ActiveNoiseCanceling { get; }
    public bool AmbientSound { get; }
    public bool Off { get; }
    
    public TouchAndHoldNoiseControlsAckParameter(BinaryReader reader)
    {
        if (DeviceSpec.Supports(Features.NoiseControlModeDualSide))
        {
            LeftActiveNoiseCanceling = reader.ReadBoolean();
            LeftAmbientSound = reader.ReadBoolean();
            LeftOff = reader.ReadBoolean();
        }

        ActiveNoiseCanceling = reader.ReadBoolean();
        AmbientSound = reader.ReadBoolean();
        Off = reader.ReadBoolean();
    }
}