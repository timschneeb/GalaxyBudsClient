using System.Collections.Generic;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Decoder;

internal class DebugModeVersionDecoder : BaseMessageDecoder
{
    public override MsgIds HandledType => MsgIds.VERSION_INFO;

    private readonly string[] _swMonth = ["A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L"];
    private readonly string[] _swRelVer = ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"];
    private readonly string[] _swYear = ["O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"];

    public string? LeftHardwareVersion { set; get; }
    public string? RightHardwareVersion { set; get; }
    public string? LeftSoftwareVersion { set; get; }
    public string? RightSoftwareVersion { set; get; }
    public string? LeftTouchSoftwareVersion { set; get; }
    public string? RightTouchSoftwareVersion { set; get; }

    public override void Decode(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        var l1 = (msg.Payload[0] & 240) >> 4;
        var l2 = msg.Payload[0] & 15;
        var r1 = (msg.Payload[1] & 240) >> 4;
        var r2 = msg.Payload[1] & 15;

        LeftHardwareVersion = "rev" + l1.ToString("X") + "." + l2.ToString("X");
        RightHardwareVersion = "rev" + r1.ToString("X") + "." + r2.ToString("X");

        LeftSoftwareVersion = VersionDataToString(msg.Payload, 2, "L");
        RightSoftwareVersion = VersionDataToString(msg.Payload, 5, "R");

        LeftTouchSoftwareVersion = msg.Payload[8].ToString("x");
        RightTouchSoftwareVersion = msg.Payload[9].ToString("x");
    }

    private string VersionDataToString(IReadOnlyList<byte> payload, int startIndex, string side)
    {
        var swVar = payload[startIndex] == 0 ? "E" : "U";
        var swYearIndex = (payload[startIndex + 1] & 240) >> 4;
        var swMonthIndex = payload[startIndex + 1] & 15;
        var swRelVerIndex = payload[startIndex + 2];
        var swRelVarString = swRelVerIndex < _swRelVer.Length ? _swRelVer[swRelVerIndex] : string.Empty;
        
        if (TargetModel == Models.Buds)
        {
            swRelVarString = swRelVerIndex <= 15 ? 
                (swRelVerIndex & 255).ToString("X") : _swRelVer[swRelVerIndex - 16];
        }
        
        var pre = TargetModel.GetModelMetadata()?.BuildPrefix.TrimStart('R') ?? "???";
        return side + pre + "XX" + swVar + "0A" + _swYear[swYearIndex] + _swMonth[swMonthIndex] + swRelVarString;
    }
}