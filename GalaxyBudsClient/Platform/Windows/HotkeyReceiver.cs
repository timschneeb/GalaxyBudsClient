using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Threading;
using csscript;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Hotkeys;
using GalaxyBudsClient.Platform.Interfaces;
using GalaxyBudsClient.Utils.DynamicLocalization;
using JetBrains.Annotations;
using Serilog;
using ThePBone.Interop.Win32;

namespace GalaxyBudsClient.Platform.Windows
{
    public class HotkeyRegisterExceptionWin32 : HotkeyRegisterException
    {
        public HotkeyRegisterExceptionWin32(int errorCode, Hotkey hotkey) : base(Resolve(errorCode), hotkey)
        {
            ResultCode = errorCode;
        }

        private static string Resolve(int code)
        {
            switch (code)
            {
                case 1409:
                    return Loc.Resolve("hotkey_add_error_duplicate");
            }

            return string.Format(Loc.Resolve("hotkey_add_error_unknown"), code);
        }
    } 
    
    public class HotkeyReceiver : IHotkeyReceiver, IDisposable
    {
        private readonly IList<Hotkey> _hotkeys = new List<Hotkey>();
        
        public HotkeyReceiver()
        {
            if (!MainWindow.IsReady())
            {
                Log.Error("Windows.HotkeyReceiver: MainWindow not ready. Cannot access WndProc callback.");
                return;
            }
            
            if (MainWindow.Instance.PlatformImpl is Win32ProcWindowImpl impl)
            {
                Log.Debug($"Windows.HotkeyReceiver: WndProc probes attached");
                impl.MessageReceived += OnMessageReceived;
            }
            else
            {
                Log.Error("Windows.HotkeyReceiver: This platform configuration is not supported.");
            }
        }
        
        public void Dispose()
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (!MainWindow.IsReady())
                {
                    return;
                }
                
                if (MainWindow.Instance.PlatformImpl is Win32ProcWindowImpl impl)
                {
                    impl.MessageReceived -= OnMessageReceived;
                }   
                
                var _ = UnregisterAllAsync();
            });
        }

        private void OnMessageReceived(object? sender, WndProcClient.WindowMessage msg)
        {            
            if (msg.Msg == WndProcClient.WindowsMessage.WM_HOTKEY)
            {
                var key = (Keys)(((int)msg.lParam >> 16) & 0xFFFF);
                var modifier = (ModifierKeys)((int)msg.lParam & 0xFFFF);
                Log.Debug($"Windows.HotkeyReceiver: Received event (Modifiers: {modifier}; Key: {key})");
                
                _hotkeys
                    .Where(x => x.Modifier.Aggregate((prev, next) => prev | next) == modifier &&
                                x.Keys.Aggregate((prev, next) => prev | next) == key)
                    .ToList()
                    .ForEach(x => EventDispatcher.Instance.Dispatch(x.Action));
            }
        }

        public async Task RegisterHotkeyAsync(Hotkey hotkey)
        {
            if (MainWindow.Instance.PlatformImpl is Win32ProcWindowImpl impl)
            {
                if (impl.Dispatcher == null)
                {
                    Log.Error(
                        "Windows.HotkeyReceiver.Register: Abnormal state: Win32Proc dispatcher not yet initialized");
                    throw new HotkeyRegisterException(
                        "Windows hotkey registry not yet initialized. Please try again later.", hotkey);
                }
                
                ModifierKeys modFlags = 0;
                if (hotkey.Modifier.ToList().Count > 0)
                {
                    modFlags = hotkey.Modifier.Aggregate((prev, next) => prev | next);
                }

                Keys keyFlags = 0;
                if (hotkey.Keys.ToList().Count > 0)
                {
                    keyFlags = hotkey.Keys.Aggregate((prev, next) => prev | next);
                }

                _hotkeys.Add(hotkey);
                int Register() => RegisterHotKey(modFlags, keyFlags);
                var result = await impl.Dispatcher.InvokeAsync(Register);
                if (result != 0)
                {
                    throw new HotkeyRegisterExceptionWin32(result, hotkey);
                }
            }
            else
            {
                throw new HotkeyRegisterException(
                    "Your platform configuration is not supported. WndProc provider unavailable.", hotkey);
            }
        }

        public async Task UnregisterAllAsync()
        {
            if (MainWindow.Instance.PlatformImpl is Win32ProcWindowImpl impl)
            {
                // Unregister all the registered hotkeys.
                impl.Dispatcher?.Post(() =>
                {
                    for (var i = _currentId; i > 0; i--)
                    {
                        UnregisterHotKey(impl.Handle.Handle, i);
                    }
                });
                _hotkeys.Clear();
                
                Log.Debug("Windows.HotkeyReceiver: All hotkeys unregistered");
            }
            
            await Task.CompletedTask;
        }

        public async Task ValidateHotkeyAsync(Hotkey hotkey)
        {
            Log.Debug("Windows.HotkeyReceiver: Validating hotkey...");
            HotkeyRegisterException? error = null;
            var backup = _hotkeys.ToList();
            await UnregisterAllAsync();
            try
            {
                await RegisterHotkeyAsync(hotkey);
            }
            catch (HotkeyRegisterException ex)
            {
                error = ex;
            }
                
            await UnregisterAllAsync();
            foreach (var b in backup.Where(b => b != null))
            {
                try
                {
                    await RegisterHotkeyAsync(b);
                }
                catch(HotkeyRegisterException){}
            }

            if (error != null)
            {
                throw error;
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    
        private int _currentId;
    
        /// <summary>
        /// Registers a hot key in the system.
        /// </summary>
        /// <param name="modifier">The modifiers that are associated with the hot key.</param>
        /// <param name="key">The key itself that is associated with the hot key.</param>
        private int RegisterHotKey(ModifierKeys modifier, Keys key)
        {
            _currentId += 1;
        
            if (!RegisterHotKey(MainWindow.Instance.PlatformImpl.Handle.Handle, _currentId, (uint) modifier, (uint) key))
            {
                var code = Marshal.GetLastWin32Error();
                Log.Error($"Windows.HotkeyReceiver.Register: Unable to register hotkey (Error code: {code}) (Modifiers: {modifier}; Key: {key})");
                return code;
            }
            
            Log.Debug($"Hotkey successfully registered (Modifiers: {modifier}; Key: {key})");
            return 0;
        }
    }
}