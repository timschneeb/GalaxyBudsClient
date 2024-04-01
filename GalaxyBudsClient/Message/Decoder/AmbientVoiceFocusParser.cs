using System;

namespace GalaxyBudsClient.Message.Decoder;

internal class AmbientVoiceFocusParser : BaseMessageParser
{
    public override MsgIds HandledType => MsgIds.AMBIENT_VOICE_FOCUS;
    public bool VoiceFocusEnabled { set; get; }

    public override void ParseMessage(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        VoiceFocusEnabled = Convert.ToBoolean(msg.Payload[0]);
    }
}