using System;
using Avalonia;
using Avalonia.Platform;
using Avalonia.Threading;
using ThePBone.Interop.Win32;

namespace GalaxyBudsClient.Platform.Windows
{
    public class Win32ProcWindowImpl : Avalonia.Win32.WindowImpl
    {
        public event EventHandler<WndProcClient.WindowMessage>? MessageReceived;

        public Dispatcher? Dispatcher;
        
        protected override IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            Dispatcher ??= new Dispatcher(AvaloniaLocator.Current.GetService<IPlatformThreadingInterface>());
            
            MessageReceived?.Invoke(this,
                new WndProcClient.WindowMessage()
                  {
                      hWnd = hWnd,
                      Msg = (WndProcClient.WindowsMessage) msg,
                      wParam = wParam,
                      lParam = lParam
                  });
            
            return base.WndProc(hWnd, msg, wParam, lParam);
        }
    }
}