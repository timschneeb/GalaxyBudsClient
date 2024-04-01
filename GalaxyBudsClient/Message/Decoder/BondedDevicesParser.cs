using System.Text;

namespace GalaxyBudsClient.Message.Decoder;

internal class BondedDevicesParser : BaseMessageParser
{
    public override MsgIds HandledType => MsgIds.UNK_BONDED_DEVICES;

    public string? Content { set; get; }

    public override void ParseMessage(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        Content = Encoding.ASCII.GetString(msg.Payload);
    }
}