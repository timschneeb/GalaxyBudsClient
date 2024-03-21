using System;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Model.Attributes;

public class DeviceAttribute : Attribute
{
    public DeviceAttribute(Models[] models)
    {
        Models = models;
    }
    public DeviceAttribute(Models model)
    {
        Models = new Models[1];
        Models[0] = model;
    }

    public override string ToString()
    {
        var str = string.Empty;
        var i = 0;
        foreach (var model in Models)
        {
            str += model.ToString();
            if (i < Models.Length - 1)
                str += ", ";

            i++;
        }
        return str;
    }

    public readonly Models[] Models;
}