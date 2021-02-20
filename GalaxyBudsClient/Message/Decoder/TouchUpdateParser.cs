using System;
using System.Collections.Generic;
using System.Reflection;

namespace GalaxyBudsClient.Message.Decoder
{
    class TouchUpdateParser : BaseMessageParser
    {
        public override SPPMessage.MessageIds HandledType => SPPMessage.MessageIds.TOUCH_UPDATED;
        public bool TouchpadLocked { set; get; }

        public override void ParseMessage(SPPMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            TouchpadLocked = Convert.ToBoolean(msg.Payload[0]);
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
