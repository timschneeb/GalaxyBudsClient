using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Decoder
{
    class SpatialAudioControlParser : BaseMessageParser
    {
        public override SppMessage.MessageIds HandledType => SppMessage.MessageIds.SPATIAL_AUDIO_CONTROL;
        public SpatialAudioControl ResultCode { set; get; }

        public override void ParseMessage(SppMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            ResultCode = (SpatialAudioControl) msg.Payload[0];
        }
    }
}
