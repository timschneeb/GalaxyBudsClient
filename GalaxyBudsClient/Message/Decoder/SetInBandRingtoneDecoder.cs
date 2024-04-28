using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

/*
 * Buds+ only
 */
[MessageDecoder(MsgIds.SET_IN_BAND_RINGTONE)]
internal class SetInBandRingtoneDecoder(SppMessage msg) : BaseMessageDecoder(msg)
{
    public byte Status { get; } = msg.Payload[0];
}