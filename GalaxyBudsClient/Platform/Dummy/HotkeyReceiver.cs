using System.Threading.Tasks;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Hotkeys;
using GalaxyBudsClient.Platform.Interfaces;
using Serilog;

#pragma warning disable 1998

namespace GalaxyBudsClient.Platform.Dummy
{
    public class HotkeyReceiver : IHotkeyReceiver
    {
        public async Task RegisterHotkeyAsync(Hotkey hotkey)
        {
            Log.Warning("Dummy.HotkeyReceiver: Platform not supported");
        }
        
        public async Task UnregisterAllAsync()
        {
            Log.Warning("Dummy.HotkeyReceiver: Platform not supported");
        }

        public async Task ValidateHotkeyAsync(Hotkey hotkey)
        {
            Log.Warning("Dummy.HotkeyReceiver: Platform not supported");
        }

        public void Dispose()
        {
            
        }
    }
}