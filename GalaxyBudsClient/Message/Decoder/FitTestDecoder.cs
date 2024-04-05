using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

public class FitTestDecoder : BaseMessageDecoder
{
    public override MsgIds HandledType => MsgIds.CHECK_THE_FIT_OF_EARBUDS_RESULT;

    public Result Left { private set; get; }
    public Result Right { private set; get; }

    public override void Decode(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        Left = (Result)msg.Payload[0];
        Right = (Result)msg.Payload[1];
    }

    public enum Result
    {
        [LocalizedDescription(Keys.GftBad)]
        Bad = 0,
        [LocalizedDescription(Keys.GftGood)]
        Good = 1,
        [LocalizedDescription(Keys.GftFail)]
        TestFailed = 2
    }
}