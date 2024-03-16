using System;
using System.Text;

namespace GalaxyBudsClient.Message.Decoder
{
    class DebugBuildInfoParser : BaseMessageParser
    {
        public override SppMessage.MessageIds HandledType => SppMessage.MessageIds.DEBUG_BUILD_INFO;
        
        public string? BuildString { set; get; }

        public override void ParseMessage(SppMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            BuildString = Encoding.ASCII.GetString(msg.Payload);
        }
    }
}