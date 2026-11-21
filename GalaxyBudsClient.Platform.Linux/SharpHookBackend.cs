using System;
using Serilog;
using SharpHook.Data;
using SharpHook.Providers;

namespace GalaxyBudsClient.Platform.Linux;

internal static class SharpHookBackend
{
    private static readonly Lazy<bool> BackendSelected = new(SelectXRecordBackend);

    /// <summary>
    /// Must be called before any other SharpHook API, because the first call loads a libuiohook backend
    /// and a loaded backend cannot be changed afterwards.
    /// </summary>
    public static void EnsureInitialized() => _ = BackendSelected.Value;

    // SharpHook 8 loads its libinput/uinput backend on Wayland sessions by default. That backend needs
    // access to /dev/input and /dev/uinput, which regular users and Flatpak sandboxes don't have.
    // Keep the XRecord/XTest backend that SharpHook used before version 8.
    private static bool SelectXRecordBackend()
    {
        var result = UioHookProvider.Instance.SetLinuxMode(LinuxMode.XRecord);
        if (result == UioHookResult.Success)
            return true;

        Log.Warning("Linux.SharpHookBackend: Failed to select the XRecord backend ({Result})", result);
        return false;
    }
}
