using FluentAssertions;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Message.Encoder;
using GalaxyBudsClient.Message.Parameter;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Tests.Buds4Pro;

[TestFixture]
public class LockTouchpadTests
{
    [Test]
    public void Encoder_UsesOfficialBuds4Layout()
    {
        var message = new LockTouchpadEncoder
        {
            TargetModel = Models.Buds4Pro,
            TapOn = true,
            DoubleTapOn = false,
            TripleTapOn = true,
            HoldTapOn = false,
            DoubleTapCallOn = true,
            HoldTapCallOn = false,
            SwipeOn = true,
            Lighting = 2,
            QuickLaunchAdvanced = 3
        }.Encode();

        // Reserved, master, single, double, triple, hold, call double,
        // call hold, swipe, duplicate call double, lighting, quick launch
        message.Payload.Should().Equal(0, 1, 1, 0, 1, 0, 1, 0, 1, 1, 2, 3);
    }

    [Test]
    public void Acknowledgement_ReadsOfficialTenByteLayout()
    {
        var message = new SppMessage(MsgIds.UNIVERSAL_MSG_ID_ACKNOWLEDGEMENT, MsgTypes.Response,
        [
            (byte)MsgIds.LOCK_TOUCHPAD,
            1, // master
            1, // single
            0, // double
            0, // triple
            1, // hold
            1, // call double
            0, // call hold
            1, // swipe
            2, // lighting
            3  // quick launch
        ], Models.Buds4Pro);

        var decoder = (AcknowledgementDecoder)message.CreateDecoder()!;
        var ack = decoder.Parameters.Should().BeOfType<LockTouchpadAckParameter>().Subject;

        ack.TapOn.Should().BeTrue();
        ack.DoubleTapOn.Should().BeFalse();
        ack.TripleTapOn.Should().BeFalse();
        ack.TouchAndHoldOn.Should().BeTrue();
        ack.ForCallDoubleTap.Should().BeTrue();
        ack.ForCallTouchAndHold.Should().BeFalse();
        ack.SwipeOn.Should().BeTrue();
        ack.Lighting.Should().Be(2);
        ack.QuickLaunchAdvanced.Should().Be(3);
        ack.SupportsAdvancedTouchLock.Should().BeTrue();
    }
}
