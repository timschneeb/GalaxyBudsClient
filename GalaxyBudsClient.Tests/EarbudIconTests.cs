using FluentAssertions;
using GalaxyBudsClient.Interface.Controls;
using GalaxyBudsClient.Model.Constants;
using NUnit.Framework;

namespace GalaxyBudsClient.Tests;

[TestFixture]
public class EarbudIconTests
{
    [TestCase(DeviceIds.BudsProBlack, DeviceIds.Buds4ProWhite, Models.Buds4Pro, DeviceIds.Buds4ProWhite)]
    [TestCase(DeviceIds.Buds4ProWhite, DeviceIds.BudsProBlack, Models.BudsPro, DeviceIds.BudsProBlack)]
    [TestCase(DeviceIds.Buds4ProApricot, DeviceIds.Buds4ProWhite, Models.Buds4Pro, DeviceIds.Buds4ProApricot)]
    [TestCase(null, DeviceIds.BudsProBlack, Models.BudsPro, DeviceIds.BudsProBlack)]
    [TestCase(DeviceIds.Unknown, DeviceIds.BudsProBlack, Models.BudsPro, DeviceIds.BudsProBlack)]
    [TestCase(DeviceIds.BudsProBlack, null, Models.Buds4Pro, null)]
    [TestCase(null, null, Models.NULL, null)]
    [TestCase(DeviceIds.Unknown, null, Models.NULL, null)]
    public void ResolveColor_OnlyUsesOverridesForTheCurrentModel(
        DeviceIds? colorOverride, DeviceIds? deviceColor, Models model, DeviceIds? expected)
    {
        EarbudIcon.ResolveColor(colorOverride, deviceColor, model).Should().Be(expected);
    }
}
