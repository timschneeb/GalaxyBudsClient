using System;
using System.Collections.Generic;
using System.Reflection;

namespace GalaxyBudsClient.Message.Decoder
{
    class DebugBuildInfoParser : BaseMessageParser
    {
        public override SPPMessage.MessageIds HandledType => SPPMessage.MessageIds.DEBUG_BUILD_INFO;
        
        public String? BuildString { set; get; }

        public override void ParseMessage(SPPMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            BuildString = System.Text.Encoding.ASCII.GetString(msg.Payload);
        }
    }
}