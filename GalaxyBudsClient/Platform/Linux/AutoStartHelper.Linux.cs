using System;
using System.Diagnostics;
using System.IO;
using GalaxyBudsClient.Platform.Interfaces;

namespace GalaxyBudsClient.Platform.Linux;

public class AutoStartHelper : IAutoStartHelper
{
    private static string AutostartFile => $"{Environment.GetEnvironmentVariable("HOME")}/.config/autostart/galaxybudsclient.desktop"; 
        
    /* Self -> Disadvantage: includes arguments */
    private static string Self => File.ReadAllText("/proc/self/cmdline").Replace('\0', ' '); 
    /* SelfAlt -> Disadvantage: only includes executable path -> problematic when called via /usr/bin/dotnet */
    private static string SelfAlt => Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty; 
        
    public bool Enabled
    {
        get => File.Exists(AutostartFile);
        set
        {
            if (value)
            {
                File.WriteAllText(
                    AutostartFile,
                    $@"[Desktop Entry]
Exec={Self} /StartMinimized
Name=GalaxyBudsClient
StartupNotify=false
Terminal=false
Type=Application
X-GNOME-Autostart-Delay=10
X-GNOME-Autostart-enabled=true
X-KDE-autostart-after=panel
X-KDE-autostart-phase=2
X-MATE-Autostart-Delay=10
"
                );
            }
            else
            {
                File.Delete(AutostartFile);
            }
        }
    }
}