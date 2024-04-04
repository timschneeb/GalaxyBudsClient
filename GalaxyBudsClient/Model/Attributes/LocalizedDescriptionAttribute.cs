using System;
using System.ComponentModel;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;

namespace GalaxyBudsClient.Model.Attributes;

[AttributeUsage(AttributeTargets.All)]
public class LocalizedDescriptionAttribute(string key) : DescriptionAttribute(Loc.Resolve(key))
{
    public string Key { get; } = key;
}