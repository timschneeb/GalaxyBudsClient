namespace GalaxyBudsClient.Message.Parameter;

public class SimpleAckParameter : MessageAsDictionary, IAckParameter
{
    public byte Value { get; init; }
}