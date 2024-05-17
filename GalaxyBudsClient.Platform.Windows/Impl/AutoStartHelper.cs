using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security;
using GalaxyBudsClient.Platform.Interfaces;
using Microsoft.Win32;
using Serilog;

#pragma warning disable CA1416

namespace GalaxyBudsClient.Platform.Windows.Impl;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class AutoStartHelper : IAutoStartHelper
{
    private const string PathName = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string KeyName = "Galaxy Buds Client";
        
    public bool Enabled
    {
        get
        {
            try
            {
                return Registry.CurrentUser.OpenSubKey(PathName, false)?
                    .GetValue(KeyName, null) != null;
            }
            catch (SecurityException)
            {
                return false;
            }
        } 
        set
        {
            try
            {
                if (value)
                {
                    Registry.CurrentUser.OpenSubKey(PathName, true)?
                        .SetValue(KeyName, "\"" + Process.GetCurrentProcess().MainModule?.FileName + "\" /StartMinimized");
                }
                else
                {
                    Registry.CurrentUser.OpenSubKey(PathName, true)?.DeleteValue(KeyName);
                }

                return;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to set autostart");
            }
        }
    }
}