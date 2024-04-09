using System;
using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder((MsgIds)LegacyMsgIds.AMBIENT_VOICE_FOCUS)]
internal class AmbientVoiceFocusDecoder : BaseMessageDecoder
{
    public bool VoiceFocusEnabled { set; get; }

    public override void Decode(SppMessage msg)
    {
        VoiceFocusEnabled = Convert.ToBoolean(msg.Payload[0]);
    }
}