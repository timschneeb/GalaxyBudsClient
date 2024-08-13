using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.RESP)]
public class GenericResponseDecoder : BaseMessageDecoder
{
    public MsgIds MessageId { get; }
    public SdkEarbudResult ResultCode { get; }
    public int? ExtraData { get; }

    public GenericResponseDecoder(SppMessage msg) : base(msg)
    {
        MessageId = (MsgIds) msg.Payload[0];
        ResultCode = (SdkEarbudResult) msg.Payload[1];
        if (msg.Payload.Length > 2)
        {
            ExtraData = msg.Payload[2];
        }
    }
}