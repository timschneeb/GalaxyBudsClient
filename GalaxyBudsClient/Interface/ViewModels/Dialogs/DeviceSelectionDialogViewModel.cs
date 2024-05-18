using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Platform.Model;
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
        // Remove existing device with the same MAC address
        var existingDevice = Settings.Data.Devices.FirstOrDefault(x => x.MacAddress == mac);
        if(existingDevice != null)
        {
            Settings.Data.Devices.Remove(existingDevice);
        }
        
        // Add new device
        var device = new Device
        {
            Model = model,
            MacAddress = mac,
            Name = name
        };
        Settings.Data.Devices.Add(device);
        
        // Hide this dialog
        Dispatcher.UIThread.Post(() => _dialog.Hide(ContentDialogResult.Primary));

        // Show connection dialog and begin connection
        var cd = new ContentDialog
        {
            Title = Strings.PleaseWait,
            Content = Strings.ConnlostConnecting,
            CloseButtonText = Strings.Cancel,
            CloseButtonCommand = new MiniCommand(p => _ = BluetoothImpl.Instance.DisconnectAsync())
        };
        _ = cd.ShowAsync(MainWindow.Instance);
        
        if(BluetoothImpl.Instance.IsConnected)
            await BluetoothImpl.Instance.DisconnectAsync();
        
        BluetoothImpl.Instance.Device.Current = device;
        await BluetoothImpl.Instance.ConnectAsync(device);
        
        cd.Hide();
    }
    
    public void DoConnectCommand(BluetoothDevice device)
    {
        var spec = DeviceSpecHelper.FindByDevice(device);
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
            .Where(dev => DeviceSpecHelper.FindByDevice(dev) != null)
            .ToList()
            .ForEach(x => Devices.Add(x));
    }
}