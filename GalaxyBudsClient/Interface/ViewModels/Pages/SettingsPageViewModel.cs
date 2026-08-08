using System.Reflection;
using Avalonia.Controls;
using Avalonia.Threading;
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
        BluetoothImpl.Instance.Connected += (_, _) => RefreshMultipointState();
        BluetoothImpl.Instance.Disconnected += (_, _) => RefreshMultipointState();
        RefreshMultipointState();
    }

    private void OnDeviceChanged(object? sender, Device? e)
    {
        CanManageDevices = BluetoothImpl.HasValidDevice;
        RefreshMultipointState();
    }

    /* Disconnection events arrive on the Bluetooth backend's thread, so bounce back to the UI thread */
    public void RefreshMultipointState() => Dispatcher.UIThread.Post(() =>
    {
        IsMultipointSupported = MultipointPatcher.IsSupportedByCurrentDevice;
        CanApplyMultipoint = IsMultipointSupported && BluetoothImpl.Instance.IsConnected;
    });

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
    [Reactive] private bool _isMultipointSupported;
    [Reactive] private bool _canApplyMultipoint;
    
    public string CurrentVersion => Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";
    public override Control CreateView() => new SettingsPage { DataContext = this };

    public override string TitleKey => Keys.SettingsHeader;
    public override Symbol IconKey => Symbol.Settings;
    public override bool ShowsInFooter => true;
}
