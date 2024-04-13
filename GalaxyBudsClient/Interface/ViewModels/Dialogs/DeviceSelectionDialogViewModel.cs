using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Bluetooth;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
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

    // Only used for design-time data
    public DeviceSelectionDialogViewModel() : this(new ContentDialog()) {}
    
    public DeviceSelectionDialogViewModel(ContentDialog dialog)
    {
        _dialog = dialog;
        Devices.CollectionChanged += (_, _) => NoDevices = !Devices.Any(); 
        DoRefreshCommand();
    }
    
    public async void RegisterDevice(Models model, string mac, string name)
    {
        LegacySettings.Instance.DeviceLegacy.Model = model;
        LegacySettings.Instance.DeviceLegacy.MacAddress = mac;
        LegacySettings.Instance.DeviceLegacy.Name = name;
        
        Dispatcher.UIThread.Post(() => _dialog.Hide(ContentDialogResult.Primary));

        var cd = new ContentDialog
        {
            Title = Strings.PleaseWait,
            Content = Strings.ConnlostConnecting,
            CloseButtonText = Strings.Cancel,
            CloseButtonCommand = new MiniCommand(p => _ = BluetoothImpl.Instance.DisconnectAsync())
        };
        _ = cd.ShowAsync(MainWindow.Instance);
        
        await BluetoothImpl.Instance.ConnectAsync();
        cd.Hide();
    }
    
    public void DoConnectCommand(BluetoothDevice device)
    {
        var spec = DeviceSpecHelper.FindByDeviceName(device.Name);
        if (spec == null || device.IsConnected == false || device.Address == string.Empty)
        {
            _ = new MessageBox
            {
                Title = Strings.Error,
                Description = Strings.DevselInvalidSelection
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
            devices = await BluetoothImpl.Instance.GetDevicesAsync();
        }
        catch (PlatformNotSupportedException ex)
        {
            await new MessageBox
            {
                Title = Strings.Error,
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