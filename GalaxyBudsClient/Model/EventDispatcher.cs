using System;
using Avalonia.Threading;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;

namespace GalaxyBudsClient.Model;
    
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