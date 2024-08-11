using System.Threading.Tasks;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Platform.Interfaces;
using GalaxyBudsClient.Utils.Extensions;
using Serilog;

namespace GalaxyBudsClient.Platform;

public class HotkeyReceiverManager
{
    public static void Reset()
    {
        _instance = null;
    }

    private readonly IHotkeyReceiver _backend = PlatformImpl.HotkeyReceiver;

    public HotkeyReceiverManager()
    {
        _backend.HotkeyPressed += (_, hotkey) =>
        {
            if (hotkey is Hotkey h)
                EventDispatcher.Instance.Dispatch(h.Action);
            else
                Log.Error("HotkeyReceiverManager: Received invalid hotkey object {Obj}", hotkey);
        };
    }
    

    public async Task<HotkeyRegisterException?> UpdateVerifySingleAsync(Hotkey target)
    {
        HotkeyRegisterException? targetResult = null;
        await UnregisterAll();
        foreach (var hotkey in Settings.Data.Hotkeys)
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
        foreach (var hotkey in Settings.Data.Hotkeys)
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

            var message = ex.Result switch
            {
                HotkeyRegisterResult.Duplicated => Strings.HotkeyAddErrorDuplicate,
                HotkeyRegisterResult.UnknownError => string.Format(Strings.HotkeyAddErrorUnknown, ex.ResultCode),
                _ => ex.Message
            };
            
            await new MessageBox
            {
                Title = Strings.HotkeyAddError,
                Description = $"{message} {Strings.HotkeyAddErrorContext} {ex.Hotkey.Keys.AsHotkeyString(ex.Hotkey.Modifier)}"
            }.ShowAsync();

            return ex;
        }
    }

    public async Task UnregisterAll()
    {
        await _backend.UnregisterAllAsync();
    }
    
#region Singleton
    private static readonly object Padlock = new();
    private static HotkeyReceiverManager? _instance;
    public static HotkeyReceiverManager Instance
    {
        get
        {
            lock (Padlock)
            {
                return _instance ??= new HotkeyReceiverManager();
            }
        }
    }
#endregion
}