using System;

namespace GalaxyBudsClient.Model.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class PostfixAttribute : Attribute
{
    public string? Text { get; set; }
}