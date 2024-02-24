using System;
using System.Collections.Generic;
using System.Reflection;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Decoder
{
    class SetOtherOptionParser : BaseMessageParser
    {
        public override SppMessage.MessageIds HandledType => SppMessage.MessageIds.SET_TOUCHPAD_OTHER_OPTION;
        public TouchOptions OptionType { set; get; }

        public override void ParseMessage(SppMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            OptionType = DeviceSpec.TouchMap.FromByte(msg.Payload[0]);
        }
    }
}
