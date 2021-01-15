using System.Collections.Generic;
using System.Text;
using Avalonia.Input;

namespace GalaxyBudsClient.Utils
{
    public static class HotkeyExtensions
    {
        public static string AsHotkeyString(this IEnumerable<Key>? keys)
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
    }
}