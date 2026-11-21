using System;
using System.Collections.Generic;
using GalaxyBudsClient.Platform.Interfaces;
using GalaxyBudsClient.Platform.Model;
using GalaxyBudsClient.Utils.Extensions;
using ReactiveUI;

namespace GalaxyBudsClient.Model;

public partial class Hotkey : ReactiveObject, IHotkey
{
    private IEnumerable<ModifierKeys> _modifier = ArraySegment<ModifierKeys>.Empty;
    public IEnumerable<ModifierKeys> Modifier
    {
        get => _modifier;
        set => this.RaiseAndSetIfChanged(ref _modifier, value);
    }

    private IEnumerable<Keys> _keys = ArraySegment<Keys>.Empty;
    public IEnumerable<Keys> Keys
    {
        get => _keys;
        set => this.RaiseAndSetIfChanged(ref _keys, value);
    }

    private Event _action;
    public Event Action
    {
        get => _action;
        set => this.RaiseAndSetIfChanged(ref _action, value);
    }
    
    internal string ActionName => Action.GetLocalizedDescription();
    internal string HotkeyName => Keys.AsHotkeyString(Modifier);

    public override string ToString()
    {
        return Keys.AsHotkeyString(Modifier) + ": " + Action.GetLocalizedDescription();
    }

    public static Hotkey Empty => new();
}
