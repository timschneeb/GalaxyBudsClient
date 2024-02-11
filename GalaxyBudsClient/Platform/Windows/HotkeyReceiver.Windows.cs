using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Threading;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Hotkeys;
using GalaxyBudsClient.Platform.Interfaces;
using GalaxyBudsClient.Utils.DynamicLocalization;
using Serilog;


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
       
#if Windows
        private readonly ThePBone.Interop.Win32.WndProcClient _wndProc = new ThePBone.Interop.Win32.WndProcClient();
#endif
        
        public HotkeyReceiver()
        {
            if (!MainWindow.IsReady())
            {
                Log.Error("Windows.HotkeyReceiver: MainWindow not ready. Cannot access WndProc callback");
                return;
            }
#if Windows
            _wndProc.MessageReceived += WndProcClient_MessageReceived;
            Log.Debug("Windows.HotkeyReceiver: WndProc probes attached");
            return;
#endif
            Log.Error("Windows.HotkeyReceiver: This platform configuration is not supported");
        }
        
        public void Dispose()
        {
            Dispatcher.UIThread.Post(() =>
            {
#if Windows                
                _wndProc.MessageReceived -= WndProcClient_MessageReceived;
#endif              
                var _ = UnregisterAllAsync();
            });
        }

#if Windows
        private void WndProcClient_MessageReceived(object? sender, ThePBone.Interop.Win32.WndProcClient.WindowMessage msg)
        {            
            if (msg.Msg == ThePBone.Interop.Win32.WndProcClient.WindowsMessage.WM_HOTKEY)
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
#endif

        public async Task RegisterHotkeyAsync(Hotkey hotkey)
        {
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
            
            var result = RegisterHotKey(modFlags, keyFlags);
            if (result != 0)
            {
                throw new HotkeyRegisterExceptionWin32(result, hotkey);
            }
        }

        public async Task UnregisterAllAsync()
        {

            for (var i = _currentId; i > 0; i--)
            {
#if Windows
                UnregisterHotKey(_wndProc.WindowHandle, i);
#endif
            }
            _hotkeys.Clear();
            
            Log.Debug("Windows.HotkeyReceiver: All hotkeys unregistered");
         
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
#if Windows
            _currentId += 1;

            if (!RegisterHotKey(_wndProc.WindowHandle, _currentId, (uint) modifier, (uint) key))
            {
                var code = Marshal.GetLastWin32Error();
                Log.Error($"Windows.HotkeyReceiver.Register: Unable to register hotkey (Error code: {code}) (Modifiers: {modifier}; Key: {key})");
                return code;
            }
            
            Log.Debug($"Hotkey successfully registered (Modifiers: {modifier}; Key: {key})");
#endif
            return 0;
        }
    }
}