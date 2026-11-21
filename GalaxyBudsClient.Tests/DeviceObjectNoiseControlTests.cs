using FluentAssertions;
using GalaxyBudsClient.Cli.Ipc.Objects;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using NUnit.Framework;

namespace GalaxyBudsClient.Tests;

[TestFixture]
public class DeviceObjectNoiseControlTests
{
    [TestCase("NoiseReduction", true, NoiseControlModes.NoiseReduction)]
    [TestCase("ambientsound", true, NoiseControlModes.AmbientSound)]
    [TestCase("OFF", true, NoiseControlModes.Off)]
    [TestCase("Adaptive", true, NoiseControlModes.Adaptive)]
    [TestCase("bogus", false, NoiseControlModes.Off)]
    [TestCase("99", false, NoiseControlModes.Off)]
    public void TryParseNoiseControlMode_ValidatesNames(string value, bool expected, NoiseControlModes expectedMode)
    {
        DeviceObject.TryParseNoiseControlMode(value, out var mode).Should().Be(expected);
        if (expected)
            mode.Should().Be(expectedMode);
    }

    [Test]
    public void IsNoiseControlModeSupported_ModernDeviceSupportsStandardModes()
    {
        var spec = new Buds2ProDeviceSpec();

        DeviceObject.IsNoiseControlModeSupported(NoiseControlModes.Off, spec).Should().BeTrue();
        DeviceObject.IsNoiseControlModeSupported(NoiseControlModes.AmbientSound, spec).Should().BeTrue();
        DeviceObject.IsNoiseControlModeSupported(NoiseControlModes.NoiseReduction, spec).Should().BeTrue();
        DeviceObject.IsNoiseControlModeSupported(NoiseControlModes.Adaptive, spec).Should().BeFalse();
    }

    [Test]
    public void IsNoiseControlModeSupported_LegacyDeviceUsesIndividualFeatures()
    {
        var ambientOnly = new BudsPlusDeviceSpec();
        DeviceObject.IsNoiseControlModeSupported(NoiseControlModes.Off, ambientOnly).Should().BeTrue();
        DeviceObject.IsNoiseControlModeSupported(NoiseControlModes.AmbientSound, ambientOnly).Should().BeTrue();
        DeviceObject.IsNoiseControlModeSupported(NoiseControlModes.NoiseReduction, ambientOnly).Should().BeFalse();
        DeviceObject.IsNoiseControlModeSupported(NoiseControlModes.Adaptive, ambientOnly).Should().BeFalse();

        var ancOnly = new BudsLiveDeviceSpec();
        DeviceObject.IsNoiseControlModeSupported(NoiseControlModes.Off, ancOnly).Should().BeTrue();
        DeviceObject.IsNoiseControlModeSupported(NoiseControlModes.AmbientSound, ancOnly).Should().BeFalse();
        DeviceObject.IsNoiseControlModeSupported(NoiseControlModes.NoiseReduction, ancOnly).Should().BeTrue();
        DeviceObject.IsNoiseControlModeSupported(NoiseControlModes.Adaptive, ancOnly).Should().BeFalse();
    }

    [Test]
    public void IsNoiseControlModeSupported_StubDeviceSupportsNothing()
    {
        var spec = new StubDeviceSpec();

        foreach (var mode in Enum.GetValues<NoiseControlModes>())
            DeviceObject.IsNoiseControlModeSupported(mode, spec).Should().BeFalse();
    }

    [Test]
    public void GetEffectiveAmbientSoundVolumeMax_IncludesExtraLoudWhenEnabled()
    {
        DeviceObject.GetEffectiveAmbientSoundVolumeMax(new BudsPlusDeviceSpec(), false).Should().Be(2);
        DeviceObject.GetEffectiveAmbientSoundVolumeMax(new BudsPlusDeviceSpec(), true).Should().Be(3);
    }

    [Test]
    public void GetEffectiveAmbientSoundVolumeMax_IgnoresExtraLoudWhenUnsupported()
    {
        DeviceObject.GetEffectiveAmbientSoundVolumeMax(new Buds4ProDeviceSpec(), true).Should().Be(4);
    }

    [Test]
    public void GetEffectiveAmbientSoundVolumeMax_ReturnsMinusOneWhenVolumeUnsupported()
    {
        // Buds2 Pro/3 Pro expose AmbientExtraLoud but not the separate ambient-volume slider.
        DeviceObject.GetEffectiveAmbientSoundVolumeMax(new Buds2ProDeviceSpec(), true).Should().Be(-1);
        DeviceObject.GetEffectiveAmbientSoundVolumeMax(new StubDeviceSpec(), true).Should().Be(-1);
    }
}
