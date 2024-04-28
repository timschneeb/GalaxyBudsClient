using System;
using System.IO;
using GalaxyBudsClient.Generated.Model.Attributes;
using GalaxyBudsClient.Model.Firmware;

namespace GalaxyBudsClient.Message.Encoder;

[MessageEncoder(MsgIds.FOTA_DOWNLOAD_DATA)]
public class FotaDownloadDataEncoder : BaseMessageEncoder
{
    public FirmwareBinary? Binary { set; get; }
    public int EntryId { set; get; }
    public int MtuSize { set; get; }
    public int Offset { set; get; }

    public bool IsLastFragment()
    {
        var next = Binary?.GetSegmentById(EntryId);
        return next == null || Offset + (long) MtuSize >= next.Size;
    }
        
    public override SppMessage Encode()
    {
        var segment = Binary?.GetSegmentById(EntryId);
        var lastFragment = IsLastFragment();
        var chunkSize = lastFragment ? (int)(segment?.Size ?? 0) - Offset : MtuSize;
        
        if (segment == null || chunkSize < 0)
        {
            return new SppMessage(MsgIds.FOTA_DOWNLOAD_DATA, MsgTypes.Response, [])
            {
                IsFragment = true
            };
        }
        
        var header = BitConverter.GetBytes(MakeFragmentHeader(lastFragment, Offset));
        var payload = new byte[header.Length + chunkSize];
        Array.Copy(header, payload, header.Length);
        Array.Copy(segment.RawData, Offset, payload, header.Length, chunkSize);
        
        return new SppMessage(MsgIds.FOTA_DOWNLOAD_DATA, MsgTypes.Response, payload)
        {
            IsFragment = true
        };
    }
        
    private static int MakeFragmentHeader(bool isLastFragment, long j) {
        var i = (int) (j & 0x7FFFFFFFL);
        return isLastFragment ? i : i | int.MinValue;
    }
}