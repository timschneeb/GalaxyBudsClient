using System;
using System.Collections.Generic;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Platform;

namespace GalaxyBudsClient.Decoder
{
    public abstract class BaseMessageParser
    {
        public abstract SPPMessage.MessageIds HandledType { get; }

        public abstract void ParseMessage(SPPMessage msg);
        public abstract Dictionary<String, String> ToStringMap();

        public Model.Constants.Models ActiveModel => BluetoothImpl.Instance.ActiveModel;
    }
}
