using System;
using System.Collections.Generic;
using System.Linq;
using GalaxyBudsClient.Model.Constants;
using InvalidOperationException = System.InvalidOperationException;

namespace GalaxyBudsClient.Model.Attributes;

public enum Selector
{
    Equal,
    NotEqual,
    GreaterEqual,
    LessEqual,
}

public class DeviceAttribute : Attribute
{
    public DeviceAttribute(params Models[] models)
    {
        Models = models;
    }
    
    public DeviceAttribute(Models model, Selector selector = Selector.Equal)
    {
        var devices = (Models[])Enum.GetValues(typeof(Models));

        Models = selector switch
        {
            Selector.Equal => [model],
            Selector.NotEqual => devices.Where(x => x != model),
            Selector.GreaterEqual => devices.Where(x => x >= model),
            Selector.LessEqual => devices.Where(x => x <= model),
            _ => throw new InvalidOperationException("Invalid selector")
        };
    }

    public override string ToString() => string.Join(",", Models);

    public readonly IEnumerable<Models> Models;
}