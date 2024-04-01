using System;
using Avalonia.Threading;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Utils;
using Serilog;

namespace GalaxyBudsClient.Message;

public class SppMessageHandler
{
    private static SppMessageHandler? _instance = null;
    private static readonly object SingletonPadlock = new();

    public static SppMessageHandler Instance
    {
        get
        {
            lock (SingletonPadlock)
            {
                return _instance ??= new SppMessageHandler();
            }
        }
    }

    public event EventHandler<BaseMessageParser?>? AnyMessageReceived;
    public event EventHandler<int>? ResetResponse;
    public event EventHandler<string>? SwVersionResponse;
    public event EventHandler<BatteryTypeParser>? BatteryTypeResponse;
    public event EventHandler<bool>? AmbientEnabledUpdateResponse;
    public event EventHandler<bool>? AncEnabledUpdateResponse;
    public event EventHandler<NoiseControlModes>? NoiseControlUpdateResponse;
    public event EventHandler<string>? BuildStringResponse;
    public event EventHandler<DebugGetAllDataParser>? GetAllDataResponse;
    public event EventHandler<DebugSerialNumberParser>? SerialNumberResponse;
    public event EventHandler<CradleSerialNumberParser>? CradleSerialNumberResponse;
    public event EventHandler<GenericResponseParser>? GenericResponse;
    public event EventHandler<SelfTestParser>? SelfTestResponse;
    public event EventHandler<TouchOptions>? OtherOption;
    public event EventHandler<ExtendedStatusUpdateParser>? ExtendedStatusUpdate;
    public event EventHandler<IBasicStatusUpdate>? BaseUpdate;
    public event EventHandler<StatusUpdateParser>? StatusUpdate;
    public event EventHandler<UsageReportParser>? UsageReport;
    public event EventHandler<MuteUpdateParser>? FindMyGearMuteUpdate;
    public event EventHandler? FindMyGearStopped;
    public event EventHandler<FitTestParser>? FitTestResult;
    public event EventHandler<DebugSkuParser>? DebugSkuUpdate;

    public void MessageReceiver(object? sender, SppMessage e)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            var parser = e.BuildParser();
            DispatchEvent(parser, e.Id);
                
        }, DispatcherPriority.Normal);
    }

    public void DispatchEvent(BaseMessageParser? parser, MsgIds? ids = null)
    {
        AnyMessageReceived?.Invoke(this, parser);
        switch (ids ?? parser?.HandledType ?? 0)
        {
            case MsgIds.RESET:
                ResetResponse?.Invoke(this, (parser as ResetResponseParser)?.ResultCode ?? -1);
                break;
            case MsgIds.FOTA_DEVICE_INFO_SW_VERSION:
                SwVersionResponse?.Invoke(this, (parser as SoftwareVersionOTAParser)?.SoftwareVersion ?? "null");
                break;
            case MsgIds.BATTERY_TYPE:
                BatteryTypeResponse?.Invoke(this, (parser as BatteryTypeParser)!);
                break;
            case MsgIds.AMBIENT_MODE_UPDATED:
                AmbientEnabledUpdateResponse?.Invoke(this, (parser as AmbientModeUpdateParser)?.Enabled ?? false);
                break;
            case MsgIds.DEBUG_BUILD_INFO:
                BuildStringResponse?.Invoke(this, (parser as DebugBuildInfoParser)?.BuildString ?? "null");
                break;
            case MsgIds.DEBUG_GET_ALL_DATA:
                GetAllDataResponse?.Invoke(this, (parser as DebugGetAllDataParser)!);
                break;
            case MsgIds.DEBUG_SERIAL_NUMBER:
                SerialNumberResponse?.Invoke(this, (parser as DebugSerialNumberParser)!);
                break;
            case MsgIds.CRADLE_SERIAL_NUMBER:
                CradleSerialNumberResponse?.Invoke(this, (parser as CradleSerialNumberParser)!);
                break;
            case MsgIds.EXTENDED_STATUS_UPDATED:
                BaseUpdate?.Invoke(this, (parser as IBasicStatusUpdate)!);
                ExtendedStatusUpdate?.Invoke(this, (parser as ExtendedStatusUpdateParser)!);
                break;
            case MsgIds.FIND_MY_EARBUDS_STOP:
                FindMyGearStopped?.Invoke(this, EventArgs.Empty);
                break;
            case MsgIds.CHECK_THE_FIT_OF_EARBUDS_RESULT:
                FitTestResult?.Invoke(this, (parser as FitTestParser)!);
                break;
            case MsgIds.RESP:
                GenericResponse?.Invoke(this, (parser as GenericResponseParser)!);
                break;
            case MsgIds.SELF_TEST:
                SelfTestResponse?.Invoke(this, (parser as SelfTestParser)!);
                break;
            case MsgIds.SET_TOUCHPAD_OTHER_OPTION:
                OtherOption?.Invoke(this, (parser as SetOtherOptionParser)!.OptionType);
                break;
            case MsgIds.STATUS_UPDATED:
                StatusUpdate?.Invoke(this, (parser as StatusUpdateParser)!);
                BaseUpdate?.Invoke(this, (parser as IBasicStatusUpdate)!);
                break;
            case MsgIds.USAGE_REPORT:
                UsageReport?.Invoke(this, (parser as UsageReportParser)!);
                break;
            case MsgIds.MUTE_EARBUD_STATUS_UPDATED:
                FindMyGearMuteUpdate?.Invoke(this, (parser as MuteUpdateParser)!);
                break;
            case MsgIds.NOISE_REDUCTION_MODE_UPDATE:
                AncEnabledUpdateResponse?.Invoke(this,
                    (parser as NoiseReductionModeUpdateParser)?.Enabled ?? false);
                break;
            case MsgIds.NOISE_CONTROLS_UPDATE:
                NoiseControlUpdateResponse?.Invoke(this,
                    (parser as NoiseControlUpdateParser)?.Mode ?? NoiseControlModes.Off);
                break;
            case MsgIds.DEBUG_SKU:
                DebugSkuUpdate?.Invoke(this, (parser as DebugSkuParser)!);
                break;
            case MsgIds.VOICE_WAKE_UP_EVENT:
                if (parser is VoiceWakeupEventParser { ResultCode: 1 })
                {
                    Log.Debug("SppMessageHandler: Voice wakeup event received");
                    EventDispatcher.Instance.Dispatch(Settings.Instance.BixbyRemapEvent);
                }
                break;
        }
    }

}