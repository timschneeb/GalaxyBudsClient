using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.Elements;
using GalaxyBudsClient.Interface.Items;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;
using Org.BouncyCastle.Crypto.Parameters;
using Serilog;

namespace GalaxyBudsClient.Interface.Pages
{
    public class SystemPage : AbstractPage
    {
        public override Pages PageType => Pages.System;
		
        public SystemPage()
        {   
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnPageShown()
        {	
            this.GetControl<Control>("FirmwareSeparator").IsVisible =
                BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.FirmwareUpdates);
            this.GetControl<Control>("FirmwareItem").IsVisible =
                BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.FirmwareUpdates);
            this.GetControl<Control>("SpatialSensorSeparator").IsVisible =
                BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.SpatialSensor);
            this.GetControl<Control>("SpatialSensorItem").IsVisible =
                BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.SpatialSensor);

        }

        private void BackButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            MainWindow.Instance.Pager.SwitchPage(Pages.Home);
        }
		
        private void FactoryReset_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            MainWindow.Instance.Pager.SwitchPage(Pages.FactoryReset);
        }

        private void RunSelfTest_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            MainWindow.Instance.Pager.SwitchPage(Pages.SelfTest);
        }

        private void SystemInfo_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            MainWindow.Instance.Pager.SwitchPage(Pages.SystemInfo);
        }

        private async void PairingMode_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.UNK_PAIRING_MODE);
            await BluetoothImpl.Instance.DisconnectAsync();
        }
        
        private async void Firmware_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (!SettingsProvider.Instance.FirmwareWarningAccepted)
            {
                var result = await new QuestionBox()
                {
                    Title = Loc.Resolve("fw_disclaimer"),
                    Description = Loc.Resolve("fw_disclaimer_desc"),
                    MinWidth = 600,
                    MaxWidth = 600
                }.ShowDialog<bool>(MainWindow.Instance);

                SettingsProvider.Instance.FirmwareWarningAccepted = result;
                if (result)
                {
                    MainWindow.Instance.Pager.SwitchPage(Pages.FirmwareSelect);
                }
            }
            else
            {
                MainWindow.Instance.Pager.SwitchPage(Pages.FirmwareSelect);
            }
        }

        private void SpatialSensor_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            MainWindow.Instance.Pager.SwitchPage(Pages.SpatialTest);
        }
    }
}