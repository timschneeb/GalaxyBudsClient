using System;

namespace GalaxyBudsClient.Model
{
    public class EventDispatcher
    {
        public enum Event
        {
            AmbientToggle,
            AmbientVolumeUp,
            AmbientVolumeDown,
            EqualizerToggle,
            EqualizerNextPreset
        }

        public event Action<Event, object?>? EventReceived;

        public void Dispatch(Event @event, object? extra = null)
        {
            EventReceived?.Invoke(@event, extra);
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