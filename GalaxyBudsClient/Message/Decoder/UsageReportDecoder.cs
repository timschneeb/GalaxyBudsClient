using System;
using System.Collections.Generic;
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
            long value = BitConverter.ToInt32(rawValue, 0);
            Statistics.Add(key, value);
        }
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