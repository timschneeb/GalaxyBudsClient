using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
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

        public void ProcessMessage()
        {

            if (Unmanaged.GetMessage(out var msg, IntPtr.Zero, 0, 0) > -1)
            {
                Unmanaged.TranslateMessage(ref msg);
                Unmanaged.DispatchMessage(ref msg);
            }
            else
            {
                Log.Error("WndProcClient: Unmanaged error in {0}. Error Code: {1}", nameof(ProcessMessage),
                    Marshal.GetLastWin32Error());
            }
        }

        public void RunLoop(CancellationToken cancellationToken)
        {
            var result = 0;
            while (!cancellationToken.IsCancellationRequested
                   && (result = Unmanaged.GetMessage(out var msg, IntPtr.Zero, 0, 0)) > 0)
            {
                Unmanaged.TranslateMessage(ref msg);
                Unmanaged.DispatchMessage(ref msg);
            }
            if (result < 0)
            {
                Log.Error("WndProcClient: Unmanaged error in {0}. Error Code: {1}", nameof(RunLoop),
                    Marshal.GetLastWin32Error());
            }
        }
    }
}