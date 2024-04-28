using System;
using System.IO;
using GalaxyBudsClient.Utils.Extensions;
using Serilog;

namespace GalaxyBudsClient.Model.Firmware;

public class FirmwareSegment
{
    public long Id { get; }
    public long Crc32 { get; }
    public long Position { get; }
    public long Size { get; }
    public byte[] RawData { private set; get; }
    public FirmwareBinary? NestedBinary { get; }

    public FirmwareSegment(BinaryReader reader, bool fullAnalysis)
    {
        Id = reader.ReadInt32();
        Crc32 = reader.ReadInt32();
        Position = reader.ReadInt32();
        Size = reader.ReadInt32();

        try
        {
            using var _ = reader.BaseStream.ScopedSeek(Position, SeekOrigin.Begin);
            RawData = reader.ReadBytes((int)Size);
        }
        catch (IOException ex)
        {
            Log.Error(ex, "Failed to read segment data");
            RawData = Array.Empty<byte>();
        }

        if (fullAnalysis && RawData.Length >= 4 &&
            BitConverter.ToUInt32(RawData, 0) == FirmwareBinary.FotaBinMagic)
        {
            NestedBinary = new FirmwareBinary(RawData, "0x" + $"{Position:X4} (nested)", true);
        }
    }

    public override string ToString()
    {
        return "ID=" + Id +
               ", Offset=0x" + $"{Position:X4}" +
               ", Size=" + Size +
               ", CRC32=0x" + $"{Crc32:X4}";
    }
}