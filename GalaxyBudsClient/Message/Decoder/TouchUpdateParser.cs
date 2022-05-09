using System;
using System.Collections.Generic;
using System.Reflection;

namespace GalaxyBudsClient.Message.Decoder
{
    class TouchUpdateParser : BaseMessageParser
    {
        public override SPPMessage.MessageIds HandledType => SPPMessage.MessageIds.TOUCH_UPDATED;
        public bool TouchpadLocked { set; get; }

        public override void ParseMessage(SPPMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            TouchpadLocked = Convert.ToBoolean(msg.Payload[0]);
        }
    }
}
