using System;
using System.Collections.Generic;
using System.Reflection;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Decoder
{
    class SpatialAudioControlParser : BaseMessageParser
    {
        public override SPPMessage.MessageIds HandledType => SPPMessage.MessageIds.SPATIAL_AUDIO_CONTROL;
        public SpatialAudioControl ResultCode { set; get; }

        public override void ParseMessage(SPPMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            ResultCode = (SpatialAudioControl) msg.Payload[0];
        }
    }
}
