using System;
using GalaxyBudsClient.Generated.Model.Attributes;
using GalaxyBudsClient.Model.Firmware;
using Serilog;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.FOTA_CONTROL)]
internal class FotaControlDecoder : BaseMessageDecoder
{
    public FirmwareConstants.ControlIds ControlId { set; get; }
    public short Id { set; get; }
    public short MtuSize { set; get; }

    public override void Decode(SppMessage msg)
    {
        ControlId = (FirmwareConstants.ControlIds) msg.Payload[0];
            
        switch (ControlId)
        {
            case FirmwareConstants.ControlIds.SendMtu:
                MtuSize = BitConverter.ToInt16(msg.Payload, 1);
                MtuSize = MtuSize > 650 ? (short)650 : MtuSize;
                break;
            case FirmwareConstants.ControlIds.ReadyToDownload:
                Id = BitConverter.ToInt16(msg.Payload, 1);
                break;
            default:
                Log.Debug("FotaControlDecoder: Unknown ControlId {Id}", msg.Payload[0]);
                break;
        }
    }
}