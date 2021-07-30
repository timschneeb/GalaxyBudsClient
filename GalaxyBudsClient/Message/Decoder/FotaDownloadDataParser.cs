using System;
using System.Collections.Generic;
using System.Reflection;

namespace GalaxyBudsClient.Message.Decoder
{
    class FotaDownloadDataParser : BaseMessageParser
    {
        public override SPPMessage.MessageIds HandledType => SPPMessage.MessageIds.FOTA_DOWNLOAD_DATA;

        public bool NAK { set; get; }
        public long ReceivedOffset { set; get; }
        public byte RequestPacketNumber { set; get; }

        public override void ParseMessage(SPPMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            NAK = (msg.Payload[3] & -128) != 0;
            ReceivedOffset = (((long) msg.Payload[1] & 255) << 8) | ((long) msg.Payload[0] & 255) |
                             (((long) msg.Payload[2] & 255) << 16) | (((long) msg.Payload[3] & 127) << 24);
            RequestPacketNumber = msg.Payload[4];
        }

        public override Dictionary<String, String> ToStringMap()
        {
            Dictionary<String, String> map = new Dictionary<string, string>();
            PropertyInfo[] properties = this.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (property.Name == "HandledType" || property.Name == "ActiveModel")
                    continue;

                map.Add(property.Name, property?.GetValue(this)?.ToString() ?? "null");
            }

            return map;
        }
    }
}