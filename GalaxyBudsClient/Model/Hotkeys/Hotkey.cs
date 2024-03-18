using System;
using System.Collections.Generic;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Utils.Extensions;

namespace GalaxyBudsClient.Model.Hotkeys
{
    public class Hotkey(IEnumerable<ModifierKeys> modifier, IEnumerable<Keys> keys, Event action)
    {
        public IEnumerable<ModifierKeys> Modifier { set; get; } = modifier;
        public IEnumerable<Keys> Keys { set; get; } = keys;
        public Event Action { set; get; } = action;
        internal string ActionName => Action.GetDescription();
        internal string HotkeyName => Keys.AsHotkeyString(Modifier);

        public override string ToString()
        {
            return Keys.AsHotkeyString(Modifier) + ": " + Action.GetDescription();
        }

        public static Hotkey Empty => new(ArraySegment<ModifierKeys>.Empty, ArraySegment<Keys>.Empty, Event.None);
    }
}
