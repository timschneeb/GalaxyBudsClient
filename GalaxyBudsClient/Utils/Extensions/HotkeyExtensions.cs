using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Input;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Platform.Model;

namespace GalaxyBudsClient.Utils.Extensions;

public static class HotkeyExtensions
{
    public static bool Compare(this Hotkey h1, Hotkey h2)
    {
        return h1.Keys.AsHotkeyString(h1.Modifier) == h2.Keys.AsHotkeyString(h2.Modifier) && h1.Action == h2.Action;
    }

    public static string AsAvaloniaHotkeyString(this IEnumerable<Key>? keys)
    {
        if (keys == null)
            return "null";

        return string.Join('+', keys);
    }
        
    public static string AsHotkeyString(this IEnumerable<Keys>? keys, IEnumerable<ModifierKeys>? modifiers)
    {
        if (keys == null && modifiers == null)
        {
            return "null";
        }

        var parts = new List<string>();
            
        foreach (var modifier in modifiers ?? [])
        {
            parts.Add(modifier.ToString());
        }
            
        foreach (var key in keys ?? [])
        {
            parts.Add(key.ToString());
        }

        return string.Join('+', parts);
    }
}