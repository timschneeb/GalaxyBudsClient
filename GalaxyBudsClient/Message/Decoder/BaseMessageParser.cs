namespace GalaxyBudsClient.Message.Decoder;

public abstract class BaseMessageParser : BaseMessageHandler
{
    public abstract void ParseMessage(SppMessage msg);
}