using System;
using System.Text;

namespace GalaxyBudsClient.Message.Decoder
{
    class SoftwareVersionOTAParser : BaseMessageParser
    {
        public override SppMessage.MessageIds HandledType => SppMessage.MessageIds.FOTA_DEVICE_INFO_SW_VERSION;

        public String? SoftwareVersion { set; get; }

        public override void ParseMessage(SppMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            SoftwareVersion = Encoding.ASCII.GetString(msg.Payload);
        }
    }
}
