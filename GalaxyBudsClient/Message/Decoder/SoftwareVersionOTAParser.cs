using System;
using System.Collections.Generic;
using System.Reflection;

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

            SoftwareVersion = System.Text.Encoding.ASCII.GetString(msg.Payload);
        }
    }
}
