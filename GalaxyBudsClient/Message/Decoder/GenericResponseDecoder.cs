using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.RESP)]
public class GenericResponseDecoder : BaseMessageDecoder
{
    public MsgIds MessageId { set; get; }
    public int ResultCode { set; get; }
    public int? ExtraData { set; get; }

    public override void Decode(SppMessage msg)
    {
        MessageId = (MsgIds) msg.Payload[0];
        ResultCode = msg.Payload[1];
        if (msg.Payload.Length > 2)
        {
            ExtraData = msg.Payload[2];
        }
    }
}