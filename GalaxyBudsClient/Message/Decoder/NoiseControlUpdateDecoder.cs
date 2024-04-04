using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Decoder;

internal class NoiseControlUpdateDecoder : BaseMessageDecoder
{
    public override MsgIds HandledType => MsgIds.NOISE_CONTROLS_UPDATE;
    public NoiseControlModes Mode { set; get; }

    public override void Decode(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        Mode = (NoiseControlModes) msg.Payload[0];
    }
}