using System;

namespace GalaxyBudsClient.Message.Decoder;

internal class AmbientModeUpdateParser : BaseMessageParser
{
    public override MsgIds HandledType => MsgIds.AMBIENT_MODE_UPDATED;
    public bool Enabled { set; get; }

    public override void ParseMessage(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        Enabled = Convert.ToBoolean(msg.Payload[0]);
    }
}