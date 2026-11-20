using System.Globalization;
using FluentAssertions;
using FluentIcons.Avalonia.Fluent;
using FluentIcons.Common;
using GalaxyBudsClient.Interface.Converters;
using GalaxyBudsClient.Model.Config;

namespace GalaxyBudsClient.Tests;

[TestFixture]
public class DeviceSelectStateConverterTests
{
    private readonly DeviceSelectStateConverter _converter = new();

    [Test]
    public void IconTarget_ReturnsNonNullFallback_WhenDeviceIsNull()
    {
        var result = Convert([null, "AA:BB:CC:DD:EE:FF"], DeviceStateConverterTarget.Icon);

        result.Should().BeOfType<SymbolIconSource>();
    }

    [Test]
    public void IconTarget_ReturnsNonNullFallback_WhenValueIsNotADevice()
    {
        var result = Convert([new object(), "AA:BB:CC:DD:EE:FF"], DeviceStateConverterTarget.Icon);

        result.Should().BeOfType<SymbolIconSource>();
    }

    [Test]
    public void IconTarget_ReturnsCheckedIcon_WhenDeviceIsSelected()
    {
        var device = new Device { MacAddress = "AA:BB:CC:DD:EE:FF" };

        var result = Convert([device, device.MacAddress], DeviceStateConverterTarget.Icon);

        var icon = result.Should().BeOfType<SymbolIconSource>().Subject;
        icon.Symbol.Should().Be(Symbol.CheckboxChecked);
        icon.IconVariant.Should().Be(IconVariant.Filled);
    }

    private object? Convert(object?[] values, DeviceStateConverterTarget target)
    {
        return _converter.Convert(values, typeof(object), target, CultureInfo.InvariantCulture);
    }
}
