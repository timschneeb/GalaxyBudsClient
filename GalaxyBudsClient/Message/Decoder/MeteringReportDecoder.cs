using System.IO;
using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.METERING_REPORT)]
public class MeteringReportDecoder : BaseMessageDecoder
{
    public byte Revision { set; get; }
    public bool IsLeftConnected { set; get; }
    public bool IsRightConnected { set; get; }
    public short TotalBatteryCapacity { set; get; }
    public byte BatteryL { set; get; }
    public byte BatteryR { set; get; }
    public int A2dpUsingTimeL { set; get; }
    public int A2dpUsingTimeR { set; get; }
    public int EscoUsingTimeL { set; get; }
    public int EscoUsingTimeR { set; get; }
    public int AncOnTimeL { set; get; }
    public int AncOnTimeR { set; get; }
    public int AmbientOnTimeL { set; get; }
    public int AmbientOnTimeR { set; get; }
    
    public override void Decode(SppMessage msg)
    {
        using var stream = new MemoryStream(msg.Payload);
        using var reader = new BinaryReader(stream);
        
        Revision = reader.ReadByte();
        var connectedSide = reader.ReadByte();
        IsLeftConnected = (byte)((connectedSide & 240) >> 4) == 1;
        IsRightConnected = (byte)(connectedSide & 15) == 1;

        if (Revision >= 2)
        {
            TotalBatteryCapacity = reader.ReadInt16();
        }

        if (IsLeftConnected)
        {
            BatteryL = reader.ReadByte();
            A2dpUsingTimeL = reader.ReadInt32();
            EscoUsingTimeL = reader.ReadInt32();
            AncOnTimeL = reader.ReadInt32();
            AmbientOnTimeL = reader.ReadInt32();
        }

        if (IsRightConnected)
        {
            BatteryR = reader.ReadByte();
            A2dpUsingTimeR = reader.ReadInt32();
            EscoUsingTimeR = reader.ReadInt32();
            AncOnTimeR = reader.ReadInt32();
            AmbientOnTimeR = reader.ReadInt32();
        }
    }
}