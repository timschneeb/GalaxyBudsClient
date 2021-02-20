using System;
using System.Collections.Generic;
using System.Reflection;

namespace GalaxyBudsClient.Message.Decoder
{
    public class BatteryTypeParser : BaseMessageParser
    {
        public override SPPMessage.MessageIds HandledType => SPPMessage.MessageIds.BATTERY_TYPE;

        public String? LeftBatteryType { set; get; }
        public String? RightBatteryType { set; get; }

        public override void ParseMessage(SPPMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            byte[] left = new byte[2];
            byte[] right = new byte[2];
            Array.Copy(msg.Payload, 0, left, 0, 2);
            Array.Copy(msg.Payload, 2, right, 0, 2);
            LeftBatteryType = System.Text.Encoding.ASCII.GetString(left);
            RightBatteryType = System.Text.Encoding.ASCII.GetString(right);
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
