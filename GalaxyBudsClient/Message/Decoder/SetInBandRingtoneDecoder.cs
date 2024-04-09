using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

/*
 * Buds+ only
 */
[MessageDecoder(MsgIds.SET_IN_BAND_RINGTONE)]
internal class SetInBandRingtoneDecoder : BaseMessageDecoder
{
    public byte Status { set; get; }

    public override void Decode(SppMessage msg)
    {
        Status = msg.Payload[0];
    }
}