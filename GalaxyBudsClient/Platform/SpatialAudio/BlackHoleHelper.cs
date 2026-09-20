using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Serilog;

namespace GalaxyBudsClient.Platform.SpatialAudio;

/// <summary>
/// Helper utilities for detecting, installing, and configuring BlackHole 2ch virtual audio driver on macOS.
/// </summary>
public static class BlackHoleHelper
{
    public const string DriverDirectory = "/Library/Audio/Plug-Ins/HAL/BlackHole2ch.driver";

    /// <summary>
    /// Checks if the BlackHole driver bundle is installed in macOS Audio HAL plugins directory.
    /// </summary>
    public static bool IsDriverInstalled =>
        RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && Directory.Exists(DriverDirectory);

    /// <summary>
    /// Checks if CoreAudio has actively loaded BlackHole 2ch into the system audio device list.
    /// </summary>
    public static bool IsLoadedInCoreAudio()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return false;

        try
        {
            var p = Process.Start(new ProcessStartInfo
            {
                FileName = "/usr/sbin/system_profiler",
                Arguments = "SPAudioDataType",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            if (p == null) return false;

            var output = p.StandardOutput.ReadToEnd();
            p.WaitForExit(2000);
            return output.Contains("BlackHole", StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "BlackHoleHelper: Error checking system_profiler for BlackHole");
            return false;
        }
    }

    /// <summary>
    /// Launches Homebrew in background to install blackhole-2ch, or opens the project GitHub page.
    /// </summary>
    public static async Task<bool> InstallViaHomebrewAsync()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return false;

        try
        {
            var brewPath = File.Exists("/opt/homebrew/bin/brew")
                ? "/opt/homebrew/bin/brew"
                : (File.Exists("/usr/local/bin/brew") ? "/usr/local/bin/brew" : null);

            if (brewPath == null)
            {
                // Fallback to opening the official BlackHole release site
                Process.Start(new ProcessStartInfo
                {
                    FileName = "/usr/bin/open",
                    Arguments = "https://github.com/ExistentialAudio/BlackHole",
                    UseShellExecute = true
                });
                return false;
            }

            Log.Information("BlackHoleHelper: Installing blackhole-2ch via Homebrew at {BrewPath}", brewPath);
            var p = Process.Start(new ProcessStartInfo
            {
                FileName = brewPath,
                Arguments = "install blackhole-2ch",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            if (p == null) return false;
            await p.WaitForExitAsync();
            Log.Information("BlackHoleHelper: Homebrew exited with code {Code}", p.ExitCode);
            return p.ExitCode == 0;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "BlackHoleHelper: Failed installing BlackHole");
            return false;
        }
    }

    /// <summary>
    /// Restarts coreaudiod using AppleScript administrator privileges prompt (Touch ID / Password)
    /// so the newly installed HAL driver is immediately recognized by macOS without rebooting.
    /// </summary>
    public static bool RestartCoreAudio()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return false;

        try
        {
            Log.Information("BlackHoleHelper: Requesting coreaudiod restart via AppleScript");
            var psi = new ProcessStartInfo
            {
                FileName = "/usr/bin/osascript",
                UseShellExecute = false
            };
            psi.ArgumentList.Add("-e");
            psi.ArgumentList.Add("do shell script \"killall coreaudiod\" with administrator privileges");
            var p = Process.Start(psi);
            return p != null;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "BlackHoleHelper: Failed to restart coreaudiod");
            return false;
        }
    }

    /// <summary>
    /// Opens macOS Audio MIDI Setup utility.
    /// </summary>
    public static void OpenAudioMidiSetup()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "/usr/bin/open",
                Arguments = "-a \"Audio MIDI Setup\"",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "BlackHoleHelper: Failed to open Audio MIDI Setup");
        }
    }

    /// <summary>
    /// Opens macOS Sound Settings preferences panel.
    /// </summary>
    public static void OpenSoundSettings()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "/usr/bin/open",
                Arguments = "x-apple.systempreferences:com.apple.preference.sound",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "BlackHoleHelper: Failed to open Sound Preferences");
        }
    }
}
