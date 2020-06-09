using System;
using System.Collections.Generic;
using Galaxy_Buds_Client.message;
using Galaxy_Buds_Client.model;

namespace Galaxy_Buds_Client.parser
{
    public abstract class BaseMessageParser
    {
        public abstract SPPMessage.MessageIds HandledType { get; }

        public abstract void ParseMessage(SPPMessage msg);
        public abstract Dictionary<String, String> ToStringMap();
    }
}
