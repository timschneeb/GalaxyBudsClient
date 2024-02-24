using System;
using System.Collections.Generic;
using System.Reflection;

namespace GalaxyBudsClient.Message.Decoder
{
    class AmbientModeUpdateParser : BaseMessageParser
    {
        public override SppMessage.MessageIds HandledType => SppMessage.MessageIds.AMBIENT_MODE_UPDATED;
        public bool Enabled { set; get; }

        public override void ParseMessage(SppMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            Enabled = Convert.ToBoolean(msg.Payload[0]);
        }
    }
}
