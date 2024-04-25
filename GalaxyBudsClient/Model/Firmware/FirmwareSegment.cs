using System.IO;

namespace GalaxyBudsClient.Model.Firmware;

public class FirmwareSegment
{
    public long Id { get; }
    public long Crc32 { get; }
    public long Position { get; }
    public long Size { get; }

    public FirmwareSegment(BinaryReader reader)
    {
        Id = reader.ReadInt32();
        Crc32 = reader.ReadInt32();
        Position = reader.ReadInt32();
        Size = reader.ReadInt32();
    }

    public override string ToString()
    {
        return "ID=" + Id +
               ", Offset=0x" + $"{Position:X4}" +
               ", Size=" + Size +
               ", CRC32=0x" + $"{Crc32:X4}";
    }
}