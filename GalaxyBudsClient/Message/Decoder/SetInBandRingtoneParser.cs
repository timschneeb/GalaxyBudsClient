namespace GalaxyBudsClient.Message.Decoder;

/*
 * Buds+ only
 */
internal class SetInBandRingtoneParser : BaseMessageParser
{
    public override MsgIds HandledType => MsgIds.SET_IN_BAND_RINGTONE;

    public byte Status { set; get; }

    public override void ParseMessage(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        Status = msg.Payload[0];
    }
}