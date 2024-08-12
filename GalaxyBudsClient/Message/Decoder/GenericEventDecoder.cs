using System;
using System.IO;
using GalaxyBudsClient.Generated.Model.Attributes;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.UNK_GENERIC_EVENT)]
public class GenericEventDecoder : BaseMessageDecoder
{
    public Devices Device { get; }
    public TimeSpan Timestamp { get; }
    public byte EventId { get; }
    public MsgTypes MessageType { get; }
    public byte[] EventData { get; } 
    // EventIds and EventData contents are unknown

    public GenericEventDecoder(SppMessage msg) : base(msg)
    {
        using var stream = new MemoryStream(msg.Payload);
        using var reader = new BinaryReader(stream);
        
        Device = reader.ReadChar() == 'L' ? Devices.L : Devices.R;
        Timestamp = TimeSpan.FromMilliseconds(reader.ReadUInt32());
        EventId = reader.ReadByte(); 
        MessageType = (MsgTypes)reader.ReadByte();
        
        try
        {
            EventData = msg.Payload[7..];
        }
        catch (Exception)
        {
            EventData = Array.Empty<byte>();
        }
    }
}

/*
   TODO Find out what some of the EventIds and EventData contents are

   this.debugArraySubMsgId.put(1, "EARBUD_STATUS");
   this.debugArraySubMsgId.put(16, "EARBUD_GTIME");
   this.debugArraySubMsgId.put(17, "CRADLE_INFO");
   this.debugArraySubMsgId.put(18, "BATTERY_INFO");
   this.debugArraySubMsgId.put(82, "BRDG_CONGESTED_RELEASE");
   this.debugArraySubMsgId.put(145, "FOTA_BINARY_UPDATE");
   this.debugArraySubMsgId.put(146, "FOTA_HOME_BINARY_UPDATE");
   this.debugArraySubMsgId.put(147, "FOTA_ROLE_CHANGE_FAILED_A");
   this.debugArraySubMsgId.put(148, "FOTA_ROLE_CHANGE_FAILED_B");
   this.debugArraySubMsgId.put(149, "FOTA_UPDATE_MGR_API_OPEN");
   this.debugArraySubMsgId.put(150, "FOTA_UPDATE_MGR_API_ABORT");
   this.debugArraySubMsgId.put(151, "FOTA_UPDATE_MGR_API_CLOSE");
   this.debugArraySubMsgId.put(152, "FOTA_UPDATE_MGR_INVALID_EVT");
   this.debugArraySubMsgId.put(153, "FOTA_EARBUDSTATE_AT_USER_ABORT");
   this.debugArraySubMsgId.put(176, "SH_AMBIENT_EVT");
   this.debugArraySubMsgId.put(177, "SH_AMBIENT_INVALID_WEAR_STATE");
   this.debugArraySubMsgId.put(178, "SH_WEAR_STATE_CHANGED");
   this.debugArraySubMsgId.put(179, "SESSION_VOLUME");
   this.debugArraySubMsgId.put(47872, "FOTA2_NONE");
   this.debugArraySubMsgId.put(47873, "FOTA2_FSM_CHANGED");
   this.debugArraySubMsgId.put(47874, "FOTA2_DOWNLOAD_ABORT");
   this.debugArraySubMsgId.put(47875, "FOTA2_DOWNLOAD_ABORTED");
   this.debugArraySubMsgId.put(47876, "FOTA2_DOWNLOAD_COMMIT");
   this.debugArraySubMsgId.put(47877, "FOTA2_DOWNLOAD_COMMITED");
   this.debugArraySubMsgId.put(47878, "FOTA2_DOWNLOAD_FINISH");
   this.debugArraySubMsgId.put(47879, "FOTA2_DOWNLOAD_TIMEOUT");
   this.debugArraySubMsgId.put(47880, "FOTA2_DOWNLOAD_TIMEOUT_CB");

*/