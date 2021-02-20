using System;

namespace ThePBone.Interop.Win32
{
     partial class WndProcClient
    {
        public class WindowMessage
        {
            public IntPtr hWnd { set; get; }
            public WindowsMessage Msg { set; get; }
            public IntPtr wParam { set; get; }
            public IntPtr lParam { set; get; }
    
            public override string ToString()
            {
                return $"WindowMessage[hWnd=0x{hWnd.ToInt64():x8}," +
                       $"Msg={Msg},wParam=0x{wParam.ToInt64():x8},lParam=0x{lParam.ToInt64():x8}]";
            }
        }
    }

}
