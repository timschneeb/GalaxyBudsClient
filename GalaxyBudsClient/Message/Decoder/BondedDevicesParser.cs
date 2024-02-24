using System;
using System.Collections.Generic;
using System.Reflection;

namespace GalaxyBudsClient.Message.Decoder
{
    class BondedDevicesParser : BaseMessageParser
    {
        public override SppMessage.MessageIds HandledType => SppMessage.MessageIds.UNK_BONDED_DEVICES;

        public String? Content { set; get; }

        public override void ParseMessage(SppMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            Content = System.Text.Encoding.ASCII.GetString(msg.Payload);
        }
    }
}
