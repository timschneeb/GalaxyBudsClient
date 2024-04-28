using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.AMBIENT_VOLUME)]
internal class AmbientVolumeDecoder(SppMessage msg) : BaseMessageDecoder(msg)
{ 
    public int AmbientVolume { get; } = msg.Payload[0];
}