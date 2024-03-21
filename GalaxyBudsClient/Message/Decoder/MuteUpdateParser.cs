using System;

namespace GalaxyBudsClient.Message.Decoder;

public class MuteUpdateParser : BaseMessageParser
{
    public override SppMessage.MessageIds HandledType => SppMessage.MessageIds.MUTE_EARBUD_STATUS_UPDATED;

    public bool LeftMuted { set; get; }
    public bool RightMuted { set; get; }

    public override void ParseMessage(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        LeftMuted = Convert.ToBoolean(msg.Payload[0]);
        RightMuted = Convert.ToBoolean(msg.Payload[1]);
    }
}