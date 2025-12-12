using System.Reflection;
using Avalonia.Controls;
using FluentIcons.Common;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Platform;
using ReactiveUI;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public partial class SettingsPageViewModel : MainPageViewModelBase
{
    public SettingsPageViewModel()
    {
        CanManageDevices = BluetoothImpl.HasValidDevice;
        BluetoothImpl.Instance.Device.DeviceChanged += OnDeviceChanged;
    }

    private void OnDeviceChanged(object? sender, Device? e)
    {
        CanManageDevices = BluetoothImpl.HasValidDevice;
    }

    public bool IsAutoStartEnabled
    {
        get => PlatformImpl.DesktopServices.IsAutoStartEnabled;
        set
        {
            PlatformImpl.DesktopServices.IsAutoStartEnabled = value;
            this.RaisePropertyChanged();
        }
    }
    
    [Reactive] private object _canManageDevices = false;
    
    public string CurrentVersion => Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";
    public override Control CreateView() => new SettingsPage { DataContext = this };

    public override string TitleKey => Keys.SettingsHeader;
    public override Symbol IconKey => Symbol.Settings;
    public override bool ShowsInFooter => true;
}
