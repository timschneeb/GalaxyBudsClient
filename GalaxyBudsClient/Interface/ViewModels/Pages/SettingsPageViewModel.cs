using System.ComponentModel;
using System.Reflection;
using Avalonia.Controls;
using FluentIcons.Common;
using GalaxyBudsClient.Generated.I18N;
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
        
        CanUnregister = BluetoothImpl.IsRegisteredDeviceValid;
        Settings.Instance.DeviceLegacy.PropertyChanged += OnDevicePropertyChanged;
    }
    
    private void OnDevicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is
            nameof(Settings.Instance.DeviceLegacy.MacAddress) or
            nameof(Settings.Instance.DeviceLegacy.Model))
            CanUnregister = BluetoothImpl.IsRegisteredDeviceValid;
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
