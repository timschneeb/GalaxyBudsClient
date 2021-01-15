using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Interface.Elements;
using GalaxyBudsClient.Interface.Items;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.DynamicLocalization;
using Serilog;

namespace GalaxyBudsClient.Interface.Pages
{
 	public class SystemInfoPage : AbstractPage
	{
		public override Pages PageType => Pages.SystemInfo;
		
		private string Waiting => Loc.Resolve("system_waiting_for_device");
		private string Left => Loc.Resolve("left");
		private string Right => Loc.Resolve("right");
		
		private readonly PageHeader _pageHeader;
		private readonly DetailListItem _hwVer;
		private readonly DetailListItem _swVer;
		private readonly DetailListItem _touchFwVer;
		private readonly DetailListItem _btAddr;
		private readonly DetailListItem _serialNumber;
		private readonly DetailListItem _buildString;
		private readonly DetailListItem _batteryType;
		private readonly DetailListItem _revision;
		
		public SystemInfoPage()
		{   
			AvaloniaXamlLoader.Load(this);
			_pageHeader = this.FindControl<PageHeader>("PageHeader");
			_hwVer = this.FindControl<DetailListItem>("HwVer");
			_swVer = this.FindControl<DetailListItem>("SwVer");
			_touchFwVer = this.FindControl<DetailListItem>("TouchFwVer");
			_btAddr = this.FindControl<DetailListItem>("BtAddr");
			_serialNumber = this.FindControl<DetailListItem>("SerialNumber");
			_buildString = this.FindControl<DetailListItem>("BuildString");
			_batteryType = this.FindControl<DetailListItem>("BatteryType");
			_revision = this.FindControl<DetailListItem>("ProtocolRevision");

			SPPMessageHandler.Instance.GetAllDataResponse += InstanceOnGetAllDataResponse;
			SPPMessageHandler.Instance.BatteryTypeResponse += InstanceOnBatteryTypeResponse;
			SPPMessageHandler.Instance.BuildStringResponse += InstanceOnBuildStringResponse;
			SPPMessageHandler.Instance.ExtendedStatusUpdate += InstanceOnExtendedStatusUpdate;
			SPPMessageHandler.Instance.SerialNumberResponse += InstanceOnSerialNumberResponse;
			
			Loc.LanguageUpdated += OnLanguageUpdated;
		}

		private void InstanceOnSerialNumberResponse(object? sender, DebugSerialNumberParser e)
		{
			_serialNumber.Description = $"{Left}: {e.LeftSerialNumber}, {Right}: {e.RightSerialNumber}";
		}

		private void InstanceOnExtendedStatusUpdate(object? sender, ExtendedStatusUpdateParser e)
		{
			_revision.Description = $"rev{e.Revision}";
		}

		private void InstanceOnBuildStringResponse(object? sender, string e)
		{
			if (e == string.Empty)
			{
				return;
			}
			_buildString.Description = e.Length > 2 ? e.Remove(0,2) : e;
		}

		private void InstanceOnBatteryTypeResponse(object? sender, BatteryTypeParser e)
		{
			_batteryType.Description = $"{Left}: {e.LeftBatteryType}, {Right}: {e.RightBatteryType}";
			if (BluetoothImpl.Instance.ActiveModel != Models.Buds)
			{
				_batteryType.Description = Loc.Resolve("system_battery_type_unknown");
			}
		}

		private void InstanceOnGetAllDataResponse(object? sender, DebugGetAllDataParser e)
		{
			_pageHeader.LoadingSpinnerVisible = false;
			_hwVer.Description = e.HardwareVersion ?? Loc.Resolve("unknown");
			_swVer.Description = e.SoftwareVersion ?? Loc.Resolve("unknown");
			_touchFwVer.Description = e.TouchSoftwareVersion ?? Loc.Resolve("unknown");
			_btAddr.Description = $"{Left}: {e.LeftBluetoothAddress}, {Right}: {e.RightBluetoothAddress}"; 
		}

		private async void OnLanguageUpdated()
		{
			if (MainWindow.Instance.Pager.CurrentPage == Pages.SystemInfo)
			{
				await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.BATTERY_TYPE);
				await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.DEBUG_SERIAL_NUMBER);
				await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.DEBUG_BUILD_INFO);
				await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.DEBUG_GET_ALL_DATA);
			}
		}

		public override async void OnPageShown()
		{
			_pageHeader.LoadingSpinnerVisible = true;
			_hwVer.Description = Waiting;
			_swVer.Description = Waiting;
			_touchFwVer.Description = Waiting;
			_btAddr.Description = Waiting;
			_serialNumber.Description = Waiting;
			_buildString.Description = Waiting;
			_batteryType.Description = Waiting;
			_revision.Description = DeviceMessageCache.Instance.ExtendedStatusUpdate?.Revision.ToString() ?? Waiting;

			await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.BATTERY_TYPE);
			await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.DEBUG_SERIAL_NUMBER);
			await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.DEBUG_BUILD_INFO);
			await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.DEBUG_GET_ALL_DATA);
		}

		private void BackButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.System);
		}
	}
}
