using FluentAssertions;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Tests;

[TestFixture]
public class SetOtherOptionTests
{
    // Captured from pinch-and-hold gestures on Galaxy Buds3 Pro.
    [TestCase(1, TouchOptions.OtherL)]
    [TestCase(0, TouchOptions.OtherR)]
    public void Buds3Pro_SideFlag_SelectsCustomAction(byte payload, TouchOptions expected)
    {
        Decode(Models.Buds3Pro, payload).Should().Be(expected.ToString());
    }

    [TestCase(Models.Buds3Pro, 5, TouchOptions.OtherL)]
    [TestCase(Models.Buds3Pro, 6, TouchOptions.OtherR)]
    [TestCase(Models.Buds2Pro, 5, TouchOptions.OtherL)]
    [TestCase(Models.Buds2Pro, 6, TouchOptions.OtherR)]
    [TestCase(Models.Buds2Pro, 1, TouchOptions.VoiceAssistant)]
    [TestCase(Models.Buds, 5, TouchOptions.OtherL)]
    [TestCase(Models.Buds, 6, TouchOptions.OtherR)]
    public void ActionValue_UsesDeviceTouchMap(Models model, byte payload, TouchOptions expected)
    {
        Decode(model, payload).Should().Be(expected.ToString());
    }

    private static string Decode(Models model, byte payload)
    {
        var message = new SppMessage(MsgIds.SET_TOUCHPAD_OTHER_OPTION,
            MsgTypes.Response, [payload], model);
        var decoder = message.CreateDecoder();
        decoder.Should().NotBeNull();
        return decoder!.ToStringMap()["OptionType"];
    }
}
