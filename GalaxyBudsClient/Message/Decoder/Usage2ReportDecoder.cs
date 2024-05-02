using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Generated.Model.Attributes;
using Serilog;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.USAGE_REPORT_V2)]
public class Usage2ReportDecoder : BaseMessageDecoder
{
    public enum LoggingType
    {
        Event = 0,
        Status = 1
    }

    public record LoggingItem
    {
        public required string Id { init; get; }
        public required LoggingType Type { init; get; } 
        public required List<ValueItem> ValueItems { init; get; }

        public string FriendlyName => Events.TryGetValue(Id, out var eventDetails) ? eventDetails.Description :
            Statuses.FirstOrDefault(x => x.Key == Id).Value ?? Id;
    }

    public record ValueItem
    {
        public required char Detail { get; init; }
        public required long Value { get; init; }
    }
    
    public string? ErrorReason { get; }
    public List<LoggingItem> Items { get; } = [];

    public Usage2ReportDecoder(SppMessage msg) : base(msg)
    {
        ErrorReason = null;
        
        using var stream = new MemoryStream(msg.Payload);
        using var reader = new BinaryReader(stream);

        var list = new List<LoggingItem>();
        try
        {
            var loggingItemCount = reader.ReadByte();

            for (var i = 0; i < loggingItemCount; i++)
            {
                var idBytes = new byte[8];
                reader.ReadBytes(7).CopyTo(idBytes, 0);
                
                var headerByte = reader.ReadByte();
                var valueSize = (byte)(headerByte & 15);
                var valueItemSize = valueSize + 1;
                var valueItemTotalLength = reader.ReadByte();
                var valueItemCount = valueItemTotalLength / valueItemSize;

                if (valueItemSize * valueItemCount != valueItemTotalLength) {
                    throw new InvalidOperationException("valueItemSize * valueItemCount != valueItemTotalLength");
                }

                var loggingItem = new LoggingItem
                {
                    Id = Encoding.UTF8.GetString(idBytes).Trim().Trim('\0'),
                    Type = (LoggingType)((headerByte & 240) >> 4),
                    ValueItems = new List<ValueItem>(valueItemCount)
                };
                for (var j = 0; j < valueItemCount; j++)
                {
                    
                    loggingItem.ValueItems.Add(new ValueItem
                    {
                        Detail = reader.ReadChar(),
                        Value = valueSize switch
                        {
                            1 => reader.ReadByte(),
                            2 => reader.ReadUInt16(),
                            4 => reader.ReadUInt32(),
                            _ => throw new Exception("Wrong valueSize")
                        }
                    });
                }

                list.Add(loggingItem);
            }
            
            Items = list;
            
            if (reader.BaseStream.Position < reader.BaseStream.Length - 1)
            {
                throw new Exception("Not all data read");
            }
        }
        catch (Exception th)
        {
            Log.Warning(th, "Failed to parse USAGE_REPORT_V2");
            ErrorReason = th.Message;
        }
    }

    public Usage2ReportDecoder(UsageReportDecoder oldUsageReport) : base(
        new SppMessage(model: oldUsageReport.TargetModel))
    {
        Items = oldUsageReport.ToLoggingItems();
    }

    public Usage2ReportDecoder Merge(Usage2ReportDecoder other)
    {
        foreach (var item in Items)
        {
            var otherItem = other.Items.FirstOrDefault(x => x.Id == item.Id);
            if (otherItem == null)
            {
                otherItem = item with { ValueItems = [] };
                other.Items.Add(otherItem);
            }

            foreach (var valueItem in item.ValueItems)
            {
                otherItem.ValueItems.RemoveAll(x => x.Detail == valueItem.Detail);
                otherItem.ValueItems.Add(valueItem);
            }
        }

        return other;
    }
    
    public override Dictionary<string, string> ToStringMap()
    {
        Dictionary<string, string> map = new();
        if (ErrorReason != null)
            map[nameof(ErrorReason)] = ErrorReason;
        
        foreach (var item in Items)
        {
            map[$"{item.FriendlyName} ({item.Type})"] = string.Join(", ", item.ValueItems
                .Select(x => x.Detail == 0 ? x.Value.ToString() : $"{GetDetailDescription(item.Id, x.Detail) ?? x.Detail.ToString()}: {x.Value}"));
            continue;

            string? GetDetailDescription(string id, char detail)
            {
                return (Events.TryGetValue(id, out var value) ? value.Details : null)?
                    .Select(d => ((char? Detail, string Description)?)d)
                    .FirstOrDefault(d => d?.Detail == detail)?
                    .Description;
            }
        }
        return map;
    }

    private static readonly (char? Detail, string Description)[] TapAndHoldDetails = [
        ('a', Strings.TouchoptionVoice),
        ('b', Strings.NcHeader),
        ('c', Strings.TouchoptionVolume),
        ('d', Strings.Anc),
        ('e', string.Format(Strings.OtherX, "A")),
        ('f', string.Format(Strings.OtherX, "B")),
        ('g', string.Format(Strings.OtherX, "C")),
        ('h', string.Format(Strings.OtherX, "D")),
        ('i', string.Format(Strings.OtherX, "E"))
    ];

    // Names & values copied from the official app. Only 90xx and EBxxx are used on the earbuds at the moment.
    private static readonly Dictionary<string, (string Description, (char? Detail, string Description)[] Details)> Events = new()
    {
        { "9001", ("EARBUDS_SINGLE_TAP", [('a', Strings.EventMediaPlay), ('b', Strings.EventMediaPause)]) },
        { "9002", ("EARBUDS_DOUBLE_TAP", [('a', Strings.Next), ('b', string.Format(Strings.OtherX, "A")), ('c', string.Format(Strings.OtherX, "B"))]) }, // Not sure about details
        { "9003", ("EARBUDS_TRIPLE_TAP", [(null, Strings.Prev)]) },
        { "9004", ("EARBUDS_TAP_AND_HOLD_LEFT", TapAndHoldDetails) },
        { "9005", ("EARBUDS_TAP_AND_HOLD_RIGHT", TapAndHoldDetails) },
        { "9006", ("EARBUDS_TAP_AND_HOLD_BOTH", []) },
        { "9007", ("EARBUDS_AMBIENT_SOUND_DURATION", []) },
        { "9008", ("EARBUDS_AMBIENT_SOUND_TIME", []) },
        { "9009", ("EARBUDS_MUSIC_FROM_PHONE_DURATION", []) },
        { "9010", ("EARBUDS_MUSIC_FROM_PHONE_TIMES", []) },
        { "9011", ("EARBUDS_WEARING_DURATION", [('a', Strings.Left), ('b', Strings.Right), ('c', Strings.Both)]) },
        { "9012", ("EARBUDS_WEARING_TIMES", []) },
        { "9013", ("EARBUDS_LOW_BATTERY", []) },
        { "9014", ("EARBUDS_ERROR_DISCHARGING", []) },
        { "9015", ("EARBUDS_ERROR_CHARGING", []) },
        { "9016", ("EARBUDS_ASSERT_CPU_EXCEPTION", []) },
        { "9018", ("TAP_AND_HOLD_OTHERS_APPS", []) },
        { "9019", ("EARBUDS_ACTIVE_NOISE_CANCELING_DURATION", []) },
        { "9021", ("EARBUDS_BATTERY_CONSUMPTION_FOR_EACH_USE", []) },
        { "9022", ("EARBUDS_MASTER_SWITCHING_FOR_EACH_USE", []) },
        { "9023", ("EARBUDS_DISCONNECTION_WHILE_INCOMING_CALL", []) },
        { "9024", ("EARBUDS_DISCONNECTION_DURING_CALL", []) },
        { "9025", ("EARBUDS_DURATION_THAT_EARBUDS_CONNECTED_BUT_UNUSED", []) },
        { "9026", ("DOUBLE_TAP_EARBUD_EDGE", []) },
        { "9027", ("EARBUDS_BIXBY_VOICE_WAKE_UP", []) },
        { "9028", ("NUM_OF_TIMES_SINGLE_EARBUDS_CONNECTION_A_WEEK", []) },
        { "9029", ("NUM_OF_TIMES_DIFFERENCE_BATTERY_EXCEEDED_15_PERCENT_A_WEEK", []) },
        { "9030", ("IN_CASE_OF_ONLY_ONE_EARBUD_A_WEEK", []) },
        // v The details for the IDs below are missing
        { "EB0001", ("WEAR_BOTH_NOISE_CONTROL_OFF", []) },
        { "EB0002", ("WEAR_LEFT_NOISE_CONTROL_OFF", []) },
        { "EB0003", ("WEAR_RIGHT_NOISE_CONTROL_OFF", []) },
        { "EB0004", ("WEAR_BOTH_ANC", []) },
        { "EB0007", ("WEAR_BOTH_AMBIENT_SOUND", []) }
    };
    private static readonly Dictionary<string, string> Statuses = new()
    {
        { "EB0008", "CHARGE_WIRELESS_COUNT_LEFT" },
        { "EB0009", "CHARGE_WIRELESS_COUNT_RIGHT" },
        { "EB0010", "CHARGE_POWERSHARE_COUNT_LEFT" },
        { "EB0011", "CHARGE_POWERSHARE_COUNT_RIGHT" },
        { "EB0012", "CHARGE_WIRED_COUNT_LEFT" },
        { "EB0013", "CHARGE_WIRED_COUNT_RIGHT" },
        { "EB0014", "CHARGE_WIRELESS_TIME_LEFT" },
        { "EB0015", "CHARGE_WIRELESS_TIME_RIGHT" },
        { "EB0016", "CHARGE_POWERSHARE_TIME_LEFT" },
        { "EB0017", "CHARGE_POWERSHARE_TIME_RIGHT" },
        { "EB0018", "CHARGE_WIRED_TIME_LEFT" },
        { "EB0019", "CHARGE_WIRED_TIME_RIGHT" },
        { "9017", "EARBUDS_SW_VERSION" },
        { "9032", "RIGHT_EARBUD_CHARGING_CYCLE" },
        { "9033", "LEFT_EARBUD_CHARGING_CYCLE" }
    };
}