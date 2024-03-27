using System;
using System.ComponentModel;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;
using ReactiveUI;

namespace GalaxyBudsClient.Interface.ViewModels;

public class BreadcrumbViewModel : ViewModelBase
{
    public BreadcrumbViewModel(string titleKey, Type pageType)
    {
        TitleKey = titleKey;
        PageType = pageType;
        
        if(titleKey == DeviceNameKey)
            BluetoothService.Instance.PropertyChanged += OnDevicePropertyChanged;
        Loc.LanguageUpdated += OnLanguageUpdated;
    }

    private void OnDevicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        this.RaisePropertyChanged(nameof(Title));
    }

    private void OnLanguageUpdated()
    {
        this.RaisePropertyChanged(nameof(Title));
    }
    
    public Type PageType { get; set; }
    public string Title
    {
        get
        {
            var name = Settings.Instance.RegisteredDevice.Name;
            if(TitleKey == DeviceNameKey && name.Length > 0)
                return name;
            return Loc.Resolve(TitleKey);
        }
    }

    protected string TitleKey { get; }
    public static string DeviceNameKey => "%device_name%";
}