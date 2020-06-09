using System;
using System.Collections.Generic;
using System.Reflection;
using Galaxy_Buds_Client.message;
using Galaxy_Buds_Client.model;

namespace Galaxy_Buds_Client.parser
{
    class StatusUpdateParser : BaseMessageParser
    {

        public override SPPMessage.MessageIds HandledType => SPPMessage.MessageIds.MSG_ID_STATUS_UPDATED;
        public int EarType { set; get; }
        public int BatteryL { set; get; }
        public int BatteryR { set; get; }
        public bool IsCoupled { set; get; }
        public Constants.DeviceInv MainConnection { set; get; }
        public Constants.WearStates WearState { set; get; }

        public override void ParseMessage(SPPMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            EarType = msg.Payload[0];
            BatteryL = msg.Payload[1];
            BatteryR = msg.Payload[2];
            IsCoupled = Convert.ToBoolean(msg.Payload[3]);
            MainConnection = (Constants.DeviceInv) msg.Payload[4];
            WearState = (Constants.WearStates) msg.Payload[5];
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
