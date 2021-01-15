using System;
using System.Collections.Generic;
using System.Reflection;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Decoder
{
    class LogCoredumpDataParser : BaseMessageParser
    {
        public override SPPMessage.MessageIds HandledType => SPPMessage.MessageIds.LOG_COREDUMP_DATA;
        
        public int PartialDataOffset { set; get; }
        public short PartialDataSize { set; get; }
        public byte[] RawData { set; get; } = new byte[0];

        public override void ParseMessage(SPPMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            PartialDataOffset = BitConverter.ToInt32(msg.Payload, 0);
            PartialDataSize = BitConverter.ToInt16(msg.Payload, 4);
            RawData = new byte[PartialDataSize];
            Array.Copy(msg.Payload, 6, RawData, 0, PartialDataSize);
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
