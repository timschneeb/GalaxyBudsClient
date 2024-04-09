using System;
using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.TOUCH_UPDATED)]
internal class TouchUpdateDecoder : BaseMessageDecoder
{
    public bool TouchpadLocked { set; get; }

    public override void Decode(SppMessage msg)
    {
        TouchpadLocked = Convert.ToBoolean(msg.Payload[0]);
    }
}