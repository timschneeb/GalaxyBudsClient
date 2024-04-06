using System;
using System.IO;
using System.Linq;
using System.Text;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Utils;
using Sentry;
using Serilog;

namespace GalaxyBudsClient.Model.Firmware;

public class FirmwareBinary
{
    private readonly long _magic;
    private readonly byte[] _data;
        
    private static readonly long FOTA_BIN_MAGIC = 0xCAFECAFE;
    private static readonly long FOTA_BIN_MAGIC_COMBINATION = 0x42434F4D;

    public string BuildName { get; }
    public long SegmentsCount { get; }
    public long TotalSize { get; }
    public int Crc32 { get; }
    public FirmwareSegment[] Segments { get; }

    public FirmwareBinary(byte[] data, string buildName)
    {
        _data = data;
        BuildName = buildName;
            
        using var stream = new MemoryStream(data);
            
        var bArr = new byte[4];
        try
        {
            if (stream.Read(bArr) != -1)
            {
                _magic = (((long) bArr[2] & 255) << 16) | (((long) bArr[3] & 255) << 24) |
                         (((long) bArr[1] & 255) << 8) | ((long) bArr[0] & 255);
                if (_magic == FOTA_BIN_MAGIC)
                {
                    // Okay! Skip ahead
                    goto MAGIC_VALID;
                }
                if (_magic == FOTA_BIN_MAGIC_COMBINATION || Encoding.ASCII.GetString(data).StartsWith(":02000004FE00FC"))
                {
                    // Notify tracker about this event and submit firmware build info
                    SentrySdk.CaptureMessage($"BCOM-Firmware discovered. Build: {buildName}, Content: {Convert.ToBase64String(data)}", SentryLevel.Fatal);
                      
                    Log.Fatal("FirmwareBinary: Parsing internal debug firmware \'{Name}\'. " +
                              "This is unsupported by this application as these binaries are not meant for retail devices", buildName);
                }

                throw new FirmwareParseException(FirmwareParseException.ErrorCodes.InvalidMagic, Strings.FwFailNoMagic);
            }

            MAGIC_VALID:
            if (stream.Read(bArr) != -1)
            {
                TotalSize = (((long) bArr[2] & 255) << 16) | (((long) bArr[3] & 255) << 24) |
                            (((long) bArr[1] & 255) << 8) | ((long) bArr[0] & 255);
                if (TotalSize == 0)
                {
                    throw new FirmwareParseException(FirmwareParseException.ErrorCodes.SizeZero,
                        Strings.FwFailSizeNull);
                }
            }

            if (stream.Read(bArr) != -1)
            {
                SegmentsCount = (((long) bArr[1] & 255) << 8) | (((long) bArr[3] & 255) << 24) |
                                (((long) bArr[2] & 255) << 16) | ((long) bArr[0] & 255);
                if (SegmentsCount == 0)
                {
                    throw new FirmwareParseException(FirmwareParseException.ErrorCodes.NoSegmentsFound,
                        Strings.FwFailNoSegments);
                }
            }

            Segments = new FirmwareSegment[SegmentsCount];
            for (var i = 0; i < SegmentsCount; i++)
            {
                Segments[i] = new FirmwareSegment(i, SegmentsCount, stream);
            }

            stream.Seek(-4, SeekOrigin.End);
            stream.Read(bArr, 0, 4);
            Crc32 = BitConverter.ToInt32(bArr);
        }
        catch (Exception ex) when (ex is not FirmwareParseException)
        {
            Log.Error(ex, "FirmwareBinary: Failed to decode binary");
            throw new FirmwareParseException(FirmwareParseException.ErrorCodes.Unknown,
                $"{Strings.FwFailUnknown}\n{ex}");
        }
    }

    public byte[] SerializeTable()
    {
        using var stream = new MemoryStream();
        var writer = new BinaryWriter(stream);
            
        writer.Write((int) Crc32);
        writer.Write((byte) SegmentsCount);

        foreach (var segment in Segments)
        {
            writer.Write((byte) segment.Id);
            writer.Write((int) segment.Size);
            writer.Write((int) segment.Crc32);
        }
            
        return stream.ToArray();
    }

    public FirmwareSegment? GetSegmentById(int id)
    {
        return Segments.FirstOrDefault(segment => segment.Id == id);
    }

    public MemoryStream OpenStream()
    {
        return new MemoryStream(_data);
    }

    public Models? DetectModel()
    {
        var fastSearch = new BoyerMoore();
        foreach (var model in Enum.GetValues<Models>())
        {
            var fwPattern = model.GetModelMetadata()?.FwPattern;
            if(fwPattern == null)
                continue;
                
            fastSearch.SetPattern(Encoding.ASCII.GetBytes(fwPattern));
            if (fastSearch.Search(_data) >= 0)
            {
                return model;
            }
        }
            
        return null;
    }
        
    public override string ToString()
    {
        return "Magic=" + $"{_magic:X2}" + ", TotalSize=" + TotalSize + ", SegmentCount=" + SegmentsCount + $", CRC32=0x{Crc32:X2}";
    }
}