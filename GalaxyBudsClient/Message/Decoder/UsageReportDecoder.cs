using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaxyBudsClient.Generated.Model.Attributes;
using GalaxyBudsClient.Utils;
using Serilog;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.USAGE_REPORT)]
public class UsageReportDecoder : BaseMessageDecoder
{
    private static readonly Dictionary<string, string> KeyLookup = new()
    {
        { "PLAY", "SingleTap/Play" },
        { "PAUS", "SingleTap/Pause" },
        { "NEXT", "DoubleTap/Next" },
        { "PREV", "DoubleTap/Prev" },
        { "AMBD", "AmbientSound/Duration" },
        { "AMBC", "AmbientSound/Count" },
        { "A2DD", "MusicFromPhone/Duration" },
        { "A2DC", "MusicFromPhone/Count" },
        { "WEAD", "Wearing/Duration" },
        { "WEAC", "Wearing/Count" },

        { "LOWB", "LowBattery" },
        { "LOWB0", "LowBattery/0" },
        { "LOWB1", "LowBattery/1" },
        { "LOWB2", "LowBattery/2" },
        { "LOWB3", "LowBattery/3" },
        { "DISC", "Error/Discharging" },
        { "CHAR", "Error/Charging" },
        { "EAST", "AssertCPUException" },

        { "TAHL0", "TapAndHold/Left0" },
        { "TAHL1", "TapAndHold/Left1" },
        { "TAHL2", "TapAndHold/Left2" },
        { "TAHL3", "TapAndHold/Left3" },
        { "TAHL4", "TapAndHold/Left4" },
        { "TAHL5", "TapAndHold/Left5" },
        { "TAHL6", "TapAndHold/Left6" },
        { "TAHL7", "TapAndHold/Left7" },
        { "TAHL8", "TapAndHold/Left8" },
        { "TAHR0", "TapAndHold/Right0" },
        { "TAHR1", "TapAndHold/Right1" },
        { "TAHR2", "TapAndHold/Right2" },
        { "TAHR3", "TapAndHold/Right3" },
        { "TAHR4", "TapAndHold/Right4" },
        { "TAHR5", "TapAndHold/Right5" },
        { "TAHR6", "TapAndHold/Right6" },
        { "TAHR7", "TapAndHold/Right7" },
        { "TAHR8", "TapAndHold/Right8" },

        { "LRLP", "LongPress/LeftRight" },
            
        { "ANSC", "Earbuds/DoubleTap" },
        { "ENDC", "Earbuds/DoubleTap" },
        { "WLOD", "Earbuds/WearingDurationLeft" },
        { "WROD", "Earbuds/WearingDurationRight" },
        { "WALD", "Earbuds/WearingDuration" },
        { "MDSW", "Earbuds/MasterSwitchingForEachUse" },
        { "DCIC", "Earbuds/DisconnectionWhileIncomingCall" },
        { "DCDC", "Earbuds/DisconnectionDuringCall" },
        { "CWFD", "Earbuds/DurationThatEarbudsConnectedBut" },
        { "ANCD", "Earbuds/ActiveNoiseCancelingDuration" },
        { "BSCR", "RightEarbud/ChargingCycle" },
        { "BSCL", "LeftEarbud/ChargingCycle" },
        { "WDCL", "Charge/WiredCountLeft" },
        { "WDCR", "Charge/WiredCountRight" },
        { "WLCL", "Charge/WirelessCountLeft" },
        { "WLCR", "Charge/WirelessCountRight" },
        { "PSCL", "Charge/PowerShareCountLeft" },
        { "PSCR", "Charge/PowerShareCountRight" },
        { "WDTL", "Charge/WiredTimeLeft" },
        { "WDTR", "Charge/WiredTimeRight" },
        { "WLTL", "Charge/WirelessTimeLeft" },
        { "WLTR", "Charge/WirelessTimeRight" },
        { "PSTL", "Charge/PowerShareTimeLeft" },
        { "PSTR", "Charge/PowerShareTimeRight" },
        { "FBDA", "WearBoth/NoiseControlOff" },
        { "FBDB", "WearBoth/NoiseControlOff" },
        { "FBDC", "WearBoth/NoiseControlOff" },
        { "FLDA", "WearLeft/NoiseControlOff" },
        { "FLDB", "WearLeft/NoiseControlOff" },
        { "FLDC", "WearLeft/NoiseControlOff" },
        { "FRDA", "WearRight/NoiseControlOff" },
        { "FRDB", "WearRight/NoiseControlOff" },
        { "FRDC", "WearRight/NoiseControlOff" },
        { "NBDA", "WearBoth/ANC" },
        { "NBDB", "WearBoth/ANC" },
        { "NBDC", "WearBoth/ANC" },
        { "ABDA", "WearBoth/AmbientSound" },
        { "ABDB", "WearBoth/AmbientSound" },
        { "ABDC", "WearBoth/AmbientSound" }
    };
    
    public Dictionary<string, long>? Statistics { get; }
    
    public UsageReportDecoder(SppMessage msg) : base(msg)
    {
        if (Statistics == null)
            Statistics = new Dictionary<string, long>();
        else
            Statistics.Clear();

        const int lengthKey = 5;
        const int lengthValue = 4;
        const int lengthItem = lengthKey + lengthValue;

        var itemCount = msg.Payload[0];

        if (msg.Size != itemCount * lengthItem + 4)
        {
            Log.Warning("UsageReport -> Invalid buffer length");
            return;
        }

        for (var i = 1; i < itemCount * lengthItem; i += lengthItem)
        {
            var rawKey = new byte[lengthKey];
            var rawValue = new byte[lengthValue];
            Array.Copy(msg.Payload, i, rawKey, 0, lengthKey);
            Array.Copy(msg.Payload, i + lengthKey, rawValue, 0, lengthValue);

            var key = Encoding.ASCII.GetString(rawKey.RTrimBytes());
            long value = BitConverter.ToUInt32(rawValue, 0);
            Statistics.Add(key, value);
        }
    }
    
    public List<Usage2ReportDecoder.LoggingItem> ToLoggingItems()
    {
        var loggingItems = new List<Usage2ReportDecoder.LoggingItem>();
        if (Statistics == null)
            return loggingItems;
        
        var eventDict = new Dictionary<string, (string id, char? detailChar)>
        {
            { "A2DD", ("9009", null) },
            { "A2DC", ("9010", null) },
            { "LOWB", ("9013", null) },
            { "LOWB0", ("9013", null) },
            { "LOWB1", ("9013", null) },
            { "LOWB2", ("9013", null) },
            { "LOWB3", ("9013", null) },
            { "DISC", ("9014", null) },
            { "CHAR", ("9015", null) },
            { "EAST", ("9016", null) },
            { "LRLP", ("9006", null) },
            { "MDSW", ("9022", null) },
            { "DCIC", ("9023", null) },
            { "DCDC", ("9024", null) },
            { "CWFD", ("9025", null) },
            { "ANCD", ("9019", null) },
            { "ABDA", ("EB0007", 'a') },
            { "ABDB", ("EB0007", 'b') },
            { "ABDC", ("EB0007", 'c') },
            { "ANSC", ("9002", 'b') },
            { "ENDC", ("9002", 'c') },
            { "FBDA", ("EB0001", 'a') },
            { "FBDB", ("EB0001", 'b') },
            { "FBDC", ("EB0001", 'c') },
            { "FLDA", ("EB0002", 'a') },
            { "FLDB", ("EB0002", 'b') },
            { "FLDC", ("EB0002", 'c') },
            { "FRDA", ("EB0003", 'a') },
            { "FRDB", ("EB0003", 'b') },
            { "FRDC", ("EB0003", 'c') },
            { "NBDA", ("EB0004", 'a') },
            { "NBDB", ("EB0004", 'b') },
            { "NBDC", ("EB0004", 'c') },
            { "NEXT", ("9002", 'a') },
            { "PAUS", ("9001", 'b') },
            { "PLAY", ("9001", 'a') },
            { "PREV", ("9003", null) },
            { "WALD", ("9011", 'c') },
            { "WLOD", ("9011", 'a') },
            { "WROD", ("9011", 'b') },
            { "TAHL0", ("9004", 'a') },
            { "TAHL1", ("9004", 'b') },
            { "TAHL2", ("9004", 'c') },
            { "TAHL3", ("9004", 'd') },
            { "TAHL4", ("9004", 'e') },
            { "TAHL5", ("9004", 'f') },
            { "TAHL6", ("9004", 'g') },
            { "TAHL7", ("9004", 'h') },
            { "TAHL8", ("9004", 'i') },
            { "TAHR0", ("9005", 'a') },
            { "TAHR1", ("9005", 'b') },
            { "TAHR2", ("9005", 'c') },
            { "TAHR3", ("9005", 'd') },
            { "TAHR4", ("9005", 'e') },
            { "TAHR5", ("9005", 'f') },
            { "TAHR6", ("9005", 'g') },
            { "TAHR7", ("9005", 'h') },
            { "TAHR8", ("9005", 'i') }
        };

        var statusDict = new Dictionary<string, string>
        {
            { "BSCL", "9033" },
            { "BSCR", "9032" },
            { "PSCL", "EB0010" },
            { "PSCR", "EB0011" },
            { "PSTL", "EB0016" },
            { "PSTR", "EB0017" },
            { "WDCL", "EB0012" },
            { "WDCR", "EB0013" },
            { "WDTL", "EB0018" },
            { "WDTR", "EB0019" },
            { "WLCL", "EB0008" },
            { "WLCR", "EB0009" },
            { "WLTL", "EB0014" },
            { "WLTR", "EB0015" }
        };

        
        foreach (var (key, value) in Statistics)
        {
            if (eventDict.TryGetValue(key, out var eventValue))
            {
                loggingItems.AddLogOrUpdateDetail(eventValue.id, Usage2ReportDecoder.LoggingType.Event, eventValue.detailChar, value);
            }
            else if (statusDict.TryGetValue(key, out var statusValue))
            {
                loggingItems.AddLogOrUpdateDetail(statusValue, Usage2ReportDecoder.LoggingType.Status, null, value);
            }
        }

        return loggingItems;
    }
    
    public override Dictionary<string, string> ToStringMap()
    {
        Dictionary<string, string> map = new();
        if (Statistics == null)
            return map;
        
        foreach (var statistic in Statistics)
        {
            var readableKey = $"{(KeyLookup.TryGetValue(statistic.Key, out var value) ? value : "Unknown")} ({statistic.Key})";
            map.Add(readableKey, statistic.Value.ToString());
        }
        return map;
    }
}

file static class Extensions
{
    public static void AddLogOrUpdateDetail(this ICollection<Usage2ReportDecoder.LoggingItem> loggingItems, string id, Usage2ReportDecoder.LoggingType type, char? detail, long value)
    {
        var item = loggingItems.FirstOrDefault(x => x.Id == id);
        if (item == null)
        {
            item = new Usage2ReportDecoder.LoggingItem
            {
                Id = id,
                Type = type,
                ValueItems = []
            };
            loggingItems.Add(item);
        }

        item.ValueItems.Add(new Usage2ReportDecoder.ValueItem
        {
            Detail = detail ?? '\0',
            Value = value
        });
    }
}