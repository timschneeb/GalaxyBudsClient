using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Threading;
using FluentIcons.Common;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Message.Encoder;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;
using ReactiveUI.Fody.Helpers;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class TouchpadPageViewModel : MainPageViewModelBase
{
    public override Control CreateView() => new TouchpadPage();

    public TouchpadPageViewModel()
    {
        SppMessageHandler.Instance.ExtendedStatusUpdate += OnExtendedStatusUpdate;

        Settings.Instance.CustomActionLeft.PropertyChanged += (sender, args) => UpdateEditStates();
        Settings.Instance.CustomActionRight.PropertyChanged += (sender, args) => UpdateEditStates();

        BluetoothImpl.Instance.Connected += OnConnected;
        Loc.LanguageUpdated += OnLanguageUpdated;
        PropertyChanged += OnPropertyChanged;
    }

    public override void OnNavigatedTo() => UpdateEditStates();

    private void OnConnected(object? sender, EventArgs e)
    {
        UpdateTouchActions();
    }

    private void OnLanguageUpdated()
    {
        UpdateTouchActions();
        UpdateEditStates();
    }

    private void OnExtendedStatusUpdate(object? sender, ExtendedStatusUpdateParser e)
    {
        using var suppressor = SuppressChangeNotifications();

        LeftAction = e.TouchpadOptionL;
        RightAction = e.TouchpadOptionR;
        IsTouchpadLocked = e.TouchpadLock;
        IsDoubleTapVolumeEnabled = e.OutsideDoubleTap;

        if (BluetoothImpl.Instance.DeviceSpec.Supports(Features.AdvancedTouchLock))
        {
            IsSingleTapGestureEnabled = e.SingleTapOn;
            IsDoubleTapGestureEnabled = e.DoubleTapOn;
            IsTripleTapGestureEnabled = e.TripleTapOn;
            IsHoldGestureEnabled = e.TouchHoldOn;

            if (BluetoothImpl.Instance.DeviceSpec.Supports(Features.AdvancedTouchLockForCalls))
            {
                IsDoubleTapGestureForCallsEnabled = e.DoubleTapForCallOn;
                IsHoldGestureForCallsEnabled = e.TouchHoldOnForCallOn;
            }
        }
        else
        {
            IsSingleTapGestureEnabled = true;
            IsDoubleTapGestureEnabled = true;
            IsTripleTapGestureEnabled = true;
            IsHoldGestureEnabled = true;
        }

        if (BluetoothImpl.Instance.DeviceSpec.Supports(Features.NoiseControlModeDualSide))
        {
            NoiseControlCycleMode = e switch
            {
                { NoiseControlTouchLeftAnc: true, NoiseControlTouchLeftOff: true } => NoiseControlCycleModes.AncOff,
                { NoiseControlTouchLeftAmbient: true, NoiseControlTouchLeftOff: true } => NoiseControlCycleModes.AmbOff,
                { NoiseControlTouchLeftAmbient: true, NoiseControlTouchLeftAnc: true } => NoiseControlCycleModes.AncAmb,
                _ => NoiseControlCycleModes.Unknown
            };
            NoiseControlCycleModeRight = e switch
            {
                { NoiseControlTouchAnc: true, NoiseControlTouchOff: true } => NoiseControlCycleModes.AncOff,
                { NoiseControlTouchAmbient: true, NoiseControlTouchOff: true } => NoiseControlCycleModes.AmbOff,
                { NoiseControlTouchAmbient: true, NoiseControlTouchAnc: true } => NoiseControlCycleModes.AncAmb,
                _ => NoiseControlCycleModes.Unknown
            };
        }
        else
        {
            NoiseControlCycleMode = e switch
            {
                { NoiseControlTouchAnc: true, NoiseControlTouchOff: true } => NoiseControlCycleModes.AncOff,
                { NoiseControlTouchAmbient: true, NoiseControlTouchOff: true } => NoiseControlCycleModes.AmbOff,
                { NoiseControlTouchAmbient: true, NoiseControlTouchAnc: true } => NoiseControlCycleModes.AncAmb,
                _ => NoiseControlCycleModes.Unknown
            };
        }

        UpdateEditStates();
    }

    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        switch (args.PropertyName)
        {
            case nameof(IsSingleTapGestureEnabled):
            case nameof(IsDoubleTapGestureEnabled):
            case nameof(IsDoubleTapGestureForCallsEnabled):
            case nameof(IsTripleTapGestureEnabled):
            case nameof(IsHoldGestureEnabled):
            case nameof(IsHoldGestureForCallsEnabled):
            case nameof(IsTouchpadLocked):
                if (BluetoothImpl.Instance.DeviceSpec.Supports(Features.AdvancedTouchLock))
                {
                    await BluetoothImpl.Instance.SendAsync(LockTouchpadEncoder.Build(IsTouchpadLocked,
                        IsSingleTapGestureEnabled, IsDoubleTapGestureEnabled, IsTripleTapGestureEnabled,
                        IsHoldGestureEnabled, IsDoubleTapGestureForCallsEnabled, IsHoldGestureForCallsEnabled));
                }
                else
                {
                    await BluetoothImpl.Instance.SendRequestAsync(MsgIds.LOCK_TOUCHPAD,
                        IsTouchpadLocked);
                }

                break;
            case nameof(IsDoubleTapVolumeEnabled):
                await BluetoothImpl.Instance.SendRequestAsync(MsgIds.OUTSIDE_DOUBLE_TAP,
                    IsDoubleTapVolumeEnabled);
                break;
            case nameof(NoiseControlCycleMode) or nameof(NoiseControlCycleModeRight):
                if(BluetoothImpl.Instance.DeviceSpec.Supports(Features.NoiseControlModeDualSide))
                {
                    (byte, byte, byte) left = NoiseControlCycleMode switch
                    {
                        NoiseControlCycleModes.AncOff => (1, 0, 1),
                        NoiseControlCycleModes.AmbOff => (0, 1, 1),
                        NoiseControlCycleModes.AncAmb => (1, 1, 0),
                        _ => (0, 0, 0)
                    };
                    (byte, byte, byte) right = NoiseControlCycleModeRight switch
                    {
                        NoiseControlCycleModes.AncOff => (1, 0, 1),
                        NoiseControlCycleModes.AmbOff => (0, 1, 1),
                        NoiseControlCycleModes.AncAmb => (1, 1, 0),
                        _ => (0, 0, 0)
                    };

                    await BluetoothImpl.Instance.SendRequestAsync(
                        MsgIds.SET_TOUCH_AND_HOLD_NOISE_CONTROLS,
                        left.Item1, left.Item2, left.Item3, right.Item1, right.Item2, right.Item3);
                }
                else
                {
                    (byte, byte, byte) value = NoiseControlCycleMode switch
                    {
                        NoiseControlCycleModes.AncOff => (1, 0, 1),
                        NoiseControlCycleModes.AmbOff => (0, 1, 1),
                        NoiseControlCycleModes.AncAmb => (1, 1, 0),
                        _ => throw new ArgumentOutOfRangeException(nameof(NoiseControlCycleMode))
                    };

                    await BluetoothImpl.Instance.SendRequestAsync(
                        MsgIds.SET_TOUCH_AND_HOLD_NOISE_CONTROLS,
                        value.Item1, value.Item2, value.Item3);
                }

                break;
            case nameof(LeftAction):
            case nameof(RightAction):
                await MessageComposer.Touch.SetOptions(LeftAction, RightAction);
                UpdateEditStates();
                if (LeftAction == TouchOptions.NoiseControl || RightAction == TouchOptions.NoiseControl)
                    OnPropertyChanged(null, new PropertyChangedEventArgs(nameof(NoiseControlCycleMode)));
                break;
        }
    }

    protected override void OnEventReceived(Event e, object? arg)
    {
        Dispatcher.UIThread.Post(() =>
        {
            switch (e)
            {
                case Event.LockTouchpadToggle:
                    IsTouchpadLocked = !IsTouchpadLocked;
                    EventDispatcher.Instance.Dispatch(Event.UpdateTrayIcon);
                    break;
                case Event.ToggleDoubleEdgeTouch:
                    IsDoubleTapVolumeEnabled = !IsDoubleTapVolumeEnabled;
                    break;
            }
        });
    }

    private void UpdateEditStates()
    {
        IsNoiseControlCycleModeEditable = LeftAction == TouchOptions.NoiseControl || 
                                          (!BluetoothImpl.Instance.DeviceSpec.Supports(Features.NoiseControlModeDualSide) && RightAction == TouchOptions.NoiseControl);
        IsNoiseControlCycleModeRightEditable = RightAction == TouchOptions.NoiseControl;

        LeftControlCycleModeLabel = Loc.Resolve(BluetoothImpl.Instance.DeviceSpec.Supports(Features.NoiseControlModeDualSide) ? 
            "touchpad_noise_control_mode_l" : "touchpad_noise_control_mode");

        IsLeftCustomActionEditable = LeftAction == TouchOptions.OtherL;
        IsRightCustomActionEditable = RightAction == TouchOptions.OtherR;

        LeftActionDescription = IsLeftCustomActionEditable
            ? ActionAsString(Settings.Instance.CustomActionLeft)
            : Loc.Resolve("touchpad_default_action");
        RightActionDescription = IsRightCustomActionEditable
            ? ActionAsString(Settings.Instance.CustomActionRight)
            : Loc.Resolve("touchpad_default_action");
        return;

        string ActionAsString(ICustomAction action) =>
            $"{Loc.Resolve("touchoption_custom_prefix")} {new CustomAction(action.Action, action.Parameter)}";
    }

    private void UpdateTouchActions()
    {
        foreach (var device in (Devices[])Enum.GetValues(typeof(Devices)))
        {
            var table = BluetoothImpl.Instance.DeviceSpec.TouchMap.LookupTable;
            var actions = table
                .Where(pair => !pair.Key.IsMemberIgnored())
                .Select(TouchActionViewModel.FromKeyValuePair);

            /* Inject custom actions if appropriate */
            if (table.ContainsKey(TouchOptions.OtherL) && table.ContainsKey(TouchOptions.OtherR))
            {
                var key = device == Devices.L ? TouchOptions.OtherL : TouchOptions.OtherR;

                actions = actions.Concat(new[]
                {
                    TouchActionViewModel.FromKeyValuePair(table.First(x => x.Key == key))
                });
            }

            if (device == Devices.L)
                LeftActions = actions.Select(x => x.Key);
            else if (device == Devices.R)
                RightActions = actions.Select(x => x.Key);
        }
    }

    [Reactive] public IEnumerable<TouchOptions>? LeftActions { set; get; }
    [Reactive] public IEnumerable<TouchOptions>? RightActions { set; get; }
    [Reactive] public TouchOptions LeftAction { set; get; }
    [Reactive] public TouchOptions RightAction { set; get; }
    [Reactive] public NoiseControlCycleModes NoiseControlCycleMode { set; get; }
    [Reactive] public NoiseControlCycleModes NoiseControlCycleModeRight { set; get; }

    [Reactive] public bool IsTouchpadLocked { set; get; }
    [Reactive] public bool IsDoubleTapVolumeEnabled { set; get; }
    [Reactive] public bool IsSingleTapGestureEnabled { set; get; }
    [Reactive] public bool IsDoubleTapGestureEnabled { set; get; }
    [Reactive] public bool IsTripleTapGestureEnabled { set; get; }
    [Reactive] public bool IsHoldGestureEnabled { set; get; }
    [Reactive] public bool IsDoubleTapGestureForCallsEnabled { set; get; }
    [Reactive] public bool IsHoldGestureForCallsEnabled { set; get; }
    
    [Reactive] public bool IsNoiseControlCycleModeEditable { set; get; }
    [Reactive] public bool IsNoiseControlCycleModeRightEditable { set; get; }
    [Reactive] public string LeftControlCycleModeLabel { set; get; } = Loc.Resolve("touchpad_noise_control_mode");

    [Reactive] public bool IsLeftCustomActionEditable { set; get; }
    [Reactive] public bool IsRightCustomActionEditable { set; get; }
    [Reactive] public string? LeftActionDescription { set; get; }
    [Reactive] public string? RightActionDescription { set; get; }

    public override string TitleKey => "mainpage_touchpad";
    public override Symbol IconKey => Symbol.HandDraw;
    public override bool ShowsInFooter => false;
}