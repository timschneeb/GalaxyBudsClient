using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GalaxyBudsClient.Platform.Interfaces;
using GalaxyBudsClient.Platform.Model;
using Serilog;

namespace GalaxyBudsClient.Platform.OSX;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class HotkeyReceiver : IHotkeyReceiver
{
    public event EventHandler<IHotkey>? HotkeyPressed;
    
    // Prevent garbage collection
    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")] 
    private readonly AppUtils.HotkeyOnDispatch _callback;
    private unsafe void* _hotkeyMgrObjc = null;
    private readonly IList<IHotkey> _hotkeys = new List<IHotkey>();
    private static readonly SemaphoreSlim Semaphore = new(1, 1);

    public HotkeyReceiver()
    {
        _callback = OnDispatchHotkey;
        unsafe
        {
            fixed (void** ptr = &_hotkeyMgrObjc)
            {
                AppUtils.allocHotkeyMgr(ptr, _callback);
            }

            if (_hotkeyMgrObjc == null)
            {
                Log.Error("OSX.HotkeyReceiver: failed to allocate hotkey manager");
            }
        }
    }

    private void OnDispatchHotkey(uint identifier)
    {
        Task.Run(async () => {
            if (!await Semaphore.WaitAsync(5000))
            {
                Log.Error("OSX.HotkeyReceiver: Blocked while trying to process hotkey");
                return;
            }

            try
            {
                IHotkey key;
                try
                {
                    key = _hotkeys[(int)identifier - 1];
                }
                finally
                {
                    Semaphore.Release();
                }

                HotkeyPressed?.Invoke(this, key);
            }
            catch(Exception ex)
            {
                Log.Error(ex, "OSX.HotkeyReceiver: Exception while processing hotkey {Id} / {Count}",
                    (int)identifier - 1, _hotkeys.Count);
            }
        });
    }
        
    public async Task RegisterHotkeyAsync(IHotkey hotkey)
    {
        ModifierKeys modFlags = 0;
        if (hotkey.Modifier.ToList().Count > 0)
        {
            modFlags = hotkey.Modifier.Aggregate((prev, next) => prev | next);
        }
        
        if (hotkey.Keys.ToList().Count != 1)
        {
            throw new HotkeyRegisterException($"More than one key isn't supported, sorry", hotkey);
        }
        var keyFlags = hotkey.Keys.ToList()[0];
        await Task.Run(async () =>
        {
            if (!await Semaphore.WaitAsync(5000))
            {
                throw new HotkeyRegisterException("Hotkey processing blocked, this shouldn't happen", hotkey);
            }
            Log.Debug("OSX.HotkeyReceiver: Registering hotkey...");
            bool result;
            unsafe
            {
                result = AppUtils.registerHotKey(_hotkeyMgrObjc, (uint)keyFlags, (uint)modFlags);
            }
            if (result)
            {
                _hotkeys.Add(hotkey);
            }

            Log.Debug("OSX.HotkeyReceiver: Registered hotkey");

            Semaphore.Release();

            if (!result)
            {
                throw new HotkeyRegisterException($"Failed to register hotkey", hotkey);
            }
        });

    }
        
    public async Task ValidateHotkeyAsync(IHotkey hotkey)
    {
        Log.Debug("OSX.HotkeyReceiver: Validating hotkey...");
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
        foreach (var b in backup)
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

        Log.Debug("OSX.HotkeyReceiver: Done validating hotkey");

    }

    public async Task UnregisterAllAsync()
    {
        await Task.Run(async () => {
            Log.Debug("OSX.HotkeyReceiver: Unregistering hotkeys...");
            if (!await Semaphore.WaitAsync(5000))
            {
                Log.Error("OSX.HotkeyReceiver: Blocked while trying to unregister hotkeys");
                return;
            }
            unsafe
            {
                AppUtils.unregisterAllHotkeys(_hotkeyMgrObjc);
            }

            // Unregister all the registered hotkeys.
            _hotkeys.Clear();
        
            Log.Debug("OSX.HotkeyReceiver: All hotkeys unregistered");

            Semaphore.Release();
        });
    }
    
    public async void Dispose()
    {
        await UnregisterAllAsync();
        unsafe
        {
            if (_hotkeyMgrObjc != null)
            {
                AppUtils.deallocHotkeyMgr(_hotkeyMgrObjc);
                _hotkeyMgrObjc = null;
            }
        }

        GC.SuppressFinalize(this);
    }
}