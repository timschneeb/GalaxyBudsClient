using System;
using System.Collections.Generic;
using System.Reflection;
using GalaxyBudsClient.Model.Firmware;

namespace GalaxyBudsClient.Message.Decoder
{
    class FotaResultParser : BaseMessageParser
    {
        public override SPPMessage.MessageIds HandledType => SPPMessage.MessageIds.FOTA_RESULT;

        public byte Result { set; get; }
        public byte ErrorCode { set; get; }

        public override void ParseMessage(SPPMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            Result = msg.Payload[0];
            ErrorCode = msg.Payload[1];
        }
    }
}