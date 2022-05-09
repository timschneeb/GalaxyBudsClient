using System;
using System.Collections.Generic;
using System.Reflection;

namespace GalaxyBudsClient.Message.Decoder
{
    class AmbientVoiceFocusParser : BaseMessageParser
    {
        public override SPPMessage.MessageIds HandledType => SPPMessage.MessageIds.AMBIENT_VOICE_FOCUS;
        public bool VoiceFocusEnabled { set; get; }

        public override void ParseMessage(SPPMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            VoiceFocusEnabled = Convert.ToBoolean(msg.Payload[0]);
        }
    }
}
