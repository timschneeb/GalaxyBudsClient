namespace GalaxyBudsClient.Message.Decoder;

public abstract class BaseMessageDecoder : BaseMessageHandler
{
    public abstract void Decode(SppMessage msg);
}