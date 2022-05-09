using System;
using System.Collections.Generic;
using System.Reflection;

namespace GalaxyBudsClient.Message.Decoder
{ 
    public class MuteUpdateParser : BaseMessageParser
    {
        public override SPPMessage.MessageIds HandledType => SPPMessage.MessageIds.MUTE_EARBUD_STATUS_UPDATED;

        public bool LeftMuted { set; get; }
        public bool RightMuted { set; get; }

        public override void ParseMessage(SPPMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            LeftMuted = Convert.ToBoolean(msg.Payload[0]);
            RightMuted = Convert.ToBoolean(msg.Payload[1]);
        }
    }
}
