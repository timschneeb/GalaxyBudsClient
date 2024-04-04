using System;

namespace GalaxyBudsClient.Message.Decoder;

internal class NoiseReductionModeUpdateDecoder : BaseMessageDecoder
{
    public override MsgIds HandledType => MsgIds.NOISE_REDUCTION_MODE_UPDATE;
    public bool Enabled { set; get; }

    public override void Decode(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        Enabled = Convert.ToBoolean(msg.Payload[0]);
    }
}