using System;
using System.Collections.Generic;
using GalaxyBudsClient.Platform.Interfaces;
using GalaxyBudsClient.Platform.Model;
using GalaxyBudsClient.Utils.Extensions;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GalaxyBudsClient.Model;

public class Hotkey : ReactiveObject, IHotkey
{
    [Reactive] public IEnumerable<ModifierKeys> Modifier { set; get; } = ArraySegment<ModifierKeys>.Empty; 
    [Reactive] public IEnumerable<Keys> Keys { set; get; } = ArraySegment<Keys>.Empty;
    [Reactive] public Event Action { set; get; }
    
    internal string ActionName => Action.GetLocalizedDescription();
    internal string HotkeyName => Keys.AsHotkeyString(Modifier);

    public override string ToString()
    {
        return Keys.AsHotkeyString(Modifier) + ": " + Action.GetLocalizedDescription();
    }

    public static Hotkey Empty => new();
}