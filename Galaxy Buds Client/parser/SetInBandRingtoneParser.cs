using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Galaxy_Buds_Client.message;

namespace Galaxy_Buds_Client.parser
{
    /*
     * Buds+ only
     */
    class SetInBandRingtoneParser : BaseMessageParser
    {
        public override SPPMessage.MessageIds HandledType => SPPMessage.MessageIds.MSG_ID_SET_IN_BAND_RINGTONE;

        public byte Status { set; get; }

        public override void ParseMessage(SPPMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            Status = msg.Payload[0];
        }

        public override Dictionary<String, String> ToStringMap()
        {
            Dictionary<String, String> map = new Dictionary<string, string>();
            PropertyInfo[] properties = this.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (property.Name == "HandledType" || property.Name == "ActiveModel")
                    continue;

                map.Add(property.Name, property.GetValue(this)?.ToString());
            }

            return map;
        }
    }
}