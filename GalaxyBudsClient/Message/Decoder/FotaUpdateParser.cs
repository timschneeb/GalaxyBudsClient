using System;
using System.Collections.Generic;
using System.Reflection;
using GalaxyBudsClient.Model.Firmware;

namespace GalaxyBudsClient.Message.Decoder
{
    class FotaUpdateParser : BaseMessageParser
    {
        public override SPPMessage.MessageIds HandledType => SPPMessage.MessageIds.FOTA_UPDATE;

        public byte ResultCode { set; get; }
        public FirmwareConstants.UpdateIds UpdateId { set; get; }
        public byte Percent { set; get; }
        public byte State { set; get; }
        
        public override void ParseMessage(SPPMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            UpdateId = (FirmwareConstants.UpdateIds) msg.Payload[0];
            switch (UpdateId)
            {
                case FirmwareConstants.UpdateIds.Percent:
                    Percent = msg.Payload[1];
                    break;
                case FirmwareConstants.UpdateIds.StateChange:
                    State = msg.Payload[1];
                    break;
            }
            
            ResultCode = msg.Payload[2];
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