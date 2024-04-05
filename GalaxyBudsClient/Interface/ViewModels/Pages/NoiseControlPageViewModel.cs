using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Threading;
using FluentIcons.Common;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using ReactiveUI.Fody.Helpers;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class NoiseControlPageViewModel : MainPageViewModelBase
{
    public override Control CreateView() => new NoiseControlPage();
    
    // TODO: test with Buds2Pro, switches seem buggy
    public NoiseControlPageViewModel()
    {
        SppMessageReceiver.Instance.ExtendedStatusUpdate += OnExtendedStatusUpdate;
        SppMessageReceiver.Instance.AncEnabledUpdateResponse += (_, enabled) =>
        {
            using var suppressor = SuppressChangeNotifications();
            IsAncEnabled = enabled;
        };
        SppMessageReceiver.Instance.AmbientEnabledUpdateResponse += (_, enabled) =>
        {
            using var suppressor = SuppressChangeNotifications();
            IsAmbientSoundEnabled = enabled;
        };
        SppMessageReceiver.Instance.NoiseControlUpdateResponse += (_, mode) =>
        {
            using var suppressor = SuppressChangeNotifications();
            switch (mode)
            {
                case NoiseControlModes.Off:
                    IsAmbientSoundEnabled = false;
                    IsAncEnabled = false;
                    break;
                case NoiseControlModes.AmbientSound:
                    IsAmbientSoundEnabled = true;
                    break;
                case NoiseControlModes.NoiseReduction:
                    IsAncEnabled = true;
                    break;
            }
        };
        
        PropertyChanged += OnPropertyChanged;
    }
    
    private void OnExtendedStatusUpdate(object? sender, ExtendedStatusUpdateDecoder e)
    {
        using var suppressor = SuppressChangeNotifications();
        if (BluetoothImpl.Instance.DeviceSpec.Supports(Features.NoiseControl))
        {
            IsAmbientSoundEnabled = e.NoiseControlMode == NoiseControlModes.AmbientSound;
            IsAncEnabled = e.NoiseControlMode == NoiseControlModes.NoiseReduction;
        }
        else
        {
            IsAmbientSoundEnabled = e.AmbientSoundEnabled;
            IsAncEnabled = e.NoiseCancelling;
        }

        IsAncLevelHigh = e.NoiseReductionLevel == 1;
        IsAncWithOneEarbudAllowed = e.NoiseControlsWithOneEarbud;
        IsVoiceDetectEnabled = e.DetectConversations;
        VoiceDetectTimeout = e.DetectConversationsDuration switch
        {
            1 => VoiceDetectTimeouts.Sec10,
            2 => VoiceDetectTimeouts.Sec15,
            _ => VoiceDetectTimeouts.Sec5
        };
    }

    private async void SendNoiseControlState()
    {
        if (BluetoothImpl.Instance.DeviceSpec.Supports(Features.NoiseControl))
        {
            var mode = IsAmbientSoundEnabled ? NoiseControlModes.AmbientSound : 
                (IsAncEnabled ? NoiseControlModes.NoiseReduction : NoiseControlModes.Off);
            await BluetoothImpl.Instance.SendRequestAsync(MsgIds.NOISE_CONTROLS, (byte)mode);
        }
        else
        {
            if (BluetoothImpl.Instance.DeviceSpec.Supports(Features.AmbientSound))
                await BluetoothImpl.Instance.SendRequestAsync(MsgIds.SET_AMBIENT_MODE, IsAmbientSoundEnabled);
            if (BluetoothImpl.Instance.DeviceSpec.Supports(Features.Anc))
                await BluetoothImpl.Instance.SendRequestAsync(MsgIds.SET_NOISE_REDUCTION, IsAncEnabled);
        }
        EventDispatcher.Instance.Dispatch(Event.UpdateTrayIcon);
    }

    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        switch (args.PropertyName)
        {
            case nameof(IsAmbientSoundEnabled) or nameof(IsAncEnabled):
                if (IsAmbientSoundEnabled && IsAncEnabled && args.PropertyName == nameof(IsAmbientSoundEnabled))
                    IsAncEnabled = false;
                else if (IsAmbientSoundEnabled && IsAncEnabled && args.PropertyName == nameof(IsAncEnabled))
                    IsAmbientSoundEnabled = false;
                else
                    SendNoiseControlState();
                break;
            case nameof(IsAncLevelHigh):
                await BluetoothImpl.Instance.SendRequestAsync(MsgIds.NOISE_REDUCTION_LEVEL, IsAncLevelHigh);
                break;
            case nameof(IsAncWithOneEarbudAllowed):
                await BluetoothImpl.Instance.SendRequestAsync(MsgIds.SET_ANC_WITH_ONE_EARBUD, IsAncWithOneEarbudAllowed);
                break;
            case nameof(IsVoiceDetectEnabled):
                await BluetoothImpl.Instance.SendRequestAsync(MsgIds.SET_DETECT_CONVERSATIONS, IsVoiceDetectEnabled);
                break;
            case nameof(VoiceDetectTimeout):
                var timeout = VoiceDetectTimeout switch
                {
                    VoiceDetectTimeouts.Sec5 => 0,
                    VoiceDetectTimeouts.Sec10 => 1,
                    _ => 2
                };  
                await BluetoothImpl.Instance.SendRequestAsync(MsgIds.SET_DETECT_CONVERSATIONS_DURATION, (byte)timeout);
                break;
        }
    }
    
    protected override void OnEventReceived(Event e, object? arg)
    {
        Dispatcher.UIThread.Post(() =>
        {
            switch (e)
            {
                case Event.ToggleConversationDetect:
                    IsVoiceDetectEnabled = !IsVoiceDetectEnabled;
                    break;
                case Event.AncToggle:
                    IsAncEnabled = !IsAncEnabled;
                    break;
                case Event.SwitchAncSensitivity:
                    IsAncLevelHigh = !IsAncLevelHigh;
                    break;
                case Event.SwitchAncOne:
                    IsAncWithOneEarbudAllowed = !IsAncWithOneEarbudAllowed;
                    break;
                case Event.AmbientToggle:
                    IsAmbientSoundEnabled = !IsAmbientSoundEnabled;
                    break;
                case Event.SetNoiseControlState:
                    switch (arg)
                    {
                        case NoiseControlModes.Off:
                            IsAmbientSoundEnabled = false;
                            IsAncEnabled = false;
                            break;
                        case NoiseControlModes.AmbientSound:
                            IsAmbientSoundEnabled = true;
                            break;
                        case NoiseControlModes.NoiseReduction:
                            IsAncEnabled = true;
                            break;
                    }
                    break;
            }
        });
    }
    
    [Reactive] public bool IsAmbientSoundEnabled { set; get; }
    [Reactive] public bool IsAncEnabled { set; get; }
    [Reactive] public bool IsAncLevelHigh { set; get; }
    [Reactive] public bool IsAncWithOneEarbudAllowed { set; get; }
    [Reactive] public bool IsVoiceDetectEnabled { set; get; }
    [Reactive] public VoiceDetectTimeouts VoiceDetectTimeout { set; get; }

    public override string TitleKey => Keys.MainpageNoise;
    public override Symbol IconKey => Symbol.HeadphonesSoundWave;
    public override bool ShowsInFooter => false;
}