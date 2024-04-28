using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;

namespace GalaxyBudsClient.Message;

public abstract class MessageAsDictionary
{   
    public Models TargetModel { set; get; } = BluetoothImpl.Instance.CurrentModel;
    protected IDeviceSpec DeviceSpec => DeviceSpecHelper.FindByModel(TargetModel) ?? new StubDeviceSpec();
    
    protected virtual bool IsHiddenProperty(MemberInfo property) => false;
    
    public virtual Dictionary<string, string> ToStringMap()
    {
        var map = new Dictionary<string, string>();
        var properties = GetType().GetProperties();
        foreach (var property in properties)
        {
            if (IsHiddenProperty(property) || property.Name is nameof(TargetModel) or nameof(DeviceSpec))
                continue;

            var devAttr = (DeviceAttribute[])property.GetCustomAttributes(typeof(DeviceAttribute), true);
            var postfixAttr = (PostfixAttribute[])property.GetCustomAttributes(typeof(PostfixAttribute), true);

            var postfix = string.Empty;
            if (postfixAttr.Length > 0)
            {
                postfix = postfixAttr[0].Text ?? string.Empty;
            }

            var value = property.GetValue(this);
            if (value is IEnumerable<byte> bytes)
            {
                value = HexUtils.Dump(bytes.ToArray(), showHeader: false, showOffset: false, showAscii: false);
            }
            
            if (value is MessageAsDictionary dict)
            {
                var nestedMap = dict.ToStringMap();
                Add(devAttr, property.Name, string.Empty, postfix);
                foreach (var (key, nestedValue) in nestedMap)
                {
                    Add(devAttr, $"\t\t{key}", nestedValue, postfix);
                }
            }
            else
            {
                Add(devAttr, property.Name, value?.ToString(), postfix);
            }
        }

        return map;

        void Add(IReadOnlyList<DeviceAttribute> devAttr, string name, string? value, string postfix)
        {
            if (devAttr.Count <= 0)
            {
                map.Add(name, (value ?? "null") + postfix);
            }
            else if (devAttr[0].Models.Contains(TargetModel))
            {
                map.Add($"{name} ({devAttr[0]})", (value ?? "null") + postfix);
            }
        }
    }
}