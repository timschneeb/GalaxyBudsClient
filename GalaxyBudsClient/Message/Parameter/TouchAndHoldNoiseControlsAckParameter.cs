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
        if (DeviceSpec.Device >= Model.Constants.Models.Buds3)
        {
            // Buds3 and newer only echo back the raw request bytes. The
            // official app has removed all code related to this ACK event.
            return;
        }

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