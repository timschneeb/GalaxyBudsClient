using System.Runtime.Serialization;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Generated.Model.Attributes;
using GalaxyBudsClient.Model.Attributes;

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
