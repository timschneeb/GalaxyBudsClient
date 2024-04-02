using System;
using System.IO;
using GalaxyBudsClient.Model.Firmware;

namespace GalaxyBudsClient.Message.Encoder;

public class FotaDownloadDataEncoder : BaseMessageEncoder
{
    public override MsgIds HandledType => MsgIds.FOTA_DOWNLOAD_DATA;
    public required FirmwareBinary Binary { init; get; }
    public required int EntryId { init; get; }
    public required int MtuSize { init; get; }
    public required int Offset { init; get; }

    public bool IsLastFragment()
    {
        var next = Binary.GetSegmentById(EntryId);
        return next == null || Offset + (long) MtuSize >= next.Size;
    }
        
    public override SppMessage Encode()
    {
        var segment = Binary.GetSegmentById(EntryId);
        if (segment == null)
        {
            goto RET_NULL;
        }
            
        var i = 0;
        var z = Offset + (long) MtuSize >= segment.Size;
        var rawDataSize = z ? (int)segment.Size - Offset : MtuSize;
        if (rawDataSize < 0) 
        {
            goto RET_NULL;
        }
            
        var bArr = new byte[(int) (4 + rawDataSize)];
        var header = BitConverter.GetBytes(MakeFragmentHeader(z, Offset));
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

            var binStream = Binary.OpenStream();
            binStream.Seek(segment.Position + Offset, SeekOrigin.Begin);
            binStream.Read(bArr, i2, rawDataSize);

            i3 = i4;
        }

        return new SppMessage(HandledType, MsgTypes.Response, bArr)
        {
            IsFragment = true
        };
            
        RET_NULL:
        return new SppMessage(HandledType, MsgTypes.Response, Array.Empty<byte>())
        {
            IsFragment = true
        };
    }
        
    private static int MakeFragmentHeader(bool z, long j) {
        var i = (int) (j & 0x7FFFFFFFL);
        return z ? i : i | int.MinValue;
    }
}