using System;
using Avalonia.Threading;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Utils;
using Serilog;

namespace GalaxyBudsClient.Message;

public class SppMessageReceiver
{
    private static SppMessageReceiver? _instance;
    private static readonly object SingletonPadlock = new();

    public static SppMessageReceiver Instance
    {
        get
        {
            lock (SingletonPadlock)
            {
                return _instance ??= new SppMessageReceiver();
            }
        }
    }

    public event EventHandler<BaseMessageParser>? AnyMessageDecoded;
    public event EventHandler<int>? ResetResponse;
    public event EventHandler<BatteryTypeParser>? BatteryTypeResponse;
    public event EventHandler<bool>? AmbientEnabledUpdateResponse;
    public event EventHandler<bool>? AncEnabledUpdateResponse;
    public event EventHandler<NoiseControlModes>? NoiseControlUpdateResponse;
    public event EventHandler<string>? BuildStringResponse;
    public event EventHandler<DebugGetAllDataParser>? GetAllDataResponse;
    public event EventHandler<DebugSerialNumberParser>? SerialNumberResponse;
    public event EventHandler<CradleSerialNumberParser>? CradleSerialNumberResponse;
    public event EventHandler<SelfTestParser>? SelfTestResponse;
    public event EventHandler<TouchOptions>? OtherOption;
    public event EventHandler<ExtendedStatusUpdateParser>? ExtendedStatusUpdate;
    public event EventHandler<IBasicStatusUpdate>? BaseUpdate;
    public event EventHandler<StatusUpdateParser>? StatusUpdate;
    public event EventHandler<MuteUpdateParser>? FindMyGearMuteUpdate;
    public event EventHandler? FindMyGearStopped;
    public event EventHandler<FitTestParser>? FitTestResult;
    public event EventHandler<DebugSkuParser>? DebugSkuUpdate;

    public void MessageReceiver(object? sender, SppMessage e)
    {
        var parser = e.BuildParser(); 
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            DispatchEventByMessage(e);
            if(parser != null)
                DispatchEventByDecoder(parser);
        });
    }

    private void DispatchEventByMessage(SppMessage msg)
    {
        switch (msg.Id)
        {
            case MsgIds.FIND_MY_EARBUDS_STOP:
                FindMyGearStopped?.Invoke(this, EventArgs.Empty);
                break;
        }
    }
    
    private void DispatchEventByDecoder(BaseMessageParser parser)
    {
        AnyMessageDecoded?.Invoke(this, parser);
        
        switch (parser)
        {
            case ResetResponseParser p:
                ResetResponse?.Invoke(this, p.ResultCode);
                break;
            case BatteryTypeParser p:
                BatteryTypeResponse?.Invoke(this, p);
                break;
            case AmbientModeUpdateParser p:
                AmbientEnabledUpdateResponse?.Invoke(this, p.Enabled);
                break;
            case DebugBuildInfoParser p:
                BuildStringResponse?.Invoke(this, p.BuildString ?? "null");
                break;
            case DebugGetAllDataParser p:
                GetAllDataResponse?.Invoke(this, p);
                break;
            case DebugSerialNumberParser p:
                SerialNumberResponse?.Invoke(this, p);
                break;
            case CradleSerialNumberParser p:
                CradleSerialNumberResponse?.Invoke(this, p);
                break;
            case ExtendedStatusUpdateParser p:
                Settings.Instance.RegisteredDevice.DeviceColor = p.DeviceColor;
                BaseUpdate?.Invoke(this, p);
                ExtendedStatusUpdate?.Invoke(this, p);
                break;
            case FitTestParser p:
                FitTestResult?.Invoke(this, p);
                break;
            case SelfTestParser p:
                SelfTestResponse?.Invoke(this, p);
                break;
            case SetOtherOptionParser p:
                OtherOption?.Invoke(this, p.OptionType);
                break;
            case StatusUpdateParser p:
                StatusUpdate?.Invoke(this, p);
                BaseUpdate?.Invoke(this, p);
                break;
            case MuteUpdateParser p:
                FindMyGearMuteUpdate?.Invoke(this, p);
                break;
            case NoiseReductionModeUpdateParser p:
                AncEnabledUpdateResponse?.Invoke(this, p.Enabled);
                break;
            case NoiseControlUpdateParser p:
                NoiseControlUpdateResponse?.Invoke(this, p.Mode);
                break;
            case DebugSkuParser p:
                DebugSkuUpdate?.Invoke(this, p);
                break;
            case VoiceWakeupEventParser p:
                if (p.ResultCode == 1)
                {
                    Log.Debug("SppMessageHandler: Voice wakeup event received");
                    EventDispatcher.Instance.Dispatch(Settings.Instance.BixbyRemapEvent);
                }
                break;
        }
    }

}