using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using GalaxyBudsClient.Platform.Interfaces;
using Microsoft.Win32;
using Serilog;

#pragma warning disable CA1416

namespace GalaxyBudsClient.Platform.Windows.Impl;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class OfficialAppDetector : IOfficialAppDetector
{
    public async Task<bool> IsInstalledAsync()
    {
        // Only search for the Buds app on Windows 10 and above
        if (Environment.OSVersion.Version.Major < 10) 
            return false;
        
        try
        {
            var process = Process.Start(
                new ProcessStartInfo {
                    FileName = "powershell",
                    Arguments = "Get-AppxPackage SAMSUNGELECTRONICSCO.LTD.GalaxyBuds",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

            if (process == null)
                return false;
                
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(4000);
            
            await process.WaitForExitAsync(cancellationTokenSource.Token);
            return (await process.StandardOutput.ReadToEndAsync(cancellationTokenSource.Token))
                .Contains("SAMSUNGELECTRONICSCO.LTD.GalaxyBuds");
        }
        catch(Exception exception)
        {
            Log.Warning(exception, "Windows.OfficialAppDetector.IsInstalledAsync");
        }

        return false;
    }
}