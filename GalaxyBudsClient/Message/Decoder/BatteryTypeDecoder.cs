using System;
using System.Text;
using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.BATTERY_TYPE)]
public class BatteryTypeDecoder : BaseMessageDecoder
{
    public string? LeftBatteryType { set; get; }
    public string? RightBatteryType { set; get; }

    public override void Decode(SppMessage msg)
    {
        var left = new byte[2];
        var right = new byte[2];
        Array.Copy(msg.Payload, 0, left, 0, 2);
        Array.Copy(msg.Payload, 2, right, 0, 2);
        LeftBatteryType = Encoding.ASCII.GetString(left);
        RightBatteryType = Encoding.ASCII.GetString(right);
    }
}