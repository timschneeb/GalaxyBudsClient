namespace GalaxyBudsClient.Message.Decoder;

internal class VoiceWakeupEventDecoder : BaseMessageDecoder
{
    public override MsgIds HandledType => MsgIds.VOICE_WAKE_UP_EVENT;
    public byte ResultCode { set; get; }
    public byte Confidence { set; get; }

    public override void Decode(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        ResultCode = msg.Payload[0];
        if (msg.Payload.Length > 1)
        {
            Confidence = msg.Payload[1];
        }
    }
}