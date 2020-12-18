using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Serilog;

namespace ThePBone.Interop.Win32
{

    public class WndProcClient
    {
        public event EventHandler<WindowMessage>? MessageReceived;

        private readonly Unmanaged.WndProc? _wndProcDelegate;
        private readonly IntPtr _hwnd;
        public IntPtr WindowHandle => _hwnd;

        public WndProcClient()
        {
            // Ensure that the delegate doesn't get garbage collected by storing it as a field.
            _wndProcDelegate = new Unmanaged.WndProc(WndProc);

            Unmanaged.WNDCLASSEX wndClassEx = new Unmanaged.WNDCLASSEX
            {
                cbSize = Marshal.SizeOf<Unmanaged.WNDCLASSEX>(),
                lpfnWndProc = _wndProcDelegate,
                hInstance = Unmanaged.GetModuleHandle(null),
                lpszClassName = "MessageWindow " + Guid.NewGuid(),
            };

            ushort atom = Unmanaged.RegisterClassEx(ref wndClassEx);

            if (atom == 0)
            {
                Log.Error("Interop.Win32.WndProcClient: atom is null");
                throw new Win32Exception();
            }

            _hwnd = Unmanaged.CreateWindowEx(0, atom, null, 0, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

            if (_hwnd == IntPtr.Zero)
            {
                Log.Error("Interop.Win32.WndProcClient: nWnd is null");
                throw new Win32Exception();
            }
        }
        
        private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            var message = new WindowMessage()
            {
                hWnd = hWnd,
                Msg = (Unmanaged.WindowsMessage) msg,
                wParam = wParam,
                lParam = lParam
            };

            MessageReceived?.Invoke(this, message);

            return Unmanaged.DefWindowProc(hWnd, msg, wParam, lParam);
        }
    }
}