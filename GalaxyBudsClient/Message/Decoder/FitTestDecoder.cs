using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Generated.Model.Attributes;
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

    [CompiledEnum]
    public enum Result
    {
        [LocalizableDescription(key: Keys.GftBad)]
        Bad = 0,
        [LocalizableDescription(Keys.GftGood)]
        Good = 1,
        [LocalizableDescription(Keys.GftFail)]
        TestFailed = 2
    }
}