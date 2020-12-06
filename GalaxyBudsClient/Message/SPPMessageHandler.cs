using System;
using GalaxyBudsClient.Decoder;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message
{
    class SPPMessageHandler
    {
        private static SPPMessageHandler _instance = null;
        private static readonly object SingletonPadlock = new object();

        public static SPPMessageHandler Instance
        {
            get
            {
                lock (SingletonPadlock)
                {
                    return _instance ?? (_instance = new SPPMessageHandler());
                }
            }
        }

        public event EventHandler<BaseMessageParser> AnyMessageReceived;
        public event EventHandler<int> ResetResponse;
        public event EventHandler<string> SwVersionResponse;
        public event EventHandler<BatteryTypeParser> BatteryTypeResponse;
        public event EventHandler<bool> AmbientEnabledUpdateResponse;
        public event EventHandler<bool> AncEnabledUpdateResponse;
        public event EventHandler<string> BuildStringResponse;
        public event EventHandler<DebugGetAllDataParser> GetAllDataResponse;
        public event EventHandler<DebugSerialNumberParser> SerialNumberResponse;
        public event EventHandler<GenericResponseParser> GenericResponse;
        public event EventHandler<SelfTestParser> SelfTestResponse;

        public event EventHandler<TouchOption.Universal> OtherOption;
        public event EventHandler<ExtendedStatusUpdateParser> ExtendedStatusUpdate;
        public event EventHandler<StatusUpdateParser> StatusUpdate;
        public event EventHandler<UsageReportParser> UsageReport;
        public event EventHandler<MuteUpdateParser> FindMyGearMuteUpdate;
        public event EventHandler FindMyGearStopped;

        public void MessageReceiver(object sender, SPPMessage e)
        {
            BaseMessageParser parser = SPPMessageParserFactory.BuildParser(e);
            AnyMessageReceived?.Invoke(this, parser);
            switch (e.Id)
            {
                case SPPMessage.MessageIds.MSG_ID_RESET:
                    ResetResponse?.Invoke(this, ((ResetResponseParser)parser).ResultCode);
                    break;
                case SPPMessage.MessageIds.MSG_ID_FOTA_DEVICE_INFO_SW_VERSION:
                    SwVersionResponse?.Invoke(this, ((SoftwareVersionOTAParser)parser).SoftwareVersion);
                    break;
                case SPPMessage.MessageIds.MSG_ID_BATTERY_TYPE:
                    BatteryTypeResponse?.Invoke(this, (BatteryTypeParser)parser);
                    break;
                case SPPMessage.MessageIds.MSG_ID_AMBIENT_MODE_UPDATED:
                    AmbientEnabledUpdateResponse?.Invoke(this, ((AmbientModeUpdateParser)parser).Enabled);
                    break;
                case SPPMessage.MessageIds.MSG_ID_DEBUG_BUILD_INFO:
                    BuildStringResponse?.Invoke(this, ((DebugBuildInfoParser)parser).BuildString);
                    break;
                case SPPMessage.MessageIds.MSG_ID_DEBUG_GET_ALL_DATA:
                    GetAllDataResponse?.Invoke(this, (DebugGetAllDataParser)parser);
                    break;
                case SPPMessage.MessageIds.MSG_ID_DEBUG_SERIAL_NUMBER:
                    SerialNumberResponse?.Invoke(this, (DebugSerialNumberParser)parser);
                    break;
                case SPPMessage.MessageIds.MSG_ID_EXTENDED_STATUS_UPDATED:
                    ExtendedStatusUpdate?.Invoke(this, (ExtendedStatusUpdateParser)parser);
                    break;
                case SPPMessage.MessageIds.MSG_ID_FIND_MY_EARBUDS_STOP:
                    FindMyGearStopped?.Invoke(this, null);
                    break;
                case SPPMessage.MessageIds.MSG_ID_RESP:
                    GenericResponse?.Invoke(this, (GenericResponseParser)parser);
                    break;
                case SPPMessage.MessageIds.MSG_ID_SELF_TEST:
                    SelfTestResponse?.Invoke(this, (SelfTestParser)parser);
                    break;
                case SPPMessage.MessageIds.MSG_ID_SET_TOUCHPAD_OTHER_OPTION:
                    OtherOption?.Invoke(this, ((SetOtherOptionParser)parser).OptionType);
                    break;
                case SPPMessage.MessageIds.MSG_ID_STATUS_UPDATED:
                    StatusUpdate?.Invoke(this, (StatusUpdateParser)parser);
                    break;
                case SPPMessage.MessageIds.MSG_ID_USAGE_REPORT:
                    UsageReport?.Invoke(this, (UsageReportParser)parser);
                    break;
                case SPPMessage.MessageIds.MSG_ID_MUTE_EARBUD_STATUS_UPDATED:
                    FindMyGearMuteUpdate?.Invoke(this, (MuteUpdateParser)parser);
                    break;
                case SPPMessage.MessageIds.MSG_ID_NOISE_REDUCTION_MODE_UPDATE:
                    AncEnabledUpdateResponse?.Invoke(this, ((NoiseReductionModeUpdateParser)parser).Enabled);
                    break;
            }
        }
    }
}
