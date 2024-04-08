using System;
using System.ComponentModel;
using GalaxyBudsClient.Utils.Interface;

namespace GalaxyBudsClient.Model.Attributes;

[AttributeUsage(AttributeTargets.All)]
public class LocalizableDescriptionAttribute(string key) : Attribute
{
    public string Key { get; } = key;
    public string Value => Loc.Resolve(Key);
}