namespace GalaxyBudsClient.Generators.Messages;

public readonly record struct HandlerToGenerate(
    string FullyQualifiedName,
    string MessageId,
    HandlerType Type)
{
    public readonly string FullyQualifiedName = FullyQualifiedName;
    public readonly string MessageId = MessageId;
    public readonly HandlerType Type = Type;
}

public enum HandlerType
{
    Decoder,
    Encoder
}