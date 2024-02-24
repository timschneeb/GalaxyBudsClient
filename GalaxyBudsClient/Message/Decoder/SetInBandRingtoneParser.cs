namespace GalaxyBudsClient.Message.Decoder
{
    /*
     * Buds+ only
     */
    class SetInBandRingtoneParser : BaseMessageParser
    {
        public override SppMessage.MessageIds HandledType => SppMessage.MessageIds.SET_IN_BAND_RINGTONE;

        public byte Status { set; get; }

        public override void ParseMessage(SppMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            Status = msg.Payload[0];
        }
    }
}