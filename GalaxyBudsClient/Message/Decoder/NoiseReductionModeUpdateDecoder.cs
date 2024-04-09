using System;
using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.NOISE_REDUCTION_MODE_UPDATE)]
internal class NoiseReductionModeUpdateDecoder : BaseMessageDecoder
{ 
    public bool Enabled { set; get; }

    public override void Decode(SppMessage msg)
    {
        Enabled = Convert.ToBoolean(msg.Payload[0]);
    }
}