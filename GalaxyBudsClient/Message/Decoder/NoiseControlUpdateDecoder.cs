using GalaxyBudsClient.Generated.Model.Attributes;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.NOISE_CONTROLS_UPDATE)]
internal class NoiseControlUpdateDecoder : BaseMessageDecoder
{
    public NoiseControlModes Mode { get; }
    public LegacyWearStates? WearingState { get; }

    public NoiseControlUpdateDecoder(SppMessage msg) : base(msg)
    {
        Mode = (NoiseControlModes) msg.Payload[0];
        if (msg.Payload.Length > 1)
        {
            WearingState = (LegacyWearStates)msg.Payload[1];
        }
    }
}