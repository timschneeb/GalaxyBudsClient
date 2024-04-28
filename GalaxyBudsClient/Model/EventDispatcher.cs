using System;
using System.Runtime.Serialization;
using Avalonia.Threading;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Generated.Model.Attributes;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;

namespace GalaxyBudsClient.Model;

[CompiledEnum]
public enum Event
{
    [LocalizableDescription(Keys.EventNone)]
    None,
    [LocalizableDescription(Keys.EventAmbientToggle)]
    AmbientToggle,
    [LocalizableDescription(Keys.EventAmbientVolumeUp)]
    AmbientVolumeUp,
    [LocalizableDescription(Keys.EventAmbientVolumeDown)]
    AmbientVolumeDown,
    [LocalizableDescription(Keys.EventAncToggle)]
    AncToggle,
    [LocalizableDescription(Keys.EventAncSwitchSensitivity)]
    SwitchAncSensitivity,
    [LocalizableDescription(Keys.EventNcSwitchOne)]
    SwitchAncOne,
    [LocalizableDescription(Keys.EventEqToggle)]
    EqualizerToggle,
    [LocalizableDescription(Keys.EventEqSwitch)]
    EqualizerNextPreset,
    [LocalizableDescription(Keys.EventTouchLockToggle)]
    LockTouchpadToggle,
    [LocalizableDescription(Keys.EventStartStopFind)]
    StartStopFind,
    [LocalizableDescription(Keys.EventStartFind)]
    StartFind,
    [LocalizableDescription(Keys.EventStopFind)]
    StopFind,
    [LocalizableDescription(Keys.EventDoubleEdgeTouchToggle)]
    ToggleDoubleEdgeTouch,
    [LocalizableDescription(Keys.EventConversationToggle)]
    ToggleConversationDetect,
    [LocalizableDescription(Keys.EventParingMode)]
    PairingMode,
    [LocalizableDescription(Keys.EventManagerVisible)]
    ToggleManagerVisibility,
    [LocalizableDescription(Keys.EventDisplayBatteryPopup)]
    ShowBatteryPopup,
    [LocalizableDescription(Keys.EventMediaPlayPause)]
    TogglePlayPause,
    [LocalizableDescription(Keys.EventMediaPlay)]
    Play,
    [LocalizableDescription(Keys.EventMediaPause)]
    Pause,
    [LocalizableDescription(Keys.EventConnect)]
    Connect,
            
    /* INTERNAL */
    [IgnoreDataMember]
    UpdateTrayIcon,
    [IgnoreDataMember]
    SetNoiseControlState
}

    
public class EventDispatcher
{
    public static bool CheckTouchOptionEligibility(Event arg)
    {
        switch (arg)
        {
            case Event.AmbientToggle:
            case Event.AncToggle:
            case Event.LockTouchpadToggle:
            case Event.StartStopFind:
            case Event.StartFind:
            case Event.StopFind:
            case Event.Connect:
                return false;
            default:
                return true;
        }
    }
        
    public static bool CheckDeviceSupport(Event arg)
    {
        switch (arg)
        {
            case Event.AmbientToggle:
            case Event.AmbientVolumeUp:
            case Event.AmbientVolumeDown:
                return BluetoothImpl.Instance.DeviceSpec.Supports(Features.AmbientSound);
            case Event.AncToggle:
                return BluetoothImpl.Instance.DeviceSpec.Supports(Features.Anc);
            case Event.SwitchAncSensitivity:
                return BluetoothImpl.Instance.DeviceSpec.Supports(Features.Anc)
                       && BluetoothImpl.Instance.DeviceSpec.Supports(Features.AncNoiseReductionLevels);
            case Event.SwitchAncOne:
                return BluetoothImpl.Instance.DeviceSpec.Supports(Features.NoiseControlsWithOneEarbud);
            case Event.ToggleDoubleEdgeTouch:
                return BluetoothImpl.Instance.DeviceSpec.Supports(Features.DoubleTapVolume);
            case Event.ToggleConversationDetect:
                return BluetoothImpl.Instance.DeviceSpec.Supports(Features.DetectConversations);
            case Event.PairingMode:
                return BluetoothImpl.Instance.DeviceSpec.Supports(Features.PairingMode);
                
            /* INTERNAL */
            case Event.UpdateTrayIcon:
            case Event.SetNoiseControlState:
                return false;
        }

        return true;
    }

    public event Action<Event, object?>? EventReceived;

    public void Dispatch(Event @event, object? extra = null)
    {
        Dispatcher.UIThread.Post(() => EventReceived?.Invoke(@event, extra));
    }
            
    #region Singleton
    private static readonly object Padlock = new();
    private static EventDispatcher? _instance;
    public static EventDispatcher Instance
    {
        get
        {
            lock (Padlock)
            {
                return _instance ??= new EventDispatcher();
            }
        }
    }

    public static void Init()
    {
        lock (Padlock)
        { 
            _instance ??= new EventDispatcher();
        }
    }
    #endregion
}