using System;
using System.Collections.Generic;
using System.Reflection;
using GalaxyBudsClient.Message;

namespace GalaxyBudsClient.Decoder
{ 
    class MuteUpdateParser : BaseMessageParser
    {
        public override SPPMessage.MessageIds HandledType => SPPMessage.MessageIds.MSG_ID_MUTE_EARBUD_STATUS_UPDATED;

        public bool LeftMuted { set; get; }
        public bool RightMuted { set; get; }

        public override void ParseMessage(SPPMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            LeftMuted = Convert.ToBoolean(msg.Payload[0]);
            RightMuted = Convert.ToBoolean(msg.Payload[1]);
        }
        public override Dictionary<String, String> ToStringMap()
        {
            Dictionary<String, String> map = new Dictionary<string, string>();
            PropertyInfo[] properties = this.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (property.Name == "HandledType" || property.Name == "ActiveModel")
                    continue;

                map.Add(property.Name, property.GetValue(this).ToString());
            }

            return map;
        }
    }
}
