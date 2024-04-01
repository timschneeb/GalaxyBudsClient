using System;
using System.Text;

namespace GalaxyBudsClient.Message.Decoder;

public class DebugSerialNumberParser : BaseMessageParser
{
    public override MsgIds HandledType => MsgIds.DEBUG_SERIAL_NUMBER;

    public string? LeftSerialNumber { set; get; }
    public string? RightSerialNumber { set; get; }

    public override void ParseMessage(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        var left = new byte[11];
        var right = new byte[11];
        Array.Copy(msg.Payload, 0, left, 0, 11);
        Array.Copy(msg.Payload, 11, right, 0, 11);
        LeftSerialNumber = Encoding.ASCII.GetString(left, 0, 11);
        RightSerialNumber = Encoding.ASCII.GetString(right, 0, 11);
    }
}