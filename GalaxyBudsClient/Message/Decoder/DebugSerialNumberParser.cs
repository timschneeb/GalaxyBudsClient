using System;
using System.Collections.Generic;
using System.Reflection;

namespace GalaxyBudsClient.Message.Decoder
{
    public class DebugSerialNumberParser : BaseMessageParser
    {
        public override SPPMessage.MessageIds HandledType => SPPMessage.MessageIds.DEBUG_SERIAL_NUMBER;

        public String? LeftSerialNumber { set; get; }
        public String? RightSerialNumber { set; get; }

        public override void ParseMessage(SPPMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            byte[] left = new byte[11];
            byte[] right = new byte[11];
            Array.Copy(msg.Payload, 0, left, 0, 11);
            Array.Copy(msg.Payload, 11, right, 0, 11);
            LeftSerialNumber = System.Text.Encoding.ASCII.GetString(left, 0, 11);
            RightSerialNumber = System.Text.Encoding.ASCII.GetString(right, 0, 11);
        }

        public override Dictionary<String, String> ToStringMap()
        {
            Dictionary<String, String> map = new Dictionary<string, string>();
            PropertyInfo[] properties = this.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (property.Name == "HandledType" || property.Name == "ActiveModel")
                    continue;

                map.Add(property.Name, property.GetValue(this)?.ToString() ?? "null");
            }

            return map;
        }
    }
}