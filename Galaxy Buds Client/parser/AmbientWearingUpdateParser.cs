using System;
using System.Collections.Generic;
using System.Reflection;
using Galaxy_Buds_Client.message;
using Galaxy_Buds_Client.model;

namespace Galaxy_Buds_Client.parser
{
    /*
     * Mostly unused if (versionOfMR < 2). Refer to ExtendedStatusUpdateParser
     */
    class AmbientWearingUpdateParser : BaseMessageParser
    {
        public override SPPMessage.MessageIds HandledType => SPPMessage.MessageIds.MSG_ID_AMBIENT_WEARING_STATUS_UPDATED;

        public Constants.WearStates WearState { set; get; }
        public int LeftDetectionCount { set; get; }
        public int RightDetectionCount { set; get; }

        public override void ParseMessage(SPPMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            WearState = (Constants.WearStates) msg.Payload[0];
            LeftDetectionCount = BitConverter.ToInt16(msg.Payload, 1);
            RightDetectionCount = BitConverter.ToInt16(msg.Payload, 3);
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
