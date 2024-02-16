using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Bluetooth;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.Elements;
using GalaxyBudsClient.Interface.Items;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.DynamicLocalization;
using Serilog;

namespace GalaxyBudsClient.Interface.Pages
{
    public class DeviceSelectionPage : AbstractPage
    {
        public override Pages PageType => Pages.DeviceSelect;
        
        public ObservableCollection<BluetoothDevice>? AvailableDevices
        {
            get => _deviceBox.ItemsSource as ObservableCollection<BluetoothDevice>;
            set => _deviceBox.ItemsSource = value;
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
        }

        public override void OnPageShown()
        {
            this.FindControl<Separator>("UseWinRTSep").IsVisible = PlatformUtils.IsWindowsContractsSdkSupported;
            this.FindControl<SwitchDetailListItem>("UseWinRT").IsVisible = PlatformUtils.IsWindowsContractsSdkSupported;
            this.FindControl<SwitchDetailListItem>("UseWinRT").IsChecked = SettingsProvider.Instance.UseBluetoothWinRT;
        }

        private void BackButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            MainWindow.Instance.Pager.SwitchPage(Pages.Welcome);
        }

        private void SelectDevice_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            RefreshList(true);
        }
        
        private void ManualPair_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(async () =>
            {
                var dialog = new ManualPairDialog();
                var accepted = await dialog.ShowDialog<bool>(MainWindow.Instance);
                if (accepted)
                {
                    if (dialog.SelectedModel == Models.NULL || dialog.SelectedDeviceMac == null)
                    {
                        ShowErrorDialog();
                        return;
                    }
                
                    RegisterDevice(dialog.SelectedModel, dialog.SelectedDeviceMac);
                }
            });
        }

        private void Next_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            
            if (Selection is not { Count: > 0 } || Selection.SelectedItem == null)
            {
                ShowErrorDialog();
                return;
            }

            var selection = Selection.SelectedItem!;
            var spec = DeviceSpecHelper.FindByDeviceName(selection.Name);

            if (spec == null || selection.IsConnected == false || selection.Address == string.Empty)
            {
                ShowErrorDialog();
                return;
            }

            RegisterDevice(spec.Device, selection.Address);
        }

        private static void ShowErrorDialog()
        {
            new MessageBox()
            {
                Title = Loc.Resolve("error"),
                Description = Loc.Resolve("devsel_invalid_selection")
            }.ShowDialog(MainWindow.Instance);
        }

        private async void RegisterDevice(Models model, string mac)
        {
            SettingsProvider.Instance.RegisteredDevice.Model = model;
            SettingsProvider.Instance.RegisteredDevice.MacAddress = mac;

            MainWindow.Instance.Pager.SwitchPage(Pages.Home);

            await Task.Factory.StartNew(async () =>
            {
                MainWindow.Instance.HomePage.ResetCache();
                
                if (await BluetoothImpl.Instance.ConnectAsync())
                {
                    MainWindow.Instance.Pager.SwitchPage(Pages.Home);
                }
            });
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
                AvailableDevices?.Add(new BluetoothDevice("Galaxy Buds2 (D592) [Dummy]", "D5:97:B8:23:AB:92", true,
                    true, new BluetoothCoD(0)));
                AvailableDevices?.Add(new BluetoothDevice("Galaxy Buds2 Pro (3292) [Dummy]", "32:97:B8:23:AB:92", true,
                    true, new BluetoothCoD(0)));
                AvailableDevices?.Add(new BluetoothDevice("Galaxy Buds FE (A7D4) [Dummy]", "A7:97:B8:23:AB:D4", true,
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

        private void UseWinRT_OnToggled(object? sender, bool e)
        {
            SettingsProvider.Instance.UseBluetoothWinRT = e;
            BluetoothImpl.Reallocate();
            RefreshList(user: true);
        }
    }
}