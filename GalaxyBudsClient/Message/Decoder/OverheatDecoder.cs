using System;
using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.OVERHEAT)]
public class OverheatDecoder(SppMessage msg) : BaseMessageDecoder(msg)
{
    public bool IsOverheating { get; } = Convert.ToBoolean(msg.Payload[0]);
}