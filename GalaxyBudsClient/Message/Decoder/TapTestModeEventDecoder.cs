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

[MessageDecoder(MsgIds.TAP_TEST_MODE_EVENT)]
public class TapTestModeEventDecoder : BaseMessageDecoder
{
    public TapTestResult Result { set; get; }
    public bool IsLeft { set; get; }
    public bool IsRight { set; get; }

    public override void Decode(SppMessage msg)
    {
        using var stream = new MemoryStream(msg.Payload);
        using var reader = new BinaryReader(stream);

        Result = (TapTestResult)reader.ReadByte();
        var side = reader.ReadByte();
        IsLeft = ByteArrayUtils.ValueOfLeft(side) == 1;
        IsRight = ByteArrayUtils.ValueOfRight(side) == 1;
    }
}