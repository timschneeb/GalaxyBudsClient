using System;
using System.Collections.Generic;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;

namespace GalaxyBudsClient.Message.Decoder
{
    public abstract class BaseMessageParser
    {
        public abstract SPPMessage.MessageIds HandledType { get; }

        public abstract void ParseMessage(SPPMessage msg);
        public abstract Dictionary<String, String> ToStringMap();

        public Model.Constants.Models ActiveModel => BluetoothImpl.Instance.ActiveModel;
        public IDeviceSpec DeviceSpec => BluetoothImpl.Instance.DeviceSpec;
    }
}
