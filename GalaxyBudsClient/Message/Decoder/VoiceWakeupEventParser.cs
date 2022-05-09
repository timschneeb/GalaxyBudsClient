using System;
using System.Collections.Generic;
using System.Reflection;

namespace GalaxyBudsClient.Message.Decoder
{
    class VoiceWakeupEventParser : BaseMessageParser
    {
        public override SPPMessage.MessageIds HandledType => SPPMessage.MessageIds.VOICE_WAKE_UP_EVENT;
        public byte ResultCode { set; get; }
        public byte Confidence { set; get; }

        public override void ParseMessage(SPPMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            ResultCode = msg.Payload[0];
            if (msg.Payload.Length > 1)
            {
                Confidence = msg.Payload[1];
            }
        }
    }
}
