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
    LessEqual
}

[AttributeUsage(AttributeTargets.Property)]
public class DeviceAttribute : Attribute
{
    private readonly Selector _selector = Selector.Equal;
    
    public DeviceAttribute(params Models[] models)
    {
        Models = models;
    }
    
    public DeviceAttribute(Models model, Selector selector = Selector.Equal)
    {
        var devices = ModelsExtensions.GetValues().Where(x => x != Constants.Models.NULL);
        _selector = selector;
        
        Models = selector switch
        {
            Selector.Equal => [model],
            Selector.NotEqual => devices.Where(x => x != model),
            Selector.GreaterEqual => devices.Where(x => x >= model),
            Selector.LessEqual => devices.Where(x => x <= model),
            _ => throw new InvalidOperationException("Invalid selector")
        };
    }

    public override string ToString()
    {
        return _selector switch
        {
            Selector.GreaterEqual => $"{Models.FirstOrDefault()} and above",
            Selector.LessEqual => $"{Models.LastOrDefault()} and below",
            _ => string.Join(',', Models)
        };
    }

    public readonly IEnumerable<Models> Models;
}