using System;
using System.Collections.Generic;
using System.Reflection;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Decoder
{
    class NoiseControlUpdateParser : BaseMessageParser
    {
        public override SPPMessage.MessageIds HandledType => SPPMessage.MessageIds.NOISE_CONTROLS_UPDATE;
        public NoiseControlMode Mode { set; get; }

        public override void ParseMessage(SPPMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            Mode = (NoiseControlMode) msg.Payload[0];
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
