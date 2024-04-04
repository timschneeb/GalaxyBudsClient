namespace GalaxyBudsClient.Message.Decoder;

/*
 * Buds+ only
 */
internal class SetInBandRingtoneDecoder : BaseMessageDecoder
{
    public override MsgIds HandledType => MsgIds.SET_IN_BAND_RINGTONE;

    public byte Status { set; get; }

    public override void Decode(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        Status = msg.Payload[0];
    }
}