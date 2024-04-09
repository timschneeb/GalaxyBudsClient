using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.AMBIENT_VOLUME)]
internal class AmbientVolumeDecoder : BaseMessageDecoder
{ 
    public int AmbientVolume { set; get; }

    public override void Decode(SppMessage msg)
    {
        AmbientVolume = msg.Payload[0];
    }
}