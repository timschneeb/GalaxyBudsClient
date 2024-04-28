using GalaxyBudsClient.Generated.Model.Attributes;
using GalaxyBudsClient.Model.Firmware;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.FOTA_UPDATE)]
internal class FotaUpdateDecoder : BaseMessageDecoder
{
    public byte ResultCode { get; }
    public FirmwareConstants.UpdateIds UpdateId { get; }
    public byte Percent { get; }
    public byte State { get; }
        
    public FotaUpdateDecoder(SppMessage msg) : base(msg)
    {
        UpdateId = (FirmwareConstants.UpdateIds) msg.Payload[0];
        switch (UpdateId)
        {
            case FirmwareConstants.UpdateIds.Percent:
                Percent = msg.Payload[1];
                break;
            case FirmwareConstants.UpdateIds.StateChange:
                State = msg.Payload[1];
                break;
        }
            
        ResultCode = msg.Payload[2];
    }
}