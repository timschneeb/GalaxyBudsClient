using System.Reflection;
using Avalonia.Controls;
using FluentIcons.Common;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Platform;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class SettingsPageViewModel : MainPageViewModelBase
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
        get => PlatformImpl.AutoStart.Enabled;
        set
        {
            PlatformImpl.AutoStart.Enabled = value;
            this.RaisePropertyChanged();
        }
    }
    
    [Reactive] public object CanManageDevices { set; get; }
    
    public string CurrentVersion => Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "Unknown";
    public override Control CreateView() => new SettingsPage { DataContext = this };

    public override string TitleKey => Keys.SettingsHeader;
    public override Symbol IconKey => Symbol.Settings;
    public override bool ShowsInFooter => true;
}
