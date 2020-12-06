using System;
using System.Collections.Generic;
using GalaxyBudsClient.Message;

namespace GalaxyBudsClient.Decoder
{
    public abstract class BaseMessageParser
    {
        public abstract SPPMessage.MessageIds HandledType { get; }

        public abstract void ParseMessage(SPPMessage msg);
        public abstract Dictionary<String, String> ToStringMap();

        public Model.Constants.Models ActiveModel => BluetoothService.Instance.ActiveModel;
    }
}
