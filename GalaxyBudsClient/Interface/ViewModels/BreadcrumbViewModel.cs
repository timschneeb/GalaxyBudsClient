using System;
using System.ComponentModel;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Interface;
using ReactiveUI;

namespace GalaxyBudsClient.Interface.ViewModels;

public class BreadcrumbViewModel : ViewModelBase
{
    public BreadcrumbViewModel(string titleKey, Type pageType)
    {
        TitleKey = titleKey;
        PageType = pageType;
        
        if(titleKey == DeviceNameKey)
            BluetoothImpl.Instance.PropertyChanged += OnDevicePropertyChanged;
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
    
    public Type PageType { get; }
    public string Title
    {
        get
        {
            var name = BluetoothImpl.Instance.Device.Current?.Name ?? Strings.Home;
            if(TitleKey == DeviceNameKey && name.Length > 0)
                return name;
            return Loc.Resolve(TitleKey);
        }
    }

    protected string TitleKey { get; }
    public static string DeviceNameKey => "%device_name%";
}