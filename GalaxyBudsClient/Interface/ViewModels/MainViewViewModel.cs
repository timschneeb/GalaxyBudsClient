using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.ViewModels.Pages;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using ReactiveUI.Fody.Helpers;

namespace GalaxyBudsClient.Interface.ViewModels;

public class MainViewViewModel : ViewModelBase
{
    public MainViewViewModel()
    {
        NavigationFactory = new NavigationFactory(this);
        LegacySettings.Instance.DeviceLegacy.PropertyChanged += OnDevicePropertyChanged;
        
        BluetoothImpl.Instance.Connecting += (_, _) =>
        {
            IsConnectButtonEnabled = false;
            ConnectButtonText = Strings.ConnlostConnecting;
        };
        BluetoothImpl.Instance.BluetoothError += (_, _) =>
        {
            IsConnectButtonEnabled = true;
            ConnectButtonText = Strings.ConnlostConnect;
        };
        BluetoothImpl.Instance.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(BluetoothImpl.Instance.IsConnected))
                return;
            IsConnectButtonEnabled = true;
            ConnectButtonText = Strings.ConnlostConnect;
        };
    }

    [Reactive] public bool IsConnectButtonEnabled { set; get; } = true;
    [Reactive] public string ConnectButtonText { set; get; } = Strings.ConnlostConnect;
    [Reactive] public bool IsInSetupWizard { set; get; } = !BluetoothImpl.IsRegisteredDeviceValid;
    public NavigationFactory NavigationFactory { get; }
    public required Func<Type, PageViewModelBase?> VmResolver { get; init; }
    public ObservableCollection<BreadcrumbViewModel> BreadcrumbItems { get; } = [
        // Workaround: The Breadcrumb library crashes if there are no items when it tries to measure its width 
        new BreadcrumbViewModel("", typeof(HomePageViewModel)) 
    ];
    
    private void OnDevicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is
            nameof(LegacySettings.Instance.DeviceLegacy.MacAddress) or
            nameof(LegacySettings.Instance.DeviceLegacy.Model))
            IsInSetupWizard = !BluetoothImpl.IsRegisteredDeviceValid;
    }
}

public class NavigationFactory(MainViewViewModel owner) : INavigationPageFactory
{
    private MainViewViewModel Owner { get; } = owner;

    public Control GetPage(Type srcType)
    {
        var model = Owner.VmResolver(srcType);
        if (model is null) 
            throw new ArgumentException("No view model found for type");
        
        return GetPageFromObject(model);
    }

    public Control GetPageFromObject(object target)
    {
        if (target is not PageViewModelBase model) 
            throw new ArgumentException("Target must derive from ViewModelBase");
        
        var view = model.CreateView();
        view.DataContext = model;
        return view;
    }
}
