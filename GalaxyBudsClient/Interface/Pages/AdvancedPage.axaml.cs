using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Decoder;
using GalaxyBudsClient.Interface.Items;
using GalaxyBudsClient.Message;
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
		private readonly SwitchDetailListItem _gamingMode;
		
		public AdvancedPage()
		{   
			AvaloniaXamlLoader.Load(this);
			_seamlessConnection = this.FindControl<SwitchDetailListItem>("SeamlessConnection");
			_resumeSensor = this.FindControl<SwitchDetailListItem>("ResumeSensor");
			_sidetone = this.FindControl<SwitchDetailListItem>("Sidetone");
			_passthrough = this.FindControl<SwitchDetailListItem>("Passthrough");
			_gamingMode = this.FindControl<SwitchDetailListItem>("GamingMode");
			
			SPPMessageHandler.Instance.ExtendedStatusUpdate += InstanceOnExtendedStatusUpdate;
		}

		private void InstanceOnExtendedStatusUpdate(object? sender, ExtendedStatusUpdateParser e)
		{
			_sidetone.IsChecked = e.SideToneEnabled;
			_passthrough.IsChecked = e.RelieveAmbient;
			_gamingMode.IsChecked = e.AdjustSoundSync;

			_seamlessConnection.IsChecked = e.SeamlessConnectionEnabled;
			_resumeSensor.IsChecked = SettingsProvider.Instance.ResumePlaybackOnSensor;
		}

		public override void OnPageShown()
		{
			_sidetone.Parent.IsVisible = BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.AmbientSidetone);
			_passthrough.Parent.IsVisible = BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.AmbientPassthrough);
			_gamingMode.Parent.IsVisible = BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.GamingMode);
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
			await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.MSG_ID_SET_SEAMLESS_CONNECTION, !e);
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
			await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.MSG_ID_SET_SIDETONE, e);
		}

		private async void Passthrough_OnToggled(object? sender, bool e)
		{
			await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.MSG_ID_PASS_THROUGH, e);
		}

		private async void GamingMode_OnToggled(object? sender, bool e)
		{
			await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.MSG_ID_ADJUST_SOUND_SYNC, e);
		}
	}
}
