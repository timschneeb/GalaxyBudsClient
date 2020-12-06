using System;
using System.Collections.Generic;
using System.Reflection;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Decoder
{
    class SetOtherOptionParser : BaseMessageParser
    {
        public override SPPMessage.MessageIds HandledType => SPPMessage.MessageIds.MSG_ID_SET_TOUCHPAD_OTHER_OPTION;
        public TouchOption.Universal OptionType { set; get; }

        public override void ParseMessage(SPPMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            OptionType = TouchOption.ToUniversal(msg.Payload[0]);
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
