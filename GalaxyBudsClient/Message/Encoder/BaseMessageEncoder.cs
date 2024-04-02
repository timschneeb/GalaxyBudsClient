namespace GalaxyBudsClient.Message.Encoder;

public abstract class BaseMessageEncoder : BaseMessageHandler
{
    public abstract SppMessage Encode();
}