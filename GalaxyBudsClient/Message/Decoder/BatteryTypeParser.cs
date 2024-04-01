using System;
using System.Text;

namespace GalaxyBudsClient.Message.Decoder;

public class BatteryTypeParser : BaseMessageParser
{
    public override MsgIds HandledType => MsgIds.BATTERY_TYPE;

    public string? LeftBatteryType { set; get; }
    public string? RightBatteryType { set; get; }

    public override void ParseMessage(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        var left = new byte[2];
        var right = new byte[2];
        Array.Copy(msg.Payload, 0, left, 0, 2);
        Array.Copy(msg.Payload, 2, right, 0, 2);
        LeftBatteryType = Encoding.ASCII.GetString(left);
        RightBatteryType = Encoding.ASCII.GetString(right);
    }
}