using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;

namespace GalaxyBudsClient.Message.Decoder
{
    public abstract class BaseMessageParser
    {
        public abstract SppMessage.MessageIds HandledType { get; }

        public abstract void ParseMessage(SppMessage msg);
        public virtual Dictionary<string, string> ToStringMap()
        {
            var map = new Dictionary<string, string>();
            var properties = this.GetType().GetProperties();
            foreach (var property in properties)
            {
                if (IsHiddenProperty(property))
                    continue;

                var devAttr = (DeviceAttribute[])property.GetCustomAttributes(typeof(DeviceAttribute), true);
                var postfixAttr = (PostfixAttribute[])property.GetCustomAttributes(typeof(PostfixAttribute), true);

                var postfix = string.Empty;
                if (postfixAttr.Length > 0)
                {
                    postfix = postfixAttr[0].Text ?? string.Empty;
                }
                
                if (devAttr.Length <= 0)
                {
                    map.Add(property.Name, (property.GetValue(this)?.ToString() ?? "null") + postfix);
                }
                else if (devAttr[0].Models.Contains(ActiveModel))
                {
                    map.Add($"{property.Name} ({devAttr[0]})", (property.GetValue(this)?.ToString() ?? "null") + postfix);
                }
            }

            return map;
        }
        
        protected static bool IsHiddenProperty(PropertyInfo property)
        {
            return property.Name is "HandledType" or "ActiveModel" or "DeviceSpec";
        }

        protected static Models ActiveModel => BluetoothImpl.Instance.ActiveModel;
        protected static IDeviceSpec DeviceSpec => BluetoothImpl.Instance.DeviceSpec;
    }
}
