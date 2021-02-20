using System;
using System.Collections.Generic;
using GalaxyBudsClient.Utils;
using Serilog;

namespace GalaxyBudsClient.Message.Decoder
{
    public class UsageReportParser : BaseMessageParser
    {
        static readonly Dictionary<string, string> KeyLookup = new Dictionary<string, string>
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
            { "TAHR0", "TapAndHold/Right0" },
            { "TAHR1", "TapAndHold/Right1" },
            { "TAHR2", "TapAndHold/Right2" },
            { "TAHR3", "TapAndHold/Right3" },
            { "TAHR4", "TapAndHold/Right4" },
            { "TAHR5", "TapAndHold/Right5" },
            { "TAHR6", "TapAndHold/Right6" },
            { "TAHR7", "TapAndHold/Right7" },

            { "LRLP", "LongPress/LeftRight" },
        };

        public override SPPMessage.MessageIds HandledType => SPPMessage.MessageIds.USAGE_REPORT;

        public Dictionary<String, long>? Statistics { set; get; }

        public override void ParseMessage(SPPMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            if(ActiveModel == Model.Constants.Models.BudsLive)
            {
                /* Buds Live not supported right now */
                return;
            }

            if (Statistics == null)
                Statistics = new Dictionary<string, long>();
            else
                Statistics.Clear();

            const int lengthKey = 5;
            const int lengthValue = 4;
            const int lengthItem = lengthKey + lengthValue;

            byte itemCount = msg.Payload[0];

            if (msg.Size != (itemCount * lengthItem) + 4)
            {
                Log.Warning("UsageReport -> Invalid buffer length");
                return;
            }

            for (int i = 1; i < (itemCount * lengthItem); i += lengthItem)
            {
                byte[] rawKey = new byte[lengthKey];
                byte[] rawValue = new byte[lengthValue];
                Array.Copy(msg.Payload, i, rawKey, 0, lengthKey);
                Array.Copy(msg.Payload, i + lengthKey, rawValue, 0, lengthValue);

                String key = System.Text.Encoding.ASCII.GetString(rawKey.RTrimBytes());
                long value = BitConverter.ToInt32(rawValue, 0);
                Statistics.Add(key, value);
            }
        }

        public override Dictionary<String, String> ToStringMap()
        {
            Dictionary<String, String> map = new Dictionary<string, string>();
            if (Statistics != null)
            {
                foreach (var statistic in Statistics)
                {
                    String readableKey;
                    if (KeyLookup.ContainsKey(statistic.Key))
                        readableKey = $"{KeyLookup[statistic.Key]} ({statistic.Key})";
                    else
                        readableKey = $"Unknown ({statistic.Key})";

                    map.Add(readableKey, statistic.Value.ToString());
                }
            }

            return map;
        }
    }
}
