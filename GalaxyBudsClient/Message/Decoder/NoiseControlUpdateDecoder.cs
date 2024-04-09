using GalaxyBudsClient.Generated.Model.Attributes;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.NOISE_CONTROLS_UPDATE)]
internal class NoiseControlUpdateDecoder : BaseMessageDecoder
{
    public NoiseControlModes Mode { set; get; }

    public override void Decode(SppMessage msg)
    {
        Mode = (NoiseControlModes) msg.Payload[0];
    }
}