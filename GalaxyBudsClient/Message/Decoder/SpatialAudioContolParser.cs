using GalaxyBudsClient.Generated.Model.Attributes;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.SPATIAL_AUDIO_CONTROL)]
internal class SpatialAudioControlDecoder(SppMessage msg) : BaseMessageDecoder(msg)
{
    public SpatialAudioControl ResultCode { get; } = (SpatialAudioControl) msg.Payload[0];
}