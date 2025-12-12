using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GalaxyBudsClient.Platform.Interfaces;
using GalaxyBudsClient.Platform.Model;
using Serilog;
using SharpHook;
using SharpHook.Data;

namespace GalaxyBudsClient.Platform.Linux;

public class HotkeyReceiver : IHotkeyReceiver
{
    public event EventHandler<IHotkey>? HotkeyPressed;
    
    private readonly List<IHotkey> _hotkeys = [];
    private readonly Dictionary<ushort, KeyCode> _pressedKeys = new();
        
    private readonly Task? _hookTask;
    private readonly TaskPoolGlobalHook _hook;
        
    public HotkeyReceiver()
    {
        _hook = new TaskPoolGlobalHook(2, GlobalHookType.All, null, true);
        _hook.KeyPressed += OnKeyPressed;
        _hook.KeyReleased += OnKeyReleased;
            
        try
        {
            _hookTask = _hook.RunAsync();
        }
        catch (Exception ex)
        {
            _hookTask = null;
            Log.Error(ex, "Linux.HotkeyReceiver: Failed to start hook");
        }
    }

    private void OnKeyReleased(object? sender, KeyboardHookEventArgs e)
    {
        lock (_pressedKeys)
        {
            _pressedKeys.Remove(e.Data.RawCode);
        }
    }

    private void OnKeyPressed(object? sender, KeyboardHookEventArgs e)
    {
        lock (_pressedKeys)
        {
            _pressedKeys.TryAdd(e.Data.RawCode, e.Data.KeyCode);
            OnKeyComboPressed(_pressedKeys.Values);
        }
    }
        
    private void OnKeyComboPressed(IEnumerable<KeyCode> keyCodes)
    {
        List<ModifierKeys> modifierKeys = [];
        List<Keys> keys = [];
        foreach (var keyCode in keyCodes)
        {
            var key = keyCode.ToKeys();
            switch (key)
            {
                case null:
                    continue;
                case Keys.LShiftKey or Keys.RShiftKey:
                    modifierKeys.Add(ModifierKeys.Shift);
                    break;
                case Keys.LControlKey or Keys.RControlKey:
                    modifierKeys.Add(ModifierKeys.Control);
                    break;
                case Keys.LMenu or Keys.RMenu:
                    modifierKeys.Add(ModifierKeys.Alt);
                    break;
                case Keys.LWin or Keys.RWin:
                    modifierKeys.Add(ModifierKeys.Win);
                    break;
                default:
                    keys.Add(key.Value);
                    break;
            }
        }
            
        foreach (var hotkey in _hotkeys.Where(hotkey => hotkey.Keys.SequenceEqual(keys) && 
                                                        hotkey.Modifier.SequenceEqual(modifierKeys)))
        {
            HotkeyPressed?.Invoke(this, hotkey);
        }
    }
        
    public async Task RegisterHotkeyAsync(IHotkey hotkey)
    {
        _hotkeys.Add(hotkey);
        Log.Debug("Hotkey successfully registered ({Name})", hotkey);
        await Task.CompletedTask;
    }
        
    public async Task ValidateHotkeyAsync(IHotkey hotkey)
    {
        await Task.CompletedTask;
    }

    public async Task UnregisterAllAsync()
    {
        _hotkeys.Clear();
        Log.Debug("Linux.HotkeyReceiver: All hotkeys unregistered");
        await Task.CompletedTask;
    }

    public async void Dispose()
    {
        await UnregisterAllAsync();
        _hook.Dispose();
        GC.SuppressFinalize(this);
    }
}
