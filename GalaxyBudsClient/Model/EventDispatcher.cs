using System;
using Avalonia.Threading;
using GalaxyBudsClient.Model.Attributes;

namespace GalaxyBudsClient.Model
{
    public class EventDispatcher
    {
        public enum Event
        {
            [LocalizedDescription("event_none")]
            None,
            [LocalizedDescription("event_ambient_toggle")]
            AmbientToggle,
            [LocalizedDescription("event_ambient_volume_up")]
            AmbientVolumeUp,
            [LocalizedDescription("event_ambient_volume_down")]
            AmbientVolumeDown,
            [LocalizedDescription("event_anc_toggle")]
            AncToggle,
            [LocalizedDescription("event_anc_switch_sensitivity")]
            SwitchAncSensitivity,
            [LocalizedDescription("event_eq_toggle")]
            EqualizerToggle,
            [LocalizedDescription("event_eq_switch")]
            EqualizerNextPreset,
            [LocalizedDescription("event_touch_lock_toggle")]
            LockTouchpadToggle,
            [LocalizedDescription("event_start_stop_find")]
            StartStopFind,
            [LocalizedDescription("event_start_find")]
            StartFind,
            [LocalizedDescription("event_stop_find")]
            StopFind,
            [LocalizedDescription("event_double_edge_touch_toggle")]
            ToggleDoubleEdgeTouch,
            [LocalizedDescription("event_conversation_toggle")]
            ToggleConversationDetect,
            [LocalizedDescription("event_paring_mode")]
            PairingMode,
            [LocalizedDescription("event_manager_visible")]
            ToggleManagerVisibility,
            [LocalizedDescription("event_display_battery_popup")]
            ShowBatteryPopup,
            [LocalizedDescription("event_media_play_pause")]
            TogglePlayPause,
            [LocalizedDescription("event_media_play")]
            Play,
            [LocalizedDescription("event_media_pause")]
            Pause,
        }

        public event Action<Event, object?>? EventReceived;

        public void Dispatch(Event @event, object? extra = null)
        {
            Dispatcher.UIThread.Post(() => EventReceived?.Invoke(@event, extra));
        }
            
        #region Singleton
        private static readonly object Padlock = new object();
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
}