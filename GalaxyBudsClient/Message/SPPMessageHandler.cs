using System;
using Avalonia.Threading;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Constants;
using Serilog;

namespace GalaxyBudsClient.Message
{
    public class SPPMessageHandler
    {
        private static SPPMessageHandler? _instance = null;
        private static readonly object SingletonPadlock = new object();

        public static SPPMessageHandler Instance
        {
            get
            {
                lock (SingletonPadlock)
                {
                    return _instance ??= new SPPMessageHandler();
                }
            }
        }

        public event EventHandler<BaseMessageParser?>? AnyMessageReceived;
        public event EventHandler<int>? ResetResponse;
        public event EventHandler<string>? SwVersionResponse;
        public event EventHandler<BatteryTypeParser>? BatteryTypeResponse;
        public event EventHandler<bool>? AmbientEnabledUpdateResponse;
        public event EventHandler<bool>? AncEnabledUpdateResponse;
        public event EventHandler<NoiseControlMode>? NoiseControlUpdateResponse;
        public event EventHandler<string>? BuildStringResponse;
        public event EventHandler<DebugGetAllDataParser>? GetAllDataResponse;
        public event EventHandler<DebugSerialNumberParser>? SerialNumberResponse;
        public event EventHandler<GenericResponseParser>? GenericResponse;
        public event EventHandler<SelfTestParser>? SelfTestResponse;
        public event EventHandler<TouchOptions>? OtherOption;
        public event EventHandler<ExtendedStatusUpdateParser>? ExtendedStatusUpdate;
        public event EventHandler<IBasicStatusUpdate>? BaseUpdate;
        public event EventHandler<StatusUpdateParser>? StatusUpdate;
        public event EventHandler<UsageReportParser>? UsageReport;
        public event EventHandler<MuteUpdateParser>? FindMyGearMuteUpdate;
        public event EventHandler? FindMyGearStopped;

        public void MessageReceiver(object? sender, SPPMessage e)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                BaseMessageParser? parser = SPPMessageParserFactory.BuildParser(e);
                DispatchEvent(parser, e.Id);
                
            }, DispatcherPriority.DataBind);
        }

        public void DispatchEvent(BaseMessageParser? parser, SPPMessage.MessageIds? ids = null)
        {
            AnyMessageReceived?.Invoke(this, parser);
            switch (ids ?? parser?.HandledType ?? 0)
            {
                case SPPMessage.MessageIds.RESET:
                    ResetResponse?.Invoke(this, (parser as ResetResponseParser)?.ResultCode ?? -1);
                    break;
                case SPPMessage.MessageIds.FOTA_DEVICE_INFO_SW_VERSION:
                    SwVersionResponse?.Invoke(this, (parser as SoftwareVersionOTAParser)?.SoftwareVersion ?? "null");
                    break;
                case SPPMessage.MessageIds.BATTERY_TYPE:
                    BatteryTypeResponse?.Invoke(this, (parser as BatteryTypeParser)!);
                    break;
                case SPPMessage.MessageIds.AMBIENT_MODE_UPDATED:
                    AmbientEnabledUpdateResponse?.Invoke(this, (parser as AmbientModeUpdateParser)?.Enabled ?? false);
                    break;
                case SPPMessage.MessageIds.DEBUG_BUILD_INFO:
                    BuildStringResponse?.Invoke(this, (parser as DebugBuildInfoParser)?.BuildString ?? "null");
                    break;
                case SPPMessage.MessageIds.DEBUG_GET_ALL_DATA:
                    GetAllDataResponse?.Invoke(this, (parser as DebugGetAllDataParser)!);
                    break;
                case SPPMessage.MessageIds.DEBUG_SERIAL_NUMBER:
                    SerialNumberResponse?.Invoke(this, (parser as DebugSerialNumberParser)!);
                    break;
                case SPPMessage.MessageIds.EXTENDED_STATUS_UPDATED:
                    BaseUpdate?.Invoke(this, (parser as IBasicStatusUpdate)!);
                    ExtendedStatusUpdate?.Invoke(this, (parser as ExtendedStatusUpdateParser)!);
                    break;
                case SPPMessage.MessageIds.FIND_MY_EARBUDS_STOP:
                    FindMyGearStopped?.Invoke(this, EventArgs.Empty);
                    break;
                case SPPMessage.MessageIds.RESP:
                    GenericResponse?.Invoke(this, (parser as GenericResponseParser)!);
                    break;
                case SPPMessage.MessageIds.SELF_TEST:
                    SelfTestResponse?.Invoke(this, (parser as SelfTestParser)!);
                    break;
                case SPPMessage.MessageIds.SET_TOUCHPAD_OTHER_OPTION:
                    OtherOption?.Invoke(this, (parser as SetOtherOptionParser)!.OptionType);
                    break;
                case SPPMessage.MessageIds.STATUS_UPDATED:
                    StatusUpdate?.Invoke(this, (parser as StatusUpdateParser)!);
                    BaseUpdate?.Invoke(this, (parser as IBasicStatusUpdate)!);
                    break;
                case SPPMessage.MessageIds.USAGE_REPORT:
                    UsageReport?.Invoke(this, (parser as UsageReportParser)!);
                    break;
                case SPPMessage.MessageIds.MUTE_EARBUD_STATUS_UPDATED:
                    FindMyGearMuteUpdate?.Invoke(this, (parser as MuteUpdateParser)!);
                    break;
                case SPPMessage.MessageIds.NOISE_REDUCTION_MODE_UPDATE:
                    AncEnabledUpdateResponse?.Invoke(this,
                        (parser as NoiseReductionModeUpdateParser)?.Enabled ?? false);
                    break;
                case SPPMessage.MessageIds.NOISE_CONTROLS_UPDATE:
                    NoiseControlUpdateResponse?.Invoke(this,
                        (parser as NoiseControlUpdateParser)?.Mode ?? NoiseControlMode.Off);
                    break;
            }
        }

    }
}