using GalaxyBudsClient.Model.Firmware;

namespace GalaxyBudsClient.Message.Decoder
{
    class FotaUpdateParser : BaseMessageParser
    {
        public override SppMessage.MessageIds HandledType => SppMessage.MessageIds.FOTA_UPDATE;

        public byte ResultCode { set; get; }
        public FirmwareConstants.UpdateIds UpdateId { set; get; }
        public byte Percent { set; get; }
        public byte State { set; get; }
        
        public override void ParseMessage(SppMessage msg)
        {
            if (msg.Id != HandledType)
                return;

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
}