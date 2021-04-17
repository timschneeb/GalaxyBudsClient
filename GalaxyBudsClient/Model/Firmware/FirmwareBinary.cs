using System;
using System.IO;
using System.Linq;
using Serilog;

namespace GalaxyBudsClient.Model.Firmware
{
    public class FirmwareBinary
    {
        private long _magic;
        private string _path;
        
        private static readonly long FOTA_BIN_MAGIC = 3405695742L;

        public long SegmentsCount { get; }
        public long TotalSize { get; }
        public int Crc32 { get; }
        public FirmwareSegment[] Segments { get; } = new FirmwareSegment[0];

        public FirmwareBinary(string path)
        {
            _path = path;
            
            byte[] bArr = new byte[4];
            FileStream fileStream = File.OpenRead(path);
            BufferedStream stream = new BufferedStream(fileStream);
            try
            {
                if (stream.Read(bArr) != -1)
                {
                    _magic = (((long) bArr[2] & 255) << 16) | (((long) bArr[3] & 255) << 24) |
                             (((long) bArr[1] & 255) << 8) | (((long) bArr[0]) & 255);
                    if (_magic != FOTA_BIN_MAGIC)
                    {
                        stream.Close();
                        fileStream.Close();
                        throw new FirmwareParseException(FirmwareParseException.ErrorCodes.InvalidMagic,
                            "This is not a valid firmware binary. Invalid magic value found in file header.");
                    }
                }

                if (stream.Read(bArr) != -1)
                {
                    TotalSize = (((long) bArr[2] & 255) << 16) | (((long) bArr[3] & 255) << 24) |
                                (((long) bArr[1] & 255) << 8) | (((long) bArr[0]) & 255);
                    if (TotalSize == 0)
                    {
                        stream.Close();
                        fileStream.Close();
                        throw new FirmwareParseException(FirmwareParseException.ErrorCodes.SizeZero,
                            "This firmware binary has its size set to zero and cannot be used for flashing. Please choose another one.");
                    }
                }

                if (stream.Read(bArr) != -1)
                {
                    SegmentsCount = (((long) bArr[1] & 255) << 8) | (((long) bArr[3] & 255) << 24) |
                                    (((long) bArr[2] & 255) << 16) | (((long) bArr[0]) & 255);
                    if (SegmentsCount == 0)
                    {
                        stream.Close();
                        fileStream.Close();
                        throw new FirmwareParseException(FirmwareParseException.ErrorCodes.NoSegmentsFound,
                            "This firmware binary does not contain any binary segments and is empty. Please choose another one.");
                    }
                }

                Segments = new FirmwareSegment[SegmentsCount];
                for (var i = 0; i < SegmentsCount; i++)
                {
                    Segments[i] = new FirmwareSegment(i, SegmentsCount, path);
                }

                stream.Seek(-4, SeekOrigin.End);
                stream.Read(bArr, 0, 4);
                Crc32 = BitConverter.ToInt32(bArr);

                stream.Close();
                fileStream.Close();
            }
            catch (Exception ex)
            {
                stream.Close();
                fileStream.Close();
                
                Log.Fatal($"FirmwareBinary: Failed to decode binary: {ex}");
     
                throw new FirmwareParseException(FirmwareParseException.ErrorCodes.NoSegmentsFound,
                    $"Unexpected error while decoding the firmware binary. This firmware archive might be corrupted. Details:\n{ex}");
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
            return new BufferedStream(File.OpenRead(_path));
        }
        
        public override string ToString()
        {
            return "Magic=" + $"{_magic:X2}" + ", TotalSize=" + TotalSize + ", SegmentCount=" + SegmentsCount + $", CRC32=0x{Crc32:X2}";
        }
    }
}