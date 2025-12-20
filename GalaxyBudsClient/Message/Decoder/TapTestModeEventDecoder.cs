using System.IO;
using GalaxyBudsClient.Generated.Model.Attributes;
using GalaxyBudsClient.Utils;

namespace GalaxyBudsClient.Message.Decoder;

public enum TapTestResult
{
    Success = 0,
    TapHarder = 1,
    WrongSpot = 2,
    OnlyTapOnce = 3
}

[MessageDecoder((MsgIds)LegacyMsgIds.TAP_TEST_MODE_EVENT)]
public class TapTestModeEventDecoder : BaseMessageDecoder
{
    public TapTestResult Result { get; }
    public bool IsLeft { get; }
    public bool IsRight { get; }

    public TapTestModeEventDecoder(SppMessage msg) : base(msg)
    {
        Result = TapTestResult.Success;
        IsLeft = false;
        IsRight = false;

        if (msg.Payload.Length <= 0)
            return;

        Result = (TapTestResult)msg.Payload[0];
        if (msg.Payload.Length <= 1)
            return;

        var side = msg.Payload[1];
        IsLeft = ByteArrayUtils.ValueOfLeft(side) == 1;
        IsRight = ByteArrayUtils.ValueOfRight(side) == 1;
    }
}
