using System;

namespace GalaxyBudsClient.Message.Decoder;

internal class TouchUpdateParser : BaseMessageParser
{
    public override SppMessage.MessageIds HandledType => SppMessage.MessageIds.TOUCH_UPDATED;
    public bool TouchpadLocked { set; get; }

    public override void ParseMessage(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        TouchpadLocked = Convert.ToBoolean(msg.Payload[0]);
    }
}