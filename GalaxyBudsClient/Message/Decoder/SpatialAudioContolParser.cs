using GalaxyBudsClient.Generated.Model.Attributes;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.SPATIAL_AUDIO_CONTROL)]
internal class SpatialAudioControlDecoder : BaseMessageDecoder
{
    public SpatialAudioControl ResultCode { set; get; }

    public override void Decode(SppMessage msg)
    {
        ResultCode = (SpatialAudioControl) msg.Payload[0];
    }
}