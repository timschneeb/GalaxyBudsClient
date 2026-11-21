using System.Text.Json;
using FluentAssertions;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform.Model;

namespace GalaxyBudsClient.Tests;

[TestFixture]
public class SettingsTests
{
    [Test]
    public void SettingsData_SerializationRoundTrip_PreservesValues()
    {
        var original = new SettingsData
        {
            Theme = Themes.Dark,
            AccentColor = 0xFF123456,
            BlurStrength = 42,
            Locale = Locales.de,
            TemperatureUnit = TemperatureUnits.Fahrenheit,
            RealisticEarbudImages = false,
            ShowSidebar = false,
            ColorOverride = DeviceIds.Buds3ProSilver,
            UseBluetoothWinRt = false,
            LastDeviceMac = "AA:BB:CC:DD:EE:FF",
            AutoEnableMultipoint = true,
            MinimizeToTray = false,
            DynamicTrayIconMode = DynamicTrayIconModes.BatteryAvg,
            PopupEnabled = true,
            PopupCompact = true,
            PopupPlacement = PopupPlacement.TopLeft,
            ExperimentsDisabled = true,
            DisableCrashReporting = true,
            OpenDevToolsOnStartup = true,
            CollectBatteryHistory = false,
            FirmwareWarningAccepted = true,
            BixbyRemapEvent = Event.AncToggle,
            ResumePlaybackOnSensor = true,
            PausePlaybackOnSensor = true,
            IsUsageReportHintHidden = true,
            IsBatteryHistoryHintHidden = true
        };

        original.Devices.Add(new Device
        {
            Model = Models.Buds3Pro,
            MacAddress = "11:22:33:44:55:66",
            Name = "Test Buds",
            DeviceColor = DeviceIds.Buds3ProWhite,
            CustomEqualizerEnabled = true,
            CustomEqualizerBands = [1, 2, 3, 4, 5],
            CustomEqualizerActiveSlot = 1,
            CustomEqualizerSlots = [[10, 20], [30, 40]]
        });

        original.Hotkeys.Add(new Hotkey
        {
            Modifier = [ModifierKeys.Control],
            Keys = [Keys.A],
            Action = Event.TogglePlayPause
        });

        original.CustomActionLeft.Action = CustomActions.RunExternalProgram;
        original.CustomActionLeft.Parameter = "/tmp/test-program";
        original.CustomActionRight.Action = CustomActions.Event;
        original.CustomActionRight.Parameter = nameof(Event.AncToggle);
        original.ExperimentsFinishedIds.Add(123456789);

        var json = JsonSerializer.Serialize(original, SettingsSerializerContext.Default.SettingsData);
        var result = JsonSerializer.Deserialize(json, SettingsSerializerContext.Default.SettingsData);

        result.Should().NotBeNull();
        result!.Theme.Should().Be(Themes.Dark);
        result.AccentColor.Should().Be(0xFF123456);
        result.BlurStrength.Should().Be(42);
        result.Locale.Should().Be(Locales.de);
        result.TemperatureUnit.Should().Be(TemperatureUnits.Fahrenheit);
        result.RealisticEarbudImages.Should().BeFalse();
        result.ShowSidebar.Should().BeFalse();
        result.ColorOverride.Should().Be(DeviceIds.Buds3ProSilver);
        result.UseBluetoothWinRt.Should().BeFalse();
        result.LastDeviceMac.Should().Be("AA:BB:CC:DD:EE:FF");
        result.AutoEnableMultipoint.Should().BeTrue();
        result.MinimizeToTray.Should().BeFalse();
        result.DynamicTrayIconMode.Should().Be(DynamicTrayIconModes.BatteryAvg);
        result.PopupEnabled.Should().BeTrue();
        result.PopupCompact.Should().BeTrue();
        result.PopupPlacement.Should().Be(PopupPlacement.TopLeft);
        result.ExperimentsDisabled.Should().BeTrue();
        result.DisableCrashReporting.Should().BeTrue();
        result.OpenDevToolsOnStartup.Should().BeTrue();
        result.CollectBatteryHistory.Should().BeFalse();
        result.FirmwareWarningAccepted.Should().BeTrue();
        result.BixbyRemapEvent.Should().Be(Event.AncToggle);
        result.ResumePlaybackOnSensor.Should().BeTrue();
        result.PausePlaybackOnSensor.Should().BeTrue();
        result.IsUsageReportHintHidden.Should().BeTrue();
        result.IsBatteryHistoryHintHidden.Should().BeTrue();

        result.CustomActionLeft.Action.Should().Be(CustomActions.RunExternalProgram);
        result.CustomActionLeft.Parameter.Should().Be("/tmp/test-program");
        result.CustomActionRight.Action.Should().Be(CustomActions.Event);
        result.CustomActionRight.Parameter.Should().Be(nameof(Event.AncToggle));
        result.ExperimentsFinishedIds.Should().Equal(123456789);

        var device = result.Devices.Should().ContainSingle().Subject;
        device.Model.Should().Be(Models.Buds3Pro);
        device.MacAddress.Should().Be("11:22:33:44:55:66");
        device.Name.Should().Be("Test Buds");
        device.DeviceColor.Should().Be(DeviceIds.Buds3ProWhite);
        device.CustomEqualizerEnabled.Should().BeTrue();
        device.CustomEqualizerBands.Should().Equal(1, 2, 3, 4, 5);
        device.CustomEqualizerActiveSlot.Should().Be(1);
        device.CustomEqualizerSlots.Should().HaveCount(2);
        device.CustomEqualizerSlots![0].Should().Equal(10, 20);
        device.CustomEqualizerSlots[1].Should().Equal(30, 40);

        var hotkey = result.Hotkeys.Should().ContainSingle().Subject;
        hotkey.Modifier.Should().Equal(ModifierKeys.Control);
        hotkey.Keys.Should().Equal(Keys.A);
        hotkey.Action.Should().Be(Event.TogglePlayPause);

        json.Should().Contain("\"theme\": \"Dark\"");
    }
}
