using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Bluetooth;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.Interface;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;
using ReactiveUI.Fody.Helpers;

namespace GalaxyBudsClient.Interface.ViewModels.Dialogs;

public class DeviceSelectionDialogViewModel : ViewModelBase
{
    public ObservableCollection<BluetoothDevice> Devices { init; get; } = [];
    public bool CanUseAlternativeBackend => PlatformUtils.IsWindowsContractsSdkSupported;

    [Reactive] public BluetoothDevice? SelectedDevice { set; get; }
    [Reactive] public Models SelectedModel { set; get; }
    [Reactive] public bool NoDevices { set; get; }

    private readonly ContentDialog _dialog;

    public DeviceSelectionDialogViewModel(ContentDialog dialog)
    {
        _dialog = dialog;
        Devices.CollectionChanged += (_, _) => NoDevices = !Devices.Any(); 
        DoRefreshCommand();
    }
    
    public async void RegisterDevice(Models model, string mac, string name)
    {
        Settings.Instance.RegisteredDevice.Model = model;
        Settings.Instance.RegisteredDevice.MacAddress = mac;
        Settings.Instance.RegisteredDevice.Name = name;
        
        Dispatcher.UIThread.Post(() => _dialog.Hide(ContentDialogResult.Primary));

        var cd = new ContentDialog
        {
            Title = Loc.Resolve("please_wait"),
            Content = Loc.Resolve("connlost_connecting"),
            CloseButtonText = Loc.Resolve("cancel"),
            CloseButtonCommand = new MiniCommand(
                (p) => _ = BluetoothService.Instance.DisconnectAsync()
            )
        };
        _ = cd.ShowAsync(MainWindow.Instance);
        
        await BluetoothService.Instance.ConnectAsync();
        cd.Hide();
    }
    
    public void DoConnectCommand(BluetoothDevice device)
    {
        var spec = DeviceSpecHelper.FindByDeviceName(device.Name);
        if (spec == null || device.IsConnected == false || device.Address == string.Empty)
        {
            _ = new MessageBox
            {
                Title = Loc.Resolve("error"),
                Description = Loc.Resolve("devsel_invalid_selection")
            }.ShowAsync();
            return;
        }

        RegisterDevice(spec.Device, device.Address, device.Name);
    }
    
    public async void DoRefreshCommand()
    {
        IEnumerable<BluetoothDevice> devices;
        try
        {
            devices = await BluetoothService.Instance.GetDevicesAsync();
        }
        catch (PlatformNotSupportedException ex)
        {
            await new MessageBox()
            {
                Title = Loc.Resolve("error"),
                Description = ex.Message
            }.ShowAsync(MainWindow.Instance);
            return;
        }

        Devices.Clear();
        devices
            .Where(dev => dev.IsConnected)
            .Where(dev => DeviceSpecHelper.FindByDeviceName(dev.Name) != null)
            .ToList()
            .ForEach(x => Devices.Add(x));
    }
}