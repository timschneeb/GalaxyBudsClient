using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using GalaxyBudsClient.Generated.Model.Attributes;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.DEBUG_EVENT)]
public class GenericEventDecoder : BaseMessageDecoder
{
    public Devices Device { get; }
    public TimeSpan Timestamp { get; }
    public byte EventRawId { get; }
    public EventIds EventId { get; }
    public MsgTypes MessageType { get; }
    public byte[] EventData { get; } 
    // EventIds and EventData contents are mostly unknown

    public GenericEventDecoder(SppMessage msg) : base(msg)
    {
        using var stream = new MemoryStream(msg.Payload);
        using var reader = new BinaryReader(stream);
        
        Device = reader.ReadChar() == 'L' ? Devices.L : Devices.R;
        Timestamp = TimeSpan.FromMilliseconds(reader.ReadUInt32());
        EventRawId = SafeReadByte(reader); 
        EventId = (EventIds)EventRawId;
        MessageType = (MsgTypes)SafeReadByte(reader);
        
        try
        {
            EventData = msg.Payload[7..];
        }
        catch (Exception)
        {
            EventData = [];
        }
    }
    
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum EventIds
    {
        UNKNOWN = 0,
        EARBUD_STATUS = 1,
        EARBUD_GTIME = 16,
        CRADLE_INFO = 17,
        BATTERY_INFO = 18,
        BRDG_CONGESTED_RELEASE = 82,
        FOTA_BINARY_UPDATE = 145,
        FOTA_HOME_BINARY_UPDATE = 146,
        FOTA_ROLE_CHANGE_FAILED_A = 147,
        FOTA_ROLE_CHANGE_FAILED_B = 148,
        FOTA_UPDATE_MGR_API_OPEN = 149,
        FOTA_UPDATE_MGR_API_ABORT = 150,
        FOTA_UPDATE_MGR_API_CLOSE = 151,
        FOTA_UPDATE_MGR_INVALID_EVT = 152,
        FOTA_EARBUDSTATE_AT_USER_ABORT = 153,
        SH_AMBIENT_EVT = 176,
        SH_AMBIENT_INVALID_WEAR_STATE = 177,
        SH_WEAR_STATE_CHANGED = 178,
        SESSION_VOLUME = 179,
        FOTA2_NONE = 47872,
        FOTA2_FSM_CHANGED = 47873,
        FOTA2_DOWNLOAD_ABORT = 47874,
        FOTA2_DOWNLOAD_ABORTED = 47875,
        FOTA2_DOWNLOAD_COMMIT = 47876,
        FOTA2_DOWNLOAD_COMMITED = 47877,
        FOTA2_DOWNLOAD_FINISH = 47878,
        FOTA2_DOWNLOAD_TIMEOUT = 47879,
        FOTA2_DOWNLOAD_TIMEOUT_CB = 47880
    }
}