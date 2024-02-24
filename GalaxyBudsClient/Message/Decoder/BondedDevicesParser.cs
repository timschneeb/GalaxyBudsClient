using System;
using System.Text;

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

            Content = Encoding.ASCII.GetString(msg.Payload);
        }
    }
}
