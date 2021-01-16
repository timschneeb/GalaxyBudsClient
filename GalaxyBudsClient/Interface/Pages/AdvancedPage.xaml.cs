using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Interface.Items;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.DynamicLocalization;
using Serilog;

namespace GalaxyBudsClient.Interface.Pages
{
 	public class AdvancedPage : AbstractPage
	{
		public override Pages PageType => Pages.Advanced;
		
		private readonly SwitchDetailListItem _seamlessConnection;
		private readonly SwitchDetailListItem _resumeSensor;
		private readonly SwitchDetailListItem _sidetone;
		private readonly SwitchDetailListItem _passthrough;
		
		public AdvancedPage()
		{   
			AvaloniaXamlLoader.Load(this);
			_seamlessConnection = this.FindControl<SwitchDetailListItem>("SeamlessConnection");
			_resumeSensor = this.FindControl<SwitchDetailListItem>("ResumeSensor");
			_sidetone = this.FindControl<SwitchDetailListItem>("Sidetone");
			_passthrough = this.FindControl<SwitchDetailListItem>("Passthrough");
			
			SPPMessageHandler.Instance.ExtendedStatusUpdate += InstanceOnExtendedStatusUpdate;
		}

		private void InstanceOnExtendedStatusUpdate(object? sender, ExtendedStatusUpdateParser e)
		{
			_sidetone.IsChecked = e.SideToneEnabled;
			_passthrough.IsChecked = e.RelieveAmbient;

			_seamlessConnection.IsChecked = e.SeamlessConnectionEnabled;
			_resumeSensor.IsChecked = SettingsProvider.Instance.ResumePlaybackOnSensor;
		}

		public override void OnPageShown()
		{
			this.FindControl<Border>("Hotkeys").IsVisible = PlatformUtils.SupportsHotkeys;
			this.FindControl<Separator>("SidetoneS").IsVisible = BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.AmbientSidetone);
			this.FindControl<Separator>("PassthroughS").IsVisible = BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.AmbientPassthrough);
			_sidetone.Parent.IsVisible = BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.AmbientSidetone);
			_passthrough.Parent.IsVisible = BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.AmbientPassthrough);
		}
		

		private void BackButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.Home);
		}

		private async void SeamlessConnection_OnToggled(object? sender, bool e)
		{
			if (!BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.SeamlessConnection))
			{
				MainWindow.Instance.ShowUnsupportedFeaturePage(
					string.Format(
						Loc.Resolve("adv_required_firmware_later"), 
						BluetoothImpl.Instance.DeviceSpec.RecommendedFwVersion(IDeviceSpec.Feature.SeamlessConnection)));
				return;
			}
			await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.SET_SEAMLESS_CONNECTION, !e);
		}

		private void ResumeSensor_OnToggled(object? sender, bool e)
		{
			SettingsProvider.Instance.ResumePlaybackOnSensor = e;
		}

		private async void Sidetone_OnToggled(object? sender, bool e)
		{
			if (!BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.AmbientSidetone))
			{
				MainWindow.Instance.ShowUnsupportedFeaturePage(
					string.Format(
						Loc.Resolve("adv_required_firmware_later"),
						BluetoothImpl.Instance.DeviceSpec.RecommendedFwVersion(IDeviceSpec.Feature.AmbientSidetone)));
				return;
			}
			await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.SET_SIDETONE, e);
		}

		private async void Passthrough_OnToggled(object? sender, bool e)
		{
			await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.PASS_THROUGH, e);
		}

		private void Hotkeys_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.Hotkeys);
		}
	}
}
