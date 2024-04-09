using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.VOICE_WAKE_UP_EVENT)]
internal class VoiceWakeupEventDecoder : BaseMessageDecoder
{ 
    public byte ResultCode { set; get; }
    public byte Confidence { set; get; }

    public override void Decode(SppMessage msg)
    {
        ResultCode = msg.Payload[0];
        if (msg.Payload.Length > 1)
        {
            Confidence = msg.Payload[1];
        }
    }
}