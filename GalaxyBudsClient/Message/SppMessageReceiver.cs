using System;
using Avalonia.Threading;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
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

    public event EventHandler<BaseMessageDecoder>? AnyMessageDecoded;
    public event EventHandler<AcknowledgementDecoder>? AcknowledgementResponse;
    public event EventHandler<GetFmmConfigDecoder>? GetFmmConfigResponse;
    public event EventHandler<byte>? SetFmmConfigResponse;
    public event EventHandler<int>? ResetResponse;
    public event EventHandler<BatteryTypeDecoder>? BatteryTypeResponse;
    public event EventHandler<bool>? AmbientEnabledUpdateResponse;
    public event EventHandler<bool>? AncEnabledUpdateResponse;
    public event EventHandler<NoiseControlModes>? NoiseControlUpdateResponse;
    public event EventHandler<string>? BuildStringResponse;
    public event EventHandler<DebugGetAllDataDecoder>? GetAllDataResponse;
    public event EventHandler<DebugSerialNumberDecoder>? SerialNumberResponse;
    public event EventHandler<CradleSerialNumberDecoder>? CradleSerialNumberResponse;
    public event EventHandler<SelfTestDecoder>? SelfTestResponse;
    public event EventHandler<TouchOptions>? OtherOption;
    public event EventHandler<ExtendedStatusUpdateDecoder>? ExtendedStatusUpdate;
    public event EventHandler<IBasicStatusUpdate>? BaseUpdate;
    public event EventHandler<StatusUpdateDecoder>? StatusUpdate;
    public event EventHandler<MuteUpdateDecoder>? FindMyGearMuteUpdate;
    public event EventHandler? FindMyGearStopped;
    public event EventHandler<FitTestDecoder>? FitTestResult;
    public event EventHandler<DebugSkuDecoder>? DebugSkuUpdate;

    public void MessageReceiver(object? sender, SppMessage e)
    {
        var decoder = e.CreateDecoder(); 
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            DispatchEventByMessage(e);
            if(decoder != null)
                DispatchEventByDecoder(decoder);
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
    
    private void DispatchEventByDecoder(BaseMessageDecoder decoder)
    {
        AnyMessageDecoded?.Invoke(this, decoder);
        
        switch (decoder)
        {
            case AcknowledgementDecoder p:
                AcknowledgementResponse?.Invoke(this, p);
                break;
            case GetFmmConfigDecoder p:
                GetFmmConfigResponse?.Invoke(this, p);
                break;
            case SetFmmConfigDecoder p:
                SetFmmConfigResponse?.Invoke(this, p.Response);
                break;
            case ResetResponseDecoder p:
                ResetResponse?.Invoke(this, p.ResultCode);
                break;
            case BatteryTypeDecoder p:
                BatteryTypeResponse?.Invoke(this, p);
                break;
            case AmbientModeUpdateDecoder p:
                AmbientEnabledUpdateResponse?.Invoke(this, p.Enabled);
                break;
            case DebugBuildInfoDecoder p:
                BuildStringResponse?.Invoke(this, p.BuildString ?? "null");
                break;
            case DebugGetAllDataDecoder p:
                GetAllDataResponse?.Invoke(this, p);
                break;
            case DebugSerialNumberDecoder p:
                SerialNumberResponse?.Invoke(this, p);
                break;
            case CradleSerialNumberDecoder p:
                CradleSerialNumberResponse?.Invoke(this, p);
                break;
            case ExtendedStatusUpdateDecoder p:
                var device = BluetoothImpl.Instance.Device.Current;
                if (device != null) 
                    device.DeviceColor = p.DeviceColor;
                
                BaseUpdate?.Invoke(this, p);
                ExtendedStatusUpdate?.Invoke(this, p);
                break;
            case FitTestDecoder p:
                FitTestResult?.Invoke(this, p);
                break;
            case SelfTestDecoder p:
                SelfTestResponse?.Invoke(this, p);
                break;
            case SetOtherOptionDecoder p:
                OtherOption?.Invoke(this, p.OptionType);
                break;
            case StatusUpdateDecoder p:
                StatusUpdate?.Invoke(this, p);
                BaseUpdate?.Invoke(this, p);
                break;
            case MuteUpdateDecoder p:
                FindMyGearMuteUpdate?.Invoke(this, p);
                break;
            case NoiseReductionModeUpdateDecoder p:
                AncEnabledUpdateResponse?.Invoke(this, p.Enabled);
                break;
            case NoiseControlUpdateDecoder p:
                NoiseControlUpdateResponse?.Invoke(this, p.Mode);
                break;
            case DebugSkuDecoder p:
                DebugSkuUpdate?.Invoke(this, p);
                break;
            case VoiceWakeupEventDecoder p:
                if (p.ResultCode == 1)
                {
                    Log.Debug("SppMessageHandler: Voice wakeup event received");
                    EventDispatcher.Instance.Dispatch(Settings.Data.BixbyRemapEvent);
                }
                break;
        }
    }

}