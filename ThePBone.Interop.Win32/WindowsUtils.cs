using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ThePBone.Interop.Win32
{
    public static class WindowsUtils
    {
        [DllImport("kernel32", SetLastError = true)]
        private static extern bool AttachConsole(int dwProcessId);

        public static void AttachConsole()
        {
            AttachConsole(-1);
        }
    }
}