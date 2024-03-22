using System.ComponentModel;
using System.Reflection;
using Avalonia.Controls;
using FluentIcons.Common;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class SettingsPageViewModel : MainPageViewModelBase
{

    public SettingsPageViewModel()
    {
        CanUnregister = BluetoothService.RegisteredDeviceValid;
        Settings.Instance.RegisteredDevice.PropertyChanged += OnDevicePropertyChanged;
    }

    private void OnDevicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is
            nameof(Settings.Instance.RegisteredDevice.MacAddress) or
            nameof(Settings.Instance.RegisteredDevice.Model))
            CanUnregister = BluetoothService.RegisteredDeviceValid;
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

    public override string TitleKey => "settings_header";
    public override Symbol IconKey => Symbol.Settings;
    public override bool ShowsInFooter => true;
}
