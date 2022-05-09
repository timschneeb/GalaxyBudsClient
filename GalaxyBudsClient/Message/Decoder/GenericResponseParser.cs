using System;
using System.Collections.Generic;
using System.Reflection;

namespace GalaxyBudsClient.Message.Decoder
{
    public class GenericResponseParser : BaseMessageParser
    {
        public override SPPMessage.MessageIds HandledType => SPPMessage.MessageIds.RESP;
        public SPPMessage.MessageIds MessageId { set; get; }
        public int ResultCode { set; get; }
        public int? ExtraData { set; get; }

        public override void ParseMessage(SPPMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            MessageId = (SPPMessage.MessageIds) msg.Payload[0];
            ResultCode = msg.Payload[1];
            if (msg.Payload.Length > 2)
            {
                ExtraData = msg.Payload[2];
            }
        }
    }
}