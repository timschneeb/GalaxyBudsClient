using System;
using System.IO;

namespace GalaxyBudsClient.Platform.Linux;

public class DesktopServices : BaseDesktopServices
{
    private static string AutostartFile => $"{Environment.GetEnvironmentVariable("HOME")}/.config/autostart/galaxybudsclient.desktop"; 
        
    /* Self -> Disadvantage: includes arguments */
    private static string Self => File.ReadAllText("/proc/self/cmdline").Replace('\0', ' '); 
    /* SelfAlt -> Disadvantage: only includes executable path -> problematic when called via /usr/bin/dotnet */
    // private static string SelfAlt => Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty; 
        
    public override bool IsAutoStartEnabled
    {
        get => File.Exists(AutostartFile);
        set
        {
            var directory = Path.GetDirectoryName(AutostartFile);
            if (directory != null && Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            if (value)
            {
                File.WriteAllText(
                    AutostartFile,
                    $"""
                     [Desktop Entry]
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
                     """
                );
            }
            else
            {
                File.Delete(AutostartFile);
            }
        }
    }
}