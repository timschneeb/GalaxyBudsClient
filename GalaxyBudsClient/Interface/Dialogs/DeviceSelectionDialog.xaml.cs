using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GalaxyBudsClient.Bluetooth;
using GalaxyBudsClient.Interface.Elements;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using Serilog;

namespace GalaxyBudsClient.Interface.Dialogs
{
    public sealed class DeviceSelectionDialog : Window
    {
        private readonly SemaphoreSlim _deviceLock = new SemaphoreSlim(1,1);
        
        public ObservableCollection<BluetoothDevice>? AvailableDevices
        {
            get => _deviceBox.Items as ObservableCollection<BluetoothDevice>;
            set => _deviceBox.Items = value;
        }

        public SelectionModel<BluetoothDevice>? Selection
        {
            get => _deviceBox.Selection as SelectionModel<BluetoothDevice>;
            set => _deviceBox.Selection = value;
        }
        
        public bool IsSearching
        {
            set => _loadingSpinner.IsVisible = value;
            get => _loadingSpinner.IsVisible;
        }

        private readonly ListBox _deviceBox;
        private readonly LoadingSpinner _loadingSpinner;
        
        public DeviceSelectionDialog()
        {
            AvaloniaXamlLoader.Load(this);
            this.AttachDevTools();

            _loadingSpinner = this.FindControl<LoadingSpinner>("LoadingSpinner");
            _deviceBox = this.FindControl<ListBox>("Devices");

            AvailableDevices = new ObservableCollection<BluetoothDevice>();
            Selection = new SelectionModel<BluetoothDevice>();
            
            IsSearching = false;
            RefreshList();
        }
        
        private void Cancel_OnClick(object? sender, RoutedEventArgs e)
        {
            Close(null);
        }

        private async void Apply_OnClick(object? sender, RoutedEventArgs e)
        {   
            Close(Selection?.SelectedItem ?? new BluetoothDevice(String.Empty, String.Empty, false, false, new BluetoothCoD(0)));
        }

        private void OnOpened(object? sender, EventArgs e)
        {
            
        }

        private async void RefreshList()
        {
            if (IsSearching)
            {
                Log.Warning("DeviceSelectionDialog: Refresh already in progress");
                return;
            }

            await _deviceLock.WaitAsync();
            
            IsSearching = true;
            AvailableDevices?.Clear();

            try
            {
                var devices = await BluetoothImpl.Instance.GetDevicesAsync();
                devices
                    .Where(dev => dev.IsConnected)
                    .Where(dev => DeviceSpecHelper.FindByDeviceName(dev.Name) != null)
                    .ToList()
                    .ForEach(x => AvailableDevices?.Add(x));

                IsSearching = false;
            }
            finally
            {
                _deviceLock.Release();
            }
        }

        private void Reload_OnClick(object? sender, RoutedEventArgs e)
        {
            RefreshList();
        }
    }
}