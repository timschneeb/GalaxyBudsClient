using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Collections;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Hotkeys;
using GalaxyBudsClient.Platform.Interfaces;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.DynamicLocalization;

namespace GalaxyBudsClient.Platform
{
    public class HotkeyReceiverImpl : IDisposable
    {
        private static readonly object Padlock = new object();
        private static HotkeyReceiverImpl? _instance;
        public static HotkeyReceiverImpl Instance
        {
            get
            {
                lock (Padlock)
                {
                    return _instance ??= new HotkeyReceiverImpl();
                }
            }
        }

        public static void Reset()
        {
            _instance?.UnregisterAll();
            _instance?.Dispose();
            _instance = null;
        }

        private readonly IHotkeyReceiver _backend;

        public HotkeyReceiverImpl()
        {
            if (PlatformUtils.IsWindows)
            {
                _backend = new Windows.HotkeyReceiver();
            }
            else
            {
                _backend = new Dummy.HotkeyReceiver();
            }
        }

        public async Task<HotkeyRegisterException?> UpdateVerifySingleAsync(Hotkey target)
        {
            HotkeyRegisterException? targetResult = null;
            UnregisterAll();
            foreach (var hotkey in SettingsProvider.Instance.Hotkeys)
            {
                var error = await RegisterAsync(hotkey, true);
                if (target == hotkey)
                {
                    targetResult = error;
                }
            }

            return targetResult;
        }
        
        public async Task<HotkeyRegisterException?> ValidateHotkeyAsync(Hotkey target)
        {
            try
            {
                await _backend.ValidateHotkeyAsync(target);
                return null;
            }
            catch (HotkeyRegisterException ex)
            {
                return ex;
            }
        }
        
        public async void Update(bool silent = false)
        {
            UnregisterAll();
            foreach (var hotkey in SettingsProvider.Instance?.Hotkeys ?? new Hotkey[0])
            {
                await RegisterAsync(hotkey, silent);
            }
        }

        public async Task<HotkeyRegisterException?> RegisterAsync(Hotkey hotkey, bool silent = false)
        {
            try
            {
                await _backend.RegisterHotkeyAsync(hotkey);
                return null;
            }
            catch (HotkeyRegisterException ex)
            {
                if (silent)
                {
                    return ex;
                }

                await new MessageBox()
                {
                    Title = Loc.Resolve("hotkey_add_error"),
                    Description = $"{ex.Message} {Loc.Resolve("hotkey_add_error_context")} {ex.Hotkey.Keys.AsHotkeyString(ex.Hotkey.Modifier)}",
                    Topmost = true
                }.ShowDialog(MainWindow.Instance);

                return ex;
            }
        }

        public async void UnregisterAll()
        {
            await _backend.UnregisterAllAsync();
        }
        
        public void Dispose()
        {
            _backend.Dispose();
        }
    }
}
