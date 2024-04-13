using System.ComponentModel;
using System.Reflection;
using Avalonia.Controls;
using FluentIcons.Common;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Model.Config.Legacy;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class SettingsPageViewModel : MainPageViewModelBase
{

    public SettingsPageViewModel()
    {
        CanUnregister = BluetoothImpl.HasValidDevice;
        BluetoothImpl.Instance.Device.DeviceChanged += OnDeviceChanged;
    }

    private void OnDeviceChanged(object? sender, Device? e)
    {
        CanUnregister = BluetoothImpl.HasValidDevice;
    }

    public bool IsAutoStartEnabled
    {
        get => AutoStartHelper.Instance.Enabled;
        set
        {
            AutoStartHelper.Instance.Enabled = value;
            this.RaisePropertyChanged();
        }
    }
    
    [Reactive] public object CanUnregister { set; get; }
    
    public string CurrentVersion => Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "Unknown";
    public override Control CreateView() => new SettingsPage();

    public override string TitleKey => Keys.SettingsHeader;
    public override Symbol IconKey => Symbol.Settings;
    public override bool ShowsInFooter => true;
}
