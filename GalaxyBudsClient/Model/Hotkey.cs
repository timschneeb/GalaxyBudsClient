using System;
using System.Collections.Generic;
using GalaxyBudsClient.Platform.Interfaces;
using GalaxyBudsClient.Platform.Model;
using GalaxyBudsClient.Utils.Extensions;
using ReactiveUI;

namespace GalaxyBudsClient.Model;

public partial class Hotkey : ReactiveObject, IHotkey
{
    [Reactive] private IEnumerable<ModifierKeys> _modifier = ArraySegment<ModifierKeys>.Empty;
    [Reactive] private IEnumerable<Keys> _keys = ArraySegment<Keys>.Empty;
    [Reactive] private Event _action;
    
    internal string ActionName => Action.GetLocalizedDescription();
    internal string HotkeyName => Keys.AsHotkeyString(Modifier);

    public override string ToString()
    {
        return Keys.AsHotkeyString(Modifier) + ": " + Action.GetLocalizedDescription();
    }

    public static Hotkey Empty => new();
}
