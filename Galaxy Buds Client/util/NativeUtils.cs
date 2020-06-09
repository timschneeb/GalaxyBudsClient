using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace Galaxy_Buds_Client.util
{
    internal static class NativeUtils
    {
        internal static uint TPM_LEFTALIGN;

        internal static uint TPM_RETURNCMD;

        static NativeUtils()
        {
            NativeUtils.TPM_LEFTALIGN = 0;
            NativeUtils.TPM_RETURNCMD = 256;
        }

        [DllImport("user32.dll", CharSet = CharSet.None, ExactSpelling = false)]
        internal static extern IntPtr PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = false, SetLastError = true)]
        internal static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll", CharSet = CharSet.None, ExactSpelling = false)]
        internal static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

        [DllImport("user32.dll", CharSet = CharSet.None, ExactSpelling = false)]
        internal static extern int TrackPopupMenuEx(IntPtr hmenu, uint fuFlags, int x, int y, IntPtr hwnd, IntPtr lptpm);
    }

    [Serializable]
    internal struct RECT
    {
        public int Left;

        public int Top;

        public int Right;

        public int Bottom;

        public int Height
        {
            get
            {
                return this.Bottom - this.Top;
            }
            set
            {
                this.Bottom = this.Top + value;
            }
        }

        public Point Position
        {
            get
            {
                return new Point((double)this.Left, (double)this.Top);
            }
        }

        public Size Size
        {
            get
            {
                return new Size((double)this.Width, (double)this.Height);
            }
        }

        public int Width
        {
            get
            {
                return this.Right - this.Left;
            }
            set
            {
                this.Right = this.Left + value;
            }
        }

        public RECT(int left, int top, int right, int bottom)
        {
            this.Left = left;
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
        }

        public RECT(Rect rect)
        {
            this.Left = (int)rect.Left;
            this.Top = (int)rect.Top;
            this.Right = (int)rect.Right;
            this.Bottom = (int)rect.Bottom;
        }

        public void Offset(int dx, int dy)
        {
            this.Left += dx;
            this.Right += dx;
            this.Top += dy;
            this.Bottom += dy;
        }

        public Int32Rect ToInt32Rect()
        {
            return new Int32Rect(this.Left, this.Top, this.Width, this.Height);
        }
    }
}
