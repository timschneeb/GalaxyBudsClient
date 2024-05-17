using System;
using System.Threading.Tasks;

// ReSharper disable MemberCanBeProtected.Global

namespace GalaxyBudsClient.Platform.Interfaces;

public enum HotkeyRegisterResult
{
    UnknownError,
    Duplicated
}

public class HotkeyRegisterException(string message, IHotkey hotkey) : Exception(message)
{
    public IHotkey Hotkey { set; get; } = hotkey;
    public int ResultCode { set; get; } = -1;
    public HotkeyRegisterResult? Result { set; get; }
    
}

public interface IHotkeyReceiver : IDisposable
{
    Task RegisterHotkeyAsync(IHotkey hotkey);
    Task UnregisterAllAsync();
    Task ValidateHotkeyAsync(IHotkey hotkey);
    event EventHandler<IHotkey> HotkeyPressed;
}