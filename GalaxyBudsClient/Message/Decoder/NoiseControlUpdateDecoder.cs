using GalaxyBudsClient.Generated.Model.Attributes;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.NOISE_CONTROLS_UPDATE)]
internal class NoiseControlUpdateDecoder : BaseMessageDecoder
{
    public NoiseControlModes Mode { set; get; }
    public LegacyWearStates? WearingState { set; get; }

    public override void Decode(SppMessage msg)
    {
        Mode = (NoiseControlModes) msg.Payload[0];
        if (msg.Payload.Length > 1)
        {
            WearingState = (LegacyWearStates)msg.Payload[1];
        }
    }
}