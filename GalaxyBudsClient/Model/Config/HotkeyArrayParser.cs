using System;
using System.Collections.Generic;
using Config.Net;
using GalaxyBudsClient.Model.Hotkeys;
using Newtonsoft.Json;
using Serilog;

namespace GalaxyBudsClient.Model.Config;

public class HotkeyArrayParser : ITypeParser
{
    public IEnumerable<Type> SupportedTypes => new[] { typeof(Hotkey[]) };

    public string? ToRawString(object? value)
    {
        if (value is Hotkey[] enumerable)
        {
            return JsonConvert.SerializeObject(enumerable);
        }
        return null;
    }

    public bool TryParse(string? value, Type t, out object? result)
    {
        if(string.IsNullOrEmpty(value))
        {
            result = Array.Empty<Hotkey>();
            return false;
        }

        if(t == typeof(Hotkey[]))
        {
            try
            {
                result = JsonConvert.DeserializeObject<Hotkey[]>(value);
            }
            catch (Exception ex)
            {
                Log.Warning("HotkeyArrayParser: Exception raised ({Message})", ex.Message);
                result = Array.Empty<Hotkey>();
                return false;
            }

            return true;
        }

        result = Array.Empty<Hotkey>();
        return false;
    }
}