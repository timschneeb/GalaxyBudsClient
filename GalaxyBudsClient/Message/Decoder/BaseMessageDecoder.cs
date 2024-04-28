namespace GalaxyBudsClient.Message.Decoder;

public abstract class BaseMessageDecoder : BaseMessageHandler
{
    public BaseMessageDecoder(SppMessage msg)
    {
        TargetModel = msg.TargetModel;
    }
};