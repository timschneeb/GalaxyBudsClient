using System;

namespace ThePBone.Interop.Win32.Devices.Utils
{
    class PointerUtils
    {
        internal static IntPtr Add(IntPtr x, int y)
        {
            checked
            {
                var xi = x.ToInt64();
                xi += y;
                IntPtr p = new IntPtr(xi);
                return p;
            }
        }
    }
}
