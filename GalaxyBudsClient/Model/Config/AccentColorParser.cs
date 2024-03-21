using System;
using System.Collections.Generic;
using Avalonia.Media;
using Config.Net;
using Serilog;

namespace GalaxyBudsClient.Model.Config;

public class AccentColorParser : ITypeParser
{
    public IEnumerable<Type> SupportedTypes => new[] { typeof(Color) };
    public static Color DefaultColor { get; } = Colors.Orange;

    public string? ToRawString(object? value)
    {
        if (value is Color color)
        {
            return color.ToUInt32().ToString();
        }
        return DefaultColor.ToUInt32().ToString();
    }

    public bool TryParse(string? value, Type t, out object? result)
    {
        if(string.IsNullOrEmpty(value))
        {
            result = DefaultColor;
            return true;
        }
            
        try
        {
            result = Color.FromUInt32(Convert.ToUInt32(value));
        }
        catch (FormatException ex)
        {
            Log.Warning(ex, "AccentColorParser: FormatException raised");
            result = DefaultColor;
        }
        return true;
    }
}