using System;
using System.Collections.Generic;
using Config.Net;
using Newtonsoft.Json;
using Serilog;

namespace GalaxyBudsClient.Model.Config;

public class DeviceArrayParser : ITypeParser
{
    public IEnumerable<Type> SupportedTypes => new[] { typeof(IDevice[]) };

    public string? ToRawString(object? value)
    {
        if (value is IDevice[] enumerable)
        {
            return JsonConvert.SerializeObject(enumerable);
        }
        return null;
    }

    public bool TryParse(string? value, Type t, out object? result)
    {
        if(string.IsNullOrEmpty(value))
        {
            result = Array.Empty<IDevice>();
            return false;
        }

        if(t == typeof(IDevice[]))
        {
            try
            {
                result = JsonConvert.DeserializeObject<IDevice[]>(value);
            }
            catch (Exception ex)
            {
                Log.Warning("DeviceArrayParser: Exception raised ({Message})", ex.Message);
                result = Array.Empty<IDevice>();
                return false;
            }

            return true;
        }

        result = Array.Empty<IDevice>();
        return false;
    }
}