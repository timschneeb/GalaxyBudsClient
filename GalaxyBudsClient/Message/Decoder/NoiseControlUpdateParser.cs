using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Decoder;

internal class NoiseControlUpdateParser : BaseMessageParser
{
    public override MsgIds HandledType => MsgIds.NOISE_CONTROLS_UPDATE;
    public NoiseControlModes Mode { set; get; }

    public override void ParseMessage(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        Mode = (NoiseControlModes) msg.Payload[0];
    }
}