using System;
using System.Threading.Tasks;
using GalaxyBudsClient.Model.Hotkeys;

// ReSharper disable MemberCanBeProtected.Global

namespace GalaxyBudsClient.Platform.Interfaces;

public class HotkeyRegisterException(string message, Hotkey hotkey) : Exception(message)
{
    public Hotkey Hotkey { set; get; } = hotkey;
    public int ResultCode { set; get; } = -1;
}

public interface IHotkeyReceiver : IDisposable
{
    Task RegisterHotkeyAsync(Hotkey hotkey);
    Task UnregisterAllAsync();
    Task ValidateHotkeyAsync(Hotkey hotkey);
}