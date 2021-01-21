using System;
using System.Threading.Tasks;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Hotkeys;

namespace GalaxyBudsClient.Platform.Interfaces
{
    public class HotkeyRegisterException : Exception
    {
        public Hotkey Hotkey { set; get; }
        public int ResultCode { set; get; } = -1;
        public HotkeyRegisterException(string message, Hotkey hotkey) : base(message)
        {
            Hotkey = hotkey;
        }
    }

    public interface IHotkeyReceiver : IDisposable
    {
        Task RegisterHotkeyAsync(Hotkey hotkey);
        Task UnregisterAllAsync();
        Task ValidateHotkeyAsync(Hotkey hotkey);
    }
}