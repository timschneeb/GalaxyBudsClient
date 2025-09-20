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
        using var stream = new MemoryStream(msg.Payload);
        using var reader = new BinaryReader(stream);

        // Usar métodos seguros da classe base
        Result = (TapTestResult)SafeReadByte(reader, 0);
        var side = SafeReadByte(reader, 0);
        IsLeft = ByteArrayUtils.ValueOfLeft(side) == 1;
        IsRight = ByteArrayUtils.ValueOfRight(side) == 1;
    }
}