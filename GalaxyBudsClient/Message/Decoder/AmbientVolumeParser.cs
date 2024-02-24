namespace GalaxyBudsClient.Message.Decoder
{
    class AmbientVolumeParser : BaseMessageParser
    {
        public override SppMessage.MessageIds HandledType => SppMessage.MessageIds.AMBIENT_VOLUME;
        public int AmbientVolume { set; get; }

        public override void ParseMessage(SppMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            AmbientVolume = msg.Payload[0];
        }
    }
}
