using System;
using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.TOUCH_UPDATED)]
internal class TouchUpdateDecoder(SppMessage msg) : BaseMessageDecoder(msg)
{
    public bool TouchpadLocked { get; } = Convert.ToBoolean(msg.Payload[0]);
}