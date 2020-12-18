using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Serilog;

namespace ThePBone.Interop.Win32.Devices
{

    public class WndProcClient
    {
        public event EventHandler<WindowMessage>? MessageReceived;

        private UnmanagedMethods.WndProc? _wndProcDelegate;
        private IntPtr _hwnd;
        public IntPtr hWnd => _hwnd;

        public WndProcClient()
        {
            // Ensure that the delegate doesn't get garbage collected by storing it as a field.
            _wndProcDelegate = new UnmanagedMethods.WndProc(WndProc);

            UnmanagedMethods.WNDCLASSEX wndClassEx = new UnmanagedMethods.WNDCLASSEX
            {
                cbSize = Marshal.SizeOf<UnmanagedMethods.WNDCLASSEX>(),
                lpfnWndProc = _wndProcDelegate,
                hInstance = UnmanagedMethods.GetModuleHandle(null),
                lpszClassName = "MessageWindow " + Guid.NewGuid(),
            };

            ushort atom = UnmanagedMethods.RegisterClassEx(ref wndClassEx);

            if (atom == 0)
            {
                throw new Win32Exception();
            }

            _hwnd = UnmanagedMethods.CreateWindowEx(0, atom, null, 0, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

            if (_hwnd == IntPtr.Zero)
            {
                throw new Win32Exception();
            }
        }
        
        private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            var message = new WindowMessage()
            {
                hWnd = hWnd,
                Msg = (UnmanagedMethods.WindowsMessage) msg,
                wParam = wParam,
                lParam = lParam
            };

            MessageReceived?.Invoke(this, message);

            return UnmanagedMethods.DefWindowProc(hWnd, msg, wParam, lParam);
        }
    }
}