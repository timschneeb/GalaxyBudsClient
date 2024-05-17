using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder((MsgIds)LegacyMsgIds.AMBIENT_VOICE_FOCUS)]
internal class AmbientVoiceFocusDecoder(SppMessage msg) : BaseMessageDecoder(msg)
{
    public bool VoiceFocusEnabled { get; } = msg.Payload[0] == 1;
}