using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Bluetooth;
using GalaxyBudsClient.Interface.Dialogs;
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

		private readonly DetailListItem _devName;
		private readonly DetailListItem _devAddress;
		private readonly DetailListItem _devModel;
		
		private readonly Border _navBar;

		private DeviceSelectionDialog? _deviceSelectionDialog;
		private BluetoothDevice? _selectedDevice;
		private IDeviceSpec? _selectedDeviceSpec;

		public BluetoothDevice? SelectedDevice
		{
			get => _selectedDevice;
			set
			{
				_selectedDevice = value;
				_navBar.IsVisible = value != null;
			}
		}

		public DeviceSelectionPage()
		{
			AvaloniaXamlLoader.Load(this);
			_devName = this.FindControl<DetailListItem>("DevName");
			_devAddress = this.FindControl<DetailListItem>("DevAddress");
			_devModel = this.FindControl<DetailListItem>("DevModel");
			_navBar = this.FindControl<Border>("NavBar");
			
			// Loc.LanguageUpdated += UpdateStrings;
		}
		
		private void BackButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.Welcome);
		}

		private async void SelectDevice_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			_deviceSelectionDialog = new DeviceSelectionDialog();
			var result = await _deviceSelectionDialog.ShowDialog<BluetoothDevice?>(MainWindow.Instance);
			if (result == null || result.Name == string.Empty)
			{
				return;
			}
			
			var spec = DeviceSpecHelper.FindByDeviceName(result.Name);
			
			_selectedDevice = result;
			_selectedDeviceSpec = spec;
			
			if (spec == null)
			{
				/* This should never happen! */
				_devModel.Description = Loc.Resolve("settings_cpopup_position_placeholder");
				_devName.Description = $"{result.Name} (UNSUPPORTED)";
				_devAddress.Description = result.Address;
				Log.Warning($"DeviceSelectionPage: IDeviceSpec is NULL ({result.Name})");
				return;
			}
			
			_devModel.Description = spec.Device.GetDescription();
			_devName.Description = result.Name;
			_devAddress.Description = result.Address;
		}

		private void Next_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			if (_selectedDevice == null || _selectedDeviceSpec == null)
			{
				new MessageBox()
				{
					Title = Loc.Resolve("error"),
					Description = Loc.Resolve("devsel_invalid_selection")
				}.ShowDialog(MainWindow.Instance);
				return;
			}

			SettingsProvider.Instance.RegisteredDevice.Model = _selectedDeviceSpec.Device;
			SettingsProvider.Instance.RegisteredDevice.MacAddress = _selectedDevice.Address;

			MainWindow.Instance.Pager.SwitchPage(Pages.Home);

			var _ = BluetoothImpl.Instance.ConnectAsync();
		}
	}
}
