using Avalonia.Controls;
using FluentIcons.Common;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Model.Config;
using Serilog;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class DevicesPageViewModel : MainPageViewModelBase
{
    public override Control CreateView() => new DevicesPage();
    public override string TitleKey => Keys.DevicesHeader;
    public override Symbol IconKey => Symbol.BluetoothConnected;
    public override bool ShowsInFooter => true;
    
    public async void DoNewCommand()
    {
        var result = await DeviceSelectionDialog.OpenDialogAsync();
        /* TODO
         
         if (result is null) 
            return;
        
        Hotkeys.Add(result);
        SaveChanges();*/
    }
    
    public async void DoConnectCommand(object? param)
    {
        if (param is not Device device)
            return;
        
        var index = Settings.Data.Devices.IndexOf(device);
        if (index < 0)
        {
            Log.Debug("DevicesPage.Connect: Cannot find device in configuration");
            return;
        }

        // TODO
    }
    
    public void DoDeleteCommand(object? param)
    {
        if (param is not Device device)
            return;

        Settings.Data.Devices.Remove(device);
    }
}