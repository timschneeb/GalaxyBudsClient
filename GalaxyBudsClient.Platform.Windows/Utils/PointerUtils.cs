using System;

namespace GalaxyBudsClient.Platform.Windows.Utils
{
    internal class PointerUtils
    {
        internal static IntPtr Add(IntPtr x, int y)
        {
            checked
            {
                var xi = x.ToInt64();
                xi += y;
                return new IntPtr(xi);
            }
        }
    }
}
