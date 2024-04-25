using System;
using System.IO;
using System.Linq;
using System.Text;
using GalaxyBudsClient.Generated.I18N;
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
        using var reader = new BinaryReader(stream); 
        
        try
        {
            _magic = reader.ReadInt32();

            if (_magic == FOTA_BIN_MAGIC_COMBINATION || Encoding.ASCII.GetString(data).StartsWith(":02000004FE00FC"))
            {
                // Notify tracker about this event and submit firmware build info
                SentrySdk.ConfigureScope((x => x.AddAttachment(data, "firmware.bin")));
                SentrySdk.CaptureMessage($"BCOM-Firmware discovered. Build: {buildName}", SentryLevel.Fatal);
                  
                Log.Fatal("FirmwareBinary: Parsing internal debug firmware \'{Name}\'. " +
                          "This is unsupported by this application as these binaries are not meant for retail devices", buildName);
            }
            
            if (_magic != FOTA_BIN_MAGIC)
            {
                throw new FirmwareParseException(FirmwareParseException.ErrorCodes.InvalidMagic, Strings.FwFailNoMagic);
            }

            TotalSize = reader.ReadInt32();
            if (TotalSize == 0)
            {
                throw new FirmwareParseException(FirmwareParseException.ErrorCodes.SizeZero,
                    Strings.FwFailSizeNull);
            }
            
            SegmentsCount = reader.ReadInt32();
            if (SegmentsCount == 0)
            {
                throw new FirmwareParseException(FirmwareParseException.ErrorCodes.NoSegmentsFound,
                    Strings.FwFailNoSegments);
            }
            
            Segments = new FirmwareSegment[SegmentsCount];
            for (var i = 0; i < SegmentsCount; i++)
            {
                Segments[i] = new FirmwareSegment(reader);
            }

            reader.BaseStream.Seek(-4, SeekOrigin.End);
            Crc32 = reader.ReadInt32();
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
        foreach (var model in ModelsExtensions.GetValues())
        {
            var fwPattern = model.GetModelMetadataAttribute()?.FwPattern;
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