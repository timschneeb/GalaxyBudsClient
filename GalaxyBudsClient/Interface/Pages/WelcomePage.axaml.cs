using System;
using System.Diagnostics;
using System.Threading;
using Avalonia.Interactivity;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.ViewModels.Pages;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;
using Serilog;

namespace GalaxyBudsClient.Interface.Pages;

public partial class WelcomePage : BasePage<WelcomePageViewModel>
{
    private bool _isOfficialAppInstalled;
    
    public WelcomePage()
    {
        InitializeComponent();
        Register.Focus();
        
        CheckIfOfficialAppIsInstalled();
    }
    
    public async void OnRegisterClicked(object? sender, RoutedEventArgs e)
    {
        if (_isOfficialAppInstalled)
        {
            await new MessageBox
            {
                Title = Loc.Resolve("budsapp_header"), 
                Description = Loc.Resolve("budsapp_text_p1").Trim() + "\n" + 
                              Loc.Resolve("budsapp_text_p2").Trim() + "\n" + 
                              Loc.Resolve("budsapp_text_p3").Trim(),
                ButtonText = Loc.Resolve("continue_button"),
            }.ShowAsync();
        }
        
        await DeviceSelectionDialog.OpenDialogAsync();
    }

    private void CheckIfOfficialAppIsInstalled()
    {
        // Only search for the Buds app on Windows 10 and above
        if (!PlatformUtils.IsWindows || Environment.OSVersion.Version.Major < 10) 
            return;
        
        ThreadPool.QueueUserWorkItem(delegate 
        {
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
                if(process?.WaitForExit(4000) ?? false) 
                {
                    _isOfficialAppInstalled = process.StandardOutput.ReadToEnd().Contains("SAMSUNGELECTRONICSCO.LTD.GalaxyBuds");
                } 
            }
            catch(Exception exception)
            {
                Log.Warning(exception, "WelcomePage.BudsAppDetected");
            }
        });
    }
}
