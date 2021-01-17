using System.Collections.Generic;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Utils;

namespace GalaxyBudsClient.Model.Hotkeys
{
    public class Hotkey
    {
        public IEnumerable<ModifierKeys> Modifier { set; get; }
        public IEnumerable<Keys> Keys { set; get; }
        public EventDispatcher.Event Action { set; get; }
        internal string ActionName => Action.GetDescription();
        internal string HotkeyName => Keys.AsHotkeyString(Modifier);

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
        
        /* Used for list separator workaround */
        internal bool IsLastInList { set; get; }
    }
}
