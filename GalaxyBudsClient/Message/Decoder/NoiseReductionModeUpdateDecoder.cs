using System;
using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.NOISE_REDUCTION_MODE_UPDATE)]
internal class NoiseReductionModeUpdateDecoder(SppMessage msg) : BaseMessageDecoder(msg)
{ 
    public bool Enabled { get; } = msg.Payload[0] == 1;
}