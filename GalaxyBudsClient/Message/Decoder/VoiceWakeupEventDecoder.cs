using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.VOICE_WAKE_UP_EVENT)]
internal class VoiceWakeupEventDecoder : BaseMessageDecoder
{ 
    public byte ResultCode { get; }
    public byte Confidence { get; }

    public VoiceWakeupEventDecoder(SppMessage msg) : base(msg)
    {
        ResultCode = msg.Payload[0];
        if (msg.Payload.Length > 1)
        {
            Confidence = msg.Payload[1];
        }
    }
}