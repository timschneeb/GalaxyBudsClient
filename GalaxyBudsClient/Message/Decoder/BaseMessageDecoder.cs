namespace GalaxyBudsClient.Message.Decoder;

public abstract class BaseMessageDecoder : BaseMessageHandler
{
    protected BaseMessageDecoder(SppMessage msg)
    {
        TargetModel = msg.TargetModel;
    }
}