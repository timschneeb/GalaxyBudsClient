using System;
using System.Text;
using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.DEBUG_SERIAL_NUMBER)]
public class DebugSerialNumberDecoder : BaseMessageDecoder
{
    public string? LeftSerialNumber { set; get; }
    public string? RightSerialNumber { set; get; }

    public override void Decode(SppMessage msg)
    {
        var left = new byte[11];
        var right = new byte[11];
        Array.Copy(msg.Payload, 0, left, 0, 11);
        Array.Copy(msg.Payload, 11, right, 0, 11);
        LeftSerialNumber = Encoding.ASCII.GetString(left, 0, 11);
        RightSerialNumber = Encoding.ASCII.GetString(right, 0, 11);
    }
}