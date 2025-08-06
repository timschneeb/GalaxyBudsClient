using System.IO;
using GalaxyBudsClient.Generated.Model.Attributes;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.METERING_REPORT)]
public class MeteringReportDecoder : BaseMessageDecoder
{
    public byte Revision { get; }
    public bool IsLeftConnected { get; }
    public bool IsRightConnected { get; }
    public short? TotalBatteryCapacity { get; }
    public byte? BatteryL { get; }
    public byte? BatteryR { get; }
    public int? A2dpUsingTimeL { get; }
    public int? A2dpUsingTimeR { get; }
    public int? EscoUsingTimeL { get; }
    public int? EscoUsingTimeR { get; }
    public int? AncOnTimeL { get; }
    public int? AncOnTimeR { get; }
    public int? AmbientOnTimeL { get; }
    public int? AmbientOnTimeR { get; }
    public int? AdaptiveOnTimeL { get; }
    public int? AdaptiveOnTimeR { get; }
    
    public MeteringReportDecoder(SppMessage msg) : base(msg)
    {
        using var stream = new MemoryStream(msg.Payload);
        using var reader = new BinaryReader(stream);
        
        Revision = SafeReadByte(reader);
        var connectedSide = SafeReadByte(reader);
        IsLeftConnected = (byte)((connectedSide & 240) >> 4) == 1;
        IsRightConnected = (byte)(connectedSide & 15) == 1;

        if (Revision >= 2)
        {
            TotalBatteryCapacity = reader.ReadInt16();
        }

        if (IsLeftConnected)
        {
            BatteryL = SafeReadByte(reader);
            A2dpUsingTimeL = reader.ReadInt32();
            EscoUsingTimeL = reader.ReadInt32();
            AncOnTimeL = reader.ReadInt32();
            AmbientOnTimeL = reader.ReadInt32();

            if (TargetModel == Models.Buds3Pro)
            {
                AdaptiveOnTimeL = reader.ReadInt32();
            }
        }

        if (IsRightConnected)
        {
            BatteryR = SafeReadByte(reader);
            A2dpUsingTimeR = reader.ReadInt32();
            EscoUsingTimeR = reader.ReadInt32();
            AncOnTimeR = reader.ReadInt32();
            AmbientOnTimeR = reader.ReadInt32();
            
            if (TargetModel == Models.Buds3Pro)
            {
                AdaptiveOnTimeR = reader.ReadInt32();
            }
        }
    }
}