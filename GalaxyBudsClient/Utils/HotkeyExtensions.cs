using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia.Input;
using GalaxyBudsClient.Model.Hotkeys;

namespace GalaxyBudsClient.Utils
{
    public static class HotkeyExtensions
    {
        public static string AsAvaloniaHotkeyString(this IEnumerable<Key>? keys)
        {
            var first = true;
            StringBuilder sb = new StringBuilder();

            if (keys == null)
                return "null";

            foreach (var key in keys)
            {
                if (!first)
                {
                    sb.Append("+");
                }
                sb.Append(key);

                first = false;
            }

            return sb.ToString();
        }
        
        public static string AsHotkeyString(this IEnumerable<Keys>? keys, IEnumerable<ModifierKeys>? modifiers)
        {
            var first = true;
            StringBuilder sb = new StringBuilder();

            if (keys == null && modifiers == null)
            {
                return "null";
            }
            
            foreach (var modifier in modifiers ?? new ModifierKeys[0])
            {
                if (!first)
                {
                    sb.Append("+");
                }
                sb.Append(modifier);

                first = false;
            }
            
            foreach (var key in keys ?? new Keys[0])
            {
                if (!first)
                {
                    sb.Append("+");
                }
                sb.Append(key);

                first = false;
            }

            return sb.ToString();
        }
    }
}