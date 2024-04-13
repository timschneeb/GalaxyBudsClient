using System;
using System.Threading.Tasks;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Model.Hotkeys;
using GalaxyBudsClient.Platform.Interfaces;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.Extensions;

namespace GalaxyBudsClient.Platform;

public class HotkeyReceiver : IDisposable
{
    private static readonly object Padlock = new();
    private static HotkeyReceiver? _instance;
    public static HotkeyReceiver Instance
    {
        get
        {
            lock (Padlock)
            {
                return _instance ??= new HotkeyReceiver();
            }
        }
    }

    public static void Reset()
    {
        _instance?.Dispose();
        _instance = null;
    }

    private readonly IHotkeyReceiver _backend;

    private HotkeyReceiver()
    {
        if (PlatformUtils.IsWindows)
        {
            _backend = new Windows.HotkeyReceiver();
        }
        else if (PlatformUtils.IsLinux)
        {
            _backend = new Linux.HotkeyReceiver();
        }    
        else if (PlatformUtils.IsOSX)
        {
            _backend = new OSX.HotkeyReceiver();
        }
        else
        {
            _backend = new Dummy.HotkeyReceiver();
        }
    }

    public async Task<HotkeyRegisterException?> UpdateVerifySingleAsync(Hotkey target)
    {
        HotkeyRegisterException? targetResult = null;
        await UnregisterAll();
        foreach (var hotkey in LegacySettings.Instance.Hotkeys)
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
        await UnregisterAll();
        foreach (var hotkey in LegacySettings.Instance?.Hotkeys ?? Array.Empty<Hotkey>())
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

            await new MessageBox
            {
                Title = Strings.HotkeyAddError,
                Description = $"{ex.Message} {Strings.HotkeyAddErrorContext} {ex.Hotkey.Keys.AsHotkeyString(ex.Hotkey.Modifier)}"
            }.ShowAsync();

            return ex;
        }
    }

    public async Task UnregisterAll()
    {
        await _backend.UnregisterAllAsync();
    }
        
    public void Dispose()
    {
        _backend.Dispose();
    }
}