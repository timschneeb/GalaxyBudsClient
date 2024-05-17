using System;
using System.Threading.Tasks;
using GalaxyBudsClient.Platform.Interfaces;
using Serilog;

#pragma warning disable 1998

namespace GalaxyBudsClient.Platform.Stubs;

public class DummyHotkeyReceiver : IHotkeyReceiver
{
    public async Task RegisterHotkeyAsync(IHotkey hotkey)
    {
        Log.Warning("Dummy.HotkeyReceiver: Platform not supported");
    }
        
    public async Task UnregisterAllAsync()
    {
        Log.Warning("Dummy.HotkeyReceiver: Platform not supported");
    }

    public async Task ValidateHotkeyAsync(IHotkey hotkey)
    {
        Log.Warning("Dummy.HotkeyReceiver: Platform not supported");
    }
    
    public event EventHandler<IHotkey>? HotkeyPressed;

    public void Dispose()
    {
            
    }
}