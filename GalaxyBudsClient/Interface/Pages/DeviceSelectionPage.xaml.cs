using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Bluetooth;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.Elements;
using GalaxyBudsClient.Interface.Items;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.DynamicLocalization;
using JetBrains.Annotations;
using Serilog;

namespace GalaxyBudsClient.Interface.Pages
{
    public class DeviceSelectionPage : AbstractPage
    {
        public override Pages PageType => Pages.DeviceSelect;
        
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
            set => _pageHeader.LoadingSpinnerVisible = value;
            get => _pageHeader.LoadingSpinnerVisible;
        }

        private readonly ListBox _deviceBox;
        private readonly PageHeader _pageHeader;
        private readonly Border _navBarNext;

        private bool _enableDummyDevices = false;
        public bool EnableDummyDevices
        {
            set
            {
                _enableDummyDevices = value;
                RefreshList();
            }
            get => _enableDummyDevices;
        }
        
        public DeviceSelectionPage()
        {
            AvaloniaXamlLoader.Load(this);
            _navBarNext = this.FindControl<Border>("NavBarNext");
            _pageHeader = this.FindControl<PageHeader>("PageHeader");
            _deviceBox = this.FindControl<ListBox>("Devices");

            AvailableDevices = new ObservableCollection<BluetoothDevice>();
            Selection = new SelectionModel<BluetoothDevice>();
            
            IsSearching = false;
            
            RefreshList();
        }
		
        private void BackButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            MainWindow.Instance.Pager.SwitchPage(Pages.Welcome);
        }

        private void SelectDevice_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            RefreshList(true);
        }

        private void Next_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            
            if (Selection == null || 
                Selection.Count <= 0 || 
                Selection.SelectedItem == null)
            {
                new MessageBox()
                {
                    Title = Loc.Resolve("error"),
                    Description = Loc.Resolve("devsel_invalid_selection")
                }.ShowDialog(MainWindow.Instance);
                return;
            }

            var selection = Selection.SelectedItem!;
            var spec = DeviceSpecHelper.FindByDeviceName(selection.Name);

            if (spec == null || selection.IsConnected == false || selection.Address == string.Empty)
            {
                new MessageBox()
                {
                    Title = Loc.Resolve("error"),
                    Description = Loc.Resolve("devsel_invalid_selection")
                }.ShowDialog(MainWindow.Instance);
                return;
            }

            SettingsProvider.Instance.RegisteredDevice.Model = spec.Device;
            SettingsProvider.Instance.RegisteredDevice.MacAddress = selection.Address;

            MainWindow.Instance.Pager.SwitchPage(Pages.Home);

            Task.Factory.StartNew(() => BluetoothImpl.Instance.ConnectAsync());
        }

        private async void RefreshList(bool user = false)
        {
            if (IsSearching)
            {
                Log.Warning("DeviceSelectionDialog: Refresh already in progress");
                return;
            }
            
            IsSearching = true;
            _navBarNext.IsVisible = false;
            AvailableDevices?.Clear();


            BluetoothDevice[] devices;
            try
            {
                devices = await BluetoothImpl.Instance.GetDevicesAsync();
            }
            catch (PlatformNotSupportedException ex)
            {
                IsSearching = false;
                
                if (!user)
                {
                    return;
                }
                
                await new MessageBox()
                {
                    Title = Loc.Resolve("error"),
                    Description = ex.Message
                }.ShowDialog(MainWindow.Instance);
                return;
            }

            devices
                .Where(dev => dev.IsConnected)
                .Where(dev => DeviceSpecHelper.FindByDeviceName(dev.Name) != null)
                .ToList()
                .ForEach(x => AvailableDevices?.Add(x));

            if (_enableDummyDevices)
            {
                /* Dummy devices for testing */
                AvailableDevices?.Add(new BluetoothDevice("Galaxy Buds (36FD) [Dummy]", "36:AB:38:F5:04:FD", true, true,
                    new BluetoothCoD(0)));
                AvailableDevices?.Add(new BluetoothDevice("Galaxy Buds+ (A2D5) [Dummy]", "A2:BF:D4:4A:52:D5", true,
                    true, new BluetoothCoD(0)));
                AvailableDevices?.Add(new BluetoothDevice("Galaxy Buds Live (4AC3) [Dummy]", "4A:6B:87:E5:12:C3", true,
                    true, new BluetoothCoD(0)));
                AvailableDevices?.Add(new BluetoothDevice("Galaxy Buds Pro (E43F) [Dummy]", "E4:25:FA:6D:B9:3F", true,
                    true, new BluetoothCoD(0)));
            }
            

            if (AvailableDevices?.Count <= 0)
            {
                AvailableDevices?.Add(new BluetoothDevice(Loc.Resolve("devsel_nodevices_title"), 
                    Loc.Resolve("devsel_nodevices"), false, false, new BluetoothCoD(0)));
                _deviceBox.IsEnabled = false;
            }
            else
            {
                _deviceBox.IsEnabled = true;
            }
            
            IsSearching = false;
        }

        private void Devices_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            _navBarNext.IsVisible = true;
        }
    }
}