using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Model.Hotkeys;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.DynamicLocalization;
using Serilog;
using Key = Avalonia.Input.Key;

namespace GalaxyBudsClient.Model
{
    public class Hotkey
    {
        public IEnumerable<ModifierKeys> Modifier;
        public IEnumerable<Keys> Keys;
        public EventDispatcher.Event Action;

        public Hotkey(IEnumerable<ModifierKeys> modifier, IEnumerable<Keys> keys, EventDispatcher.Event action)
        {
            Action = action;
            Modifier = modifier;
            Keys = keys;
        }
        
        public override string ToString()
        {
            return Keys.AsHotkeyString(Modifier) + ": " + Action.GetDescription();
        }
    }
}
