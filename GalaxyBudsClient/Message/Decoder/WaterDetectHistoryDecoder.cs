using System;
using System.Collections.Generic;
using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.READ_WATER_DETECTION_HISTORY)]
public class WaterDetectHistoryDecoder : BaseMessageDecoder
{
    public List<(DateTime? Date, bool Detected)>? WaterDetectionHistory { get; }
    
    public WaterDetectHistoryDecoder(SppMessage msg) : base(msg)
    {
        if(msg.Payload.Length != 180)
            return;
        
        var history = new List<(DateTime? Date, bool Detected)>();
        
        var i = 0;
        while (i < 180)
        {
            var nextIndex = i + 9;
            var rawTime = BitConverter.ToInt64(msg.Payload, i);
            var detected = msg.Payload[i + 8] != 0;
            
            DateTime? date = rawTime != 0 ? new DateTime(1970, 1, 1).AddMilliseconds(rawTime) : null;
            history.Add((date, detected));

            i = nextIndex;
        }

        WaterDetectionHistory = history;
    }

    public override Dictionary<string, string> ToStringMap()
    {
        Dictionary<string, string> map = new();
        if (WaterDetectionHistory == null)
        {
            map["Error"] = "Invalid data";
        }
        else
        {
            for (var i = 0; i < WaterDetectionHistory.Count; i++)
            {
                var item = WaterDetectionHistory[i];
                map[$"Row {i + 1}"] = $"{item.Date?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Unknown date"}: " +
                             $"{(item.Detected ? "Water detected" : "No water detected")}";
            }
        }
        
        return map;
    }
}