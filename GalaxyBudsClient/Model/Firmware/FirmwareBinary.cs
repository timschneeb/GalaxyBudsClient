using System;
using System.Buffers.Text;
using System.IO;
using System.Linq;
using System.Text;
using GalaxyBudsClient.Utils.DynamicLocalization;
using Sentry;
using Serilog;

namespace GalaxyBudsClient.Model.Firmware
{
    public class FirmwareBinary : IDisposable
    {
        private readonly long _magic;
        private MemoryStream _stream;
        
        private static readonly long FOTA_BIN_MAGIC = 3405695742L;
        private static readonly long FOTA_BIN_MAGIC_COMBINATION = 1111707469L;

        public string BuildName { get; }
        public long SegmentsCount { get; }
        public long TotalSize { get; }
        public int Crc32 { get; }
        public FirmwareSegment[] Segments { get; }

        public FirmwareBinary(byte[] data, string buildName)
        {
            /* stream is now owned/managed by this class */
            _stream = new MemoryStream(data);
            BuildName = buildName;
            
            byte[] bArr = new byte[4];
            try
            {
                if (_stream.Read(bArr) != -1)
                {
                    _magic = (((long) bArr[2] & 255) << 16) | (((long) bArr[3] & 255) << 24) |
                             (((long) bArr[1] & 255) << 8) | (((long) bArr[0]) & 255);
                    if (_magic == FOTA_BIN_MAGIC)
                    {
                        // Okay! Skip ahead
                        goto MAGIC_VALID;
                    }
                    if (_magic == FOTA_BIN_MAGIC_COMBINATION || Encoding.ASCII.GetString(data).StartsWith(":02000004FE00FC"))
                    {
                        // Notify tracker about this event and submit firmware build info
                        SentrySdk.CaptureMessage($"BCOM-Firmware discovered. Build: {buildName}, Content: {Convert.ToBase64String(data)}", SentryLevel.Fatal);
                      
                        Log.Fatal($"FirmwareBinary: Parsing internal debug firmware '{buildName}'. " +
                                  "This is unsupported by this application as these binaries are not meant for retail devices.");
                        throw new FirmwareParseException(FirmwareParseException.ErrorCodes.InvalidMagic,
                            Loc.Resolve("fw_fail_no_magic"));
                    }

                    _stream.Close();
                    throw new FirmwareParseException(FirmwareParseException.ErrorCodes.InvalidMagic,
                        Loc.Resolve("fw_fail_no_magic"));
                }

                MAGIC_VALID:
                if (_stream.Read(bArr) != -1)
                {
                    TotalSize = (((long) bArr[2] & 255) << 16) | (((long) bArr[3] & 255) << 24) |
                                (((long) bArr[1] & 255) << 8) | (((long) bArr[0]) & 255);
                    if (TotalSize == 0)
                    {
                        _stream.Close();
                        throw new FirmwareParseException(FirmwareParseException.ErrorCodes.SizeZero,
                            Loc.Resolve("fw_fail_size_null"));
                    }
                }

                if (_stream.Read(bArr) != -1)
                {
                    SegmentsCount = (((long) bArr[1] & 255) << 8) | (((long) bArr[3] & 255) << 24) |
                                    (((long) bArr[2] & 255) << 16) | (((long) bArr[0]) & 255);
                    if (SegmentsCount == 0)
                    {
                        _stream.Close();
                        throw new FirmwareParseException(FirmwareParseException.ErrorCodes.NoSegmentsFound,
                            Loc.Resolve("fw_fail_no_segments"));
                    }
                }

                Segments = new FirmwareSegment[SegmentsCount];
                for (var i = 0; i < SegmentsCount; i++)
                {
                    Segments[i] = new FirmwareSegment(i, SegmentsCount, _stream);
                }

                _stream.Seek(-4, SeekOrigin.End);
                _stream.Read(bArr, 0, 4);
                Crc32 = BitConverter.ToInt32(bArr);
            }
            catch (Exception ex) when (!(ex is FirmwareParseException))
            {
                _stream.Close();
                
                Log.Error($"FirmwareBinary: Failed to decode binary: {ex}");
                throw new FirmwareParseException(FirmwareParseException.ErrorCodes.Unknown,
                    $"{Loc.Resolve("fw_fail_unknown")}\n{ex}");
            }
        }

        public byte[] SerializeTable()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            
            writer.Write((int) Crc32);
            writer.Write((byte) SegmentsCount);

            foreach (var segment in Segments)
            {
                writer.Write((byte) segment.Id);
                writer.Write((int) segment.Size);
                writer.Write((int) segment.Crc32);
            }

            var table = stream.ToArray();
            stream.Close();
            
            return table;
        }

        public FirmwareSegment? GetSegmentById(int id)
        {
            return Segments.FirstOrDefault(segment => segment.Id == id);
        }

        public BufferedStream OpenStream()
        {
            return new BufferedStream(_stream);
        }
        
        public override string ToString()
        {
            return "Magic=" + $"{_magic:X2}" + ", TotalSize=" + TotalSize + ", SegmentCount=" + SegmentsCount + $", CRC32=0x{Crc32:X2}";
        }

        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}