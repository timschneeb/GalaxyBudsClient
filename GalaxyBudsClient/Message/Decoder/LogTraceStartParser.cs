using System;
using System.Collections.Generic;
using System.Reflection;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Decoder
{
    class LogTraceStartParser : BaseMessageParser
    {
        public override SPPMessage.MessageIds HandledType => SPPMessage.MessageIds.LOG_TRACE_START;
        
        public int DataSize { set; get; }
        public short PartialDataMaxSize { set; get; }
        public DeviceInv DeviceType { set; get; }
        public bool Coupled { set; get; }
        public int FragmentCount { set; get; }

        public override void ParseMessage(SPPMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            DataSize = BitConverter.ToInt32(msg.Payload, 0);
            PartialDataMaxSize = BitConverter.ToInt16(msg.Payload, 4);
            DeviceType = (DeviceInv) msg.Payload[6];
            Coupled = msg.Payload[7] == 0;
            FragmentCount = (int)Math.Ceiling((double)DataSize/(double)PartialDataMaxSize);
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
