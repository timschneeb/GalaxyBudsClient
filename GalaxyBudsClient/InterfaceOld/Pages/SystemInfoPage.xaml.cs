using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using GalaxyBudsClient.InterfaceOld.Elements;
using GalaxyBudsClient.InterfaceOld.Items;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;

namespace GalaxyBudsClient.InterfaceOld.Pages
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
		private readonly Separator _separatorLegacyDebug1;
		private readonly Separator _separatorLegacyDebug2;
		
		public SystemInfoPage()
		{   
			AvaloniaXamlLoader.Load(this);
			_pageHeader = this.GetControl<PageHeader>("PageHeader");
			_hwVer = this.GetControl<DetailListItem>("HwVer");
			_swVer = this.GetControl<DetailListItem>("SwVer");
			_touchFwVer = this.GetControl<DetailListItem>("TouchFwVer");
			_btAddr = this.GetControl<DetailListItem>("BtAddr");
			_serialNumber = this.GetControl<DetailListItem>("SerialNumber");
			_buildString = this.GetControl<DetailListItem>("BuildString");
			_batteryType = this.GetControl<DetailListItem>("BatteryType");
			_revision = this.GetControl<DetailListItem>("ProtocolRevision");
			_separatorLegacyDebug1 = this.GetControl<Separator>("SeparatorLegacyDebug1");
			_separatorLegacyDebug2 = this.GetControl<Separator>("SeparatorLegacyDebug2");

			SppMessageHandler.Instance.GetAllDataResponse += InstanceOnGetAllDataResponse;
			SppMessageHandler.Instance.BatteryTypeResponse += InstanceOnBatteryTypeResponse;
			SppMessageHandler.Instance.BuildStringResponse += InstanceOnBuildStringResponse;
			SppMessageHandler.Instance.ExtendedStatusUpdate += InstanceOnExtendedStatusUpdate;
			SppMessageHandler.Instance.SerialNumberResponse += InstanceOnSerialNumberResponse;
			
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
			if (BluetoothImpl.ActiveModel != Models.Buds)
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
				if (BluetoothImpl.Instance.DeviceSpec.Supports(Features.DebugInfoLegacy))
				{
					await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.BATTERY_TYPE);
					await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.DEBUG_BUILD_INFO);
				}

				await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.DEBUG_SERIAL_NUMBER);
				await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.DEBUG_GET_ALL_DATA);
			}
		}

		public override async void OnPageShown()
		{
			this.GetControl<Control>("TraceCoreDump").IsVisible =
				BluetoothImpl.Instance.DeviceSpec.Supports(Features.FragmentedMessages);
			
			_pageHeader.LoadingSpinnerVisible = true;
			_hwVer.Description = Waiting;
			_swVer.Description = Waiting;
			_touchFwVer.Description = Waiting;
			_btAddr.Description = Waiting;
			_serialNumber.Description = Waiting;
			_buildString.Description = Waiting;
			_batteryType.Description = Waiting;
			_revision.Description = DeviceMessageCache.Instance.ExtendedStatusUpdate?.Revision.ToString() ?? Waiting;

			var supportsLegacyDebug = BluetoothImpl.Instance.DeviceSpec.Supports(Features.DebugInfoLegacy);

			_buildString.IsVisible = _buildString.GetVisualParent()!.IsVisible = supportsLegacyDebug;
			_batteryType.IsVisible = _batteryType.GetVisualParent()!.IsVisible = supportsLegacyDebug;
			_separatorLegacyDebug1.IsVisible = _separatorLegacyDebug2.IsVisible = supportsLegacyDebug;
				
			if (supportsLegacyDebug)
			{
				await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.BATTERY_TYPE);
				await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.DEBUG_BUILD_INFO);
			}

			await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.DEBUG_SERIAL_NUMBER);
			await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.DEBUG_GET_ALL_DATA);
		}

		private void BackButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.System);
		}

		private void TraceCoreDump_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.SystemCoredump);
		}
	}
}
