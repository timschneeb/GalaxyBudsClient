using System;
using System.Collections.Generic;
using GalaxyBudsClient.Utils.Extensions;

namespace GalaxyBudsClient.Model.Hotkeys;

public class Hotkey(IEnumerable<ModifierKeys> modifier, IEnumerable<Keys> keys, Event action)
{
    public IEnumerable<ModifierKeys> Modifier { get; } = modifier;
    public IEnumerable<Keys> Keys { get; } = keys;
    public Event Action { get; } = action;
    internal string ActionName => Action.GetLocalizedDescription() ?? string.Empty;
    internal string HotkeyName => Keys.AsHotkeyString(Modifier);

    public override string ToString()
    {
        return Keys.AsHotkeyString(Modifier) + ": " + Action.GetLocalizedDescription();
    }

    public static Hotkey Empty => new(ArraySegment<ModifierKeys>.Empty, ArraySegment<Keys>.Empty, Event.None);
}