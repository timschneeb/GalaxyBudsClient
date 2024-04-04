using System;
using GalaxyBudsClient.Model.Firmware;
using Serilog;

namespace GalaxyBudsClient.Message.Decoder;

internal class FotaControlDecoder : BaseMessageDecoder
{
    public override MsgIds HandledType => MsgIds.FOTA_CONTROL;

    public FirmwareConstants.ControlIds ControlId { set; get; }
    public short Id { set; get; }
    public short MtuSize { set; get; }

    public override void Decode(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

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