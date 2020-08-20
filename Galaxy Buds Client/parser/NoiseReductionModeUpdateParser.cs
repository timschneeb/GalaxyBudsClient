using Galaxy_Buds_Client.message;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Galaxy_Buds_Client.parser
{
    class NoiseReductionModeUpdateParser : BaseMessageParser
    {
        public override SPPMessage.MessageIds HandledType => SPPMessage.MessageIds.MSG_ID_NOISE_REDUCTION_MODE_UPDATE;
        public bool Enabled { set; get; }

        public override void ParseMessage(SPPMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            Enabled = Convert.ToBoolean(msg.Payload[0]);
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
