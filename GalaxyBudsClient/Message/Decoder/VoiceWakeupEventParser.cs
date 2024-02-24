namespace GalaxyBudsClient.Message.Decoder
{
    class VoiceWakeupEventParser : BaseMessageParser
    {
        public override SppMessage.MessageIds HandledType => SppMessage.MessageIds.VOICE_WAKE_UP_EVENT;
        public byte ResultCode { set; get; }
        public byte Confidence { set; get; }

        public override void ParseMessage(SppMessage msg)
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
}
