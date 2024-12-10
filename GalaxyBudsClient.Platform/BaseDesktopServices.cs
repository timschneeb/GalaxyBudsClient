using System;
using System.Diagnostics;
using GalaxyBudsClient.Platform.Interfaces;
using Serilog;

namespace GalaxyBudsClient.Platform;

public abstract class BaseDesktopServices : IDesktopServices
{
    public abstract bool IsAutoStartEnabled { get; set; }

    public virtual void OpenUri(string uri)
    {
        // Default implementation
        try
        {
            Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true, Verb = "open" });
        }
        catch(Exception ex)
        {
            Log.Error(ex, "Cannot open link {Uri}", uri);
        }
    }
}