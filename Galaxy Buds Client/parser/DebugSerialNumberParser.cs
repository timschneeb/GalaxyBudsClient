using System;
using System.Collections.Generic;
using System.Reflection;
using Galaxy_Buds_Client.message;
using Galaxy_Buds_Client.model;

namespace Galaxy_Buds_Client.parser
{
    class DebugSerialNumberParser : BaseMessageParser
    {
        public override SPPMessage.MessageIds HandledType => SPPMessage.MessageIds.MSG_ID_DEBUG_SERIAL_NUMBER;

        public String LeftSerialNumber { set; get; }
        public String RightSerialNumber { set; get; }

        public override void ParseMessage(SPPMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            byte[] left = new byte[11];
            byte[] right = new byte[11];
            Array.Copy(msg.Payload, 0, left, 0, 11);
            Array.Copy(msg.Payload, 12, right, 0, 11);
            LeftSerialNumber = System.Text.Encoding.ASCII.GetString(left);
            RightSerialNumber = System.Text.Encoding.ASCII.GetString(right);
        }

        public override Dictionary<String, String> ToStringMap()
        {
            Dictionary<String, String> map = new Dictionary<string, string>();
            PropertyInfo[] properties = this.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (property.Name == "HandledType")
                    continue;

                map.Add(property.Name, property.GetValue(this).ToString());
            }

            return map;
        }
    }
}