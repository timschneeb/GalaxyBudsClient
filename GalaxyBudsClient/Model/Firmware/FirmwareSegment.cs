using System.IO;

namespace GalaxyBudsClient.Model.Firmware;

public class FirmwareSegment
{
    public long Crc32 { get; }
    public long Position { get; }
    public long Size { get; }
    public long Id { get; }

    public FirmwareSegment(int i, long count, Stream stream)
    {
        var bArr = new byte[256];
        var i2 = i * 16 + 12;
            
        stream.Seek(0, SeekOrigin.Begin);
        stream.Read(bArr);
        Id = (((long) bArr[i2 + 3] & 255) << 24) | (((long) bArr[i2 + 2] & 255) << 16) |
             (((long) bArr[i2 + 1] & 255) << 8) | ((long) bArr[i2] & 255);
        var i3 = i2 + 4;
        Crc32 = (((long) bArr[i3 + 3] & 255) << 24) | (((long) bArr[i3 + 2] & 255) << 16) |
                (((long) bArr[i3 + 1] & 255) << 8) | ((long) bArr[i3] & 255);
        var i4 = i3 + 4;
        Position = (((long) bArr[i4 + 3] & 255) << 24) | (((long) bArr[i4 + 2] & 255) << 16) |
                   (((long) bArr[i4 + 1] & 255) << 8) | ((long) bArr[i4] & 255);
        var i5 = i4 + 4;
        Size = (((long) bArr[i5 + 1] & 255) << 8) | (((long) bArr[i5 + 3] & 255) << 24) |
               (((long) bArr[i5 + 2] & 255) << 16) | ((long) bArr[i5] & 255);
    }

    public override string ToString()
    {
        return "ID=" + Id +
               ", Offset=0x" + $"{Position:X4}" +
               ", Size=" + Size +
               ", CRC32=0x" + $"{Crc32:X4}";
    }
}