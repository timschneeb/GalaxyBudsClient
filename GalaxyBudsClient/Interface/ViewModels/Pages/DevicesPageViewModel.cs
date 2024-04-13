using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using FluentIcons.Common;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Utils;
using Serilog;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class DevicesPageViewModel : MainPageViewModelBase
{
    public override Control CreateView() => new DevicesPage();
    public override string TitleKey => Keys.DevicesHeader;
    public override Symbol IconKey => Symbol.BluetoothConnected;
    public override bool ShowsInFooter => true;
    // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
    public ObservableCollection<IDevice> Devices { get; } = new(LegacySettings.Instance.Devices ?? Array.Empty<IDevice>());

    public async void DoNewCommand()
    {
        var result = await DeviceSelectionDialog.OpenDialogAsync();
        /*if (result is null) 
            return;
        
        Hotkeys.Add(result);
        SaveChanges();*/
    }
    
    public async void DoConnectCommand(object? param)
    {
        if (param is not IDevice device)
            return;
        
        var index = Devices.IndexOf(device);
        if (index < 0)
        {
            Log.Debug("DevicesPage.Connect: Cannot find device in configuration");
            return;
        }

        
    }
    
    public void DoDeleteCommand(object? param)
    {
        if (param is not IDevice device)
            return;

        Devices.Remove(device);
        SaveChanges();
    }

    private void SaveChanges()
    {
        LegacySettings.Instance.Devices = Devices.ToArray();
    }
    
}