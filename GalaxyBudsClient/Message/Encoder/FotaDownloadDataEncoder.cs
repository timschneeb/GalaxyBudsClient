using System;
using System.IO;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Firmware;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;

namespace GalaxyBudsClient.Message.Encoder
{
    public class FotaDownloadDataEncoder
    {
        private readonly FirmwareBinary _binary;
        private readonly int _entryId;
        private readonly int _offset;
        private readonly int _mtuSize;

        public FotaDownloadDataEncoder(FirmwareBinary binary, int entryId, int offset, int mtuSize)
        {
            _binary = binary;
            _entryId = entryId;
            _offset = offset;
            _mtuSize = mtuSize;
        }

        public int Offset => _offset;

        public bool IsLastFragment()
        {
            var next = _binary.GetSegmentById(_entryId);
            return next == null || Offset + ((long) _mtuSize) >= next.Size;
        }
        
        public SppMessage Build()
        {
            var segment = _binary.GetSegmentById(_entryId);
            if (segment == null)
            {
                goto RET_NULL;
            }
            
            var i = 0;
            var z = Offset + ((long) _mtuSize) >= segment.Size;
            var rawDataSize = z ? (int)segment.Size - Offset : _mtuSize;
            if (rawDataSize < 0) 
            {
                goto RET_NULL;
            }
            
            byte[] bArr = new byte[(int) (4 + rawDataSize)];
            byte[] header = BitConverter.GetBytes(MakeFragmentHeader(z, Offset));
            var length = header.Length;
            var i2 = 0;
            while (i < length) {
                bArr[i2] = header[i];
                i++;
                i2++;
            }
            
            var i3 = 5;
            while (true) {
                var i4 = i3 - 1;
                if (i3 <= 0) {
                    break;
                }

                var binStream = _binary.OpenStream();
                binStream.Seek(segment.Position + Offset, SeekOrigin.Begin);
                binStream.Read(bArr, i2, rawDataSize);

                i3 = i4;
            }

            return new SppMessage(SppMessage.MessageIds.FOTA_DOWNLOAD_DATA, SppMessage.MsgType.Response, bArr)
            {
                IsFragment = true
            };
            
            RET_NULL:
            return new SppMessage(SppMessage.MessageIds.FOTA_DOWNLOAD_DATA, SppMessage.MsgType.Response, Array.Empty<byte>())
            {
                IsFragment = true
            };
        }
        
        private static int MakeFragmentHeader(bool z, long j) {
            var i = (int) (j & 2147483647L);
            return z ? i : i | int.MinValue;
        }
    }
}