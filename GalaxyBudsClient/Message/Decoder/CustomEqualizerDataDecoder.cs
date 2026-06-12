using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

/*
 * Response to a CUSTOM_EQUALIZE_RECV query.
 * Payload: [presetCount][bandCount][(presetCount - 1) * bandCount preset table][bandCount custom band gains]
 * The flat first preset is omitted from the table on the wire.
 */
[MessageDecoder(MsgIds.CUSTOM_EQUALIZE_RECV)]
internal class CustomEqualizerDataDecoder : BaseMessageDecoder
{
    public int PresetCount { get; }
    public int BandCount { get; }
    public sbyte[] CustomBands { get; }

    public CustomEqualizerDataDecoder(SppMessage msg) : base(msg)
    {
        PresetCount = msg.Payload.Length > 0 ? msg.Payload[0] : 0;
        BandCount = msg.Payload.Length > 1 ? msg.Payload[1] : 0;
        CustomBands = new sbyte[BandCount];

        var offset = 2 + (PresetCount - 1) * BandCount;
        for (var i = 0; i < BandCount && offset + i < msg.Payload.Length; i++)
        {
            CustomBands[i] = (sbyte)msg.Payload[offset + i];
        }
    }
}
