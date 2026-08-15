using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Encoder;

/*
 * Payload: [bandCount][bandCount signed band gains], gains range from -10 to +10.
 * Applies only while the custom preset is selected via the EQUALIZER message.
 */
[MessageEncoder(MsgIds.CUSTOM_EQUALIZE_SEND)]
public class SetCustomEqualizerEncoder : BaseMessageEncoder
{
    public sbyte[] BandGains { get; init; } = new sbyte[9];

    public override SppMessage Encode()
    {
        var payload = new byte[BandGains.Length + 1];
        payload[0] = (byte)BandGains.Length;
        for (var i = 0; i < BandGains.Length; i++)
        {
            payload[i + 1] = (byte)BandGains[i];
        }

        return new SppMessage(MsgIds.CUSTOM_EQUALIZE_SEND, MsgTypes.Request, payload);
    }
}
