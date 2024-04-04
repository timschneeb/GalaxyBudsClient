using System;

namespace GalaxyBudsClient.Message.Decoder;

internal class TouchUpdateDecoder : BaseMessageDecoder
{
    public override MsgIds HandledType => MsgIds.TOUCH_UPDATED;
    public bool TouchpadLocked { set; get; }

    public override void Decode(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        TouchpadLocked = Convert.ToBoolean(msg.Payload[0]);
    }
}