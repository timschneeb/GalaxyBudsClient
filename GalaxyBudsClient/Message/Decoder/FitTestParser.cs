using GalaxyBudsClient.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

public class FitTestParser : BaseMessageParser
{
    public override SppMessage.MessageIds HandledType => SppMessage.MessageIds.CHECK_THE_FIT_OF_EARBUDS_RESULT;

    public Result Left;
    public Result Right;

    public override void ParseMessage(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        Left = (Result)msg.Payload[0];
        Right = (Result)msg.Payload[1];
    }

    public enum Result : byte
    {
        [LocalizedDescription("gft_bad")]
        Bad = 0,
        [LocalizedDescription("gft_good")]
        Good = 1,
        [LocalizedDescription("gft_fail")]
        TestFailed = 2
    }
}