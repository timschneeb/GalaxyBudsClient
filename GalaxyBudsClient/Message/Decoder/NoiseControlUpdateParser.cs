using System;
using System.Collections.Generic;
using System.Reflection;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Decoder
{
    class NoiseControlUpdateParser : BaseMessageParser
    {
        public override SppMessage.MessageIds HandledType => SppMessage.MessageIds.NOISE_CONTROLS_UPDATE;
        public NoiseControlMode Mode { set; get; }

        public override void ParseMessage(SppMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            Mode = (NoiseControlMode) msg.Payload[0];
        }
    }
}
