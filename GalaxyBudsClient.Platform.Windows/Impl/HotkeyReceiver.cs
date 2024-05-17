using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Threading;
using GalaxyBudsClient.Bluetooth.Windows;
using GalaxyBudsClient.Platform.Interfaces;
using GalaxyBudsClient.Platform.Model;
using Serilog;
using Keys = GalaxyBudsClient.Platform.Model.Keys;

namespace GalaxyBudsClient.Platform.Windows.Impl;

public class HotkeyRegisterExceptionWin32 : HotkeyRegisterException
{
    public HotkeyRegisterExceptionWin32(int errorCode, IHotkey hotkey) : base($"Hotkey registration failed ({errorCode})", hotkey)
    {
        ResultCode = errorCode;
        Result = errorCode switch
        {
            1409 => HotkeyRegisterResult.Duplicated,
            _ => HotkeyRegisterResult.UnknownError
        };
    }
} 
    
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class HotkeyReceiver : IHotkeyReceiver, IDisposable
{
    public event EventHandler<IHotkey>? HotkeyPressed;
    
    private readonly IList<IHotkey> _hotkeys = new List<IHotkey>();
    
    private readonly WndProcClient _wndProc = new WndProcClient();
        
    public HotkeyReceiver()
    {
        _wndProc.MessageReceived += WndProcClient_MessageReceived;
        Log.Debug("Windows.HotkeyReceiver: WndProc probes attached");
    }
        
    public void Dispose()
    {
        Dispatcher.UIThread.Post(() =>
        {
            _wndProc.MessageReceived -= WndProcClient_MessageReceived;
            _ = UnregisterAllAsync();
        });
    }

    private void WndProcClient_MessageReceived(object? sender, WndProcClient.WindowMessage msg)
    {            
        if (msg.Msg == WndProcClient.WindowsMessage.WM_HOTKEY)
        {
            var key = (Keys)(((int)msg.lParam >> 16) & 0xFFFF);
            var modifier = (ModifierKeys)((int)msg.lParam & 0xFFFF);
            Log.Debug("Windows.HotkeyReceiver: Received event (Modifiers: {Modifier}; Key: {Key})", modifier, key);
                
            _hotkeys
                .Where(x => x.Modifier.Aggregate((prev, next) => prev | next) == modifier &&
                            x.Keys.Aggregate((prev, next) => prev | next) == key)
                .ToList()
                .ForEach(x => HotkeyPressed?.Invoke(this, x));
        }
    }

    public async Task RegisterHotkeyAsync(IHotkey hotkey)
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

        await Task.CompletedTask;
    }

    public async Task UnregisterAllAsync()
    {
        for (var i = _currentId; i > 0; i--)
        {
            UnregisterHotKey(_wndProc.WindowHandle, i);
        }
            
        _hotkeys.Clear();
            
        Log.Debug("Windows.HotkeyReceiver: All hotkeys unregistered");
         
        await Task.CompletedTask;
    }

    public async Task ValidateHotkeyAsync(IHotkey hotkey)
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

        if (!RegisterHotKey(_wndProc.WindowHandle, _currentId, (uint) modifier, (uint) key))
        {
            var code = Marshal.GetLastWin32Error();
            Log.Error("Windows.HotkeyReceiver.Register: Unable to register hotkey (Error code: {Code}) (Modifiers: {Modifier}; Key: {Key})", code, modifier, key);
            return code;
        }
            
        Log.Debug("Hotkey successfully registered (Modifiers: {Modifier}; Key: {Key})", modifier, key);
        return 0;
    }
}