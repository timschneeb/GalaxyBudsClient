﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;

namespace GalaxyBudsClient.Message;

public abstract class BaseMessageHandler
{
    public Models TargetModel { set; get; } = BluetoothImpl.ActiveModel;
    public abstract MsgIds HandledType { get; }
    protected IDeviceSpec DeviceSpec => DeviceSpecHelper.FindByModel(TargetModel) ?? new StubDeviceSpec();

    public virtual Dictionary<string, string> ToStringMap()
    {
        var map = new Dictionary<string, string>();
        var properties = GetType().GetProperties();
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
            else if (devAttr[0].Models.Contains(TargetModel))
            {
                map.Add($"{property.Name} ({devAttr[0]})", (property.GetValue(this)?.ToString() ?? "null") + postfix);
            }
        }

        return map;
    }
    protected static bool IsHiddenProperty(MemberInfo property)
    {
        return property.Name is nameof(HandledType) or nameof(DeviceSpec) or nameof(TargetModel);
    }
}