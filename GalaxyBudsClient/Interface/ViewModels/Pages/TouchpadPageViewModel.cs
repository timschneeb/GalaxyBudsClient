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
        
        BluetoothService.Instance.Connected += OnConnected;
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
        
        if(BluetoothService.Instance.DeviceSpec.Supports(Features.AdvancedTouchLock))
        {
            IsSingleTapGestureEnabled = e.SingleTapOn;
            IsDoubleTapGestureEnabled = e.DoubleTapOn;
            IsTripleTapGestureEnabled = e.TripleTapOn;
            IsHoldGestureEnabled = e.TouchHoldOn;
        }
        else
        {
            IsSingleTapGestureEnabled = true;
            IsDoubleTapGestureEnabled = true;
            IsTripleTapGestureEnabled = true;
            IsHoldGestureEnabled = true;
        }

        NoiseControlCycleMode = e switch
        {
            { NoiseControlTouchAnc: true, NoiseControlTouchOff: true } => NoiseControlCycleModes.AncOff,
            { NoiseControlTouchAmbient: true, NoiseControlTouchOff: true } => NoiseControlCycleModes.AmbOff,
            { NoiseControlTouchAmbient: true, NoiseControlTouchAnc: true } => NoiseControlCycleModes.AncAmb,
            _ => NoiseControlCycleModes.Unknown
        };

        UpdateEditStates();
    }

    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        switch (args.PropertyName)
        {
            case nameof(IsSingleTapGestureEnabled):
            case nameof(IsDoubleTapGestureEnabled):
            case nameof(IsTripleTapGestureEnabled):
            case nameof(IsHoldGestureEnabled):
            case nameof(IsTouchpadLocked):
                if (BluetoothService.Instance.DeviceSpec.Supports(Features.AdvancedTouchLock))
                {
                    await BluetoothService.Instance.SendAsync(LockTouchpadEncoder.Build(IsTouchpadLocked,
                        IsSingleTapGestureEnabled, IsDoubleTapGestureEnabled, IsTripleTapGestureEnabled,
                        IsHoldGestureEnabled));
                }
                else
                {
                    await BluetoothService.Instance.SendRequestAsync(SppMessage.MessageIds.LOCK_TOUCHPAD, IsTouchpadLocked);
                }
                break;
            case nameof(IsDoubleTapVolumeEnabled):
                await BluetoothService.Instance.SendRequestAsync(SppMessage.MessageIds.OUTSIDE_DOUBLE_TAP, IsDoubleTapVolumeEnabled);
                break;
            case nameof(NoiseControlCycleMode):
                (byte, byte, byte) value = NoiseControlCycleMode switch
                {
                    NoiseControlCycleModes.AncOff => (1, 0, 1),
                    NoiseControlCycleModes.AmbOff => (0, 1, 1),
                    NoiseControlCycleModes.AncAmb => (1, 1, 0),
                    _ => throw new ArgumentOutOfRangeException(nameof(NoiseControlCycleMode))
                };
                
                await BluetoothService.Instance.SendRequestAsync(
                    SppMessage.MessageIds.SET_TOUCH_AND_HOLD_NOISE_CONTROLS, 
                    value.Item1, value.Item2, value.Item3);
                break;
            case nameof(LeftAction):
            case nameof(RightAction):
                await MessageComposer.Touch.SetOptions(LeftAction, RightAction);
                UpdateEditStates();
                if(LeftAction == TouchOptions.NoiseControl || RightAction == TouchOptions.NoiseControl)
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

    public void SetCustomAction(Devices side, CustomAction action)
    {
        if (side == Devices.L)
        {
            Settings.Instance.CustomActionLeft.Action = action.Action;
            Settings.Instance.CustomActionLeft.Parameter = action.Parameter;
        }
        else if (side == Devices.R)
        {
            Settings.Instance.CustomActionRight.Action = action.Action;
            Settings.Instance.CustomActionRight.Parameter = action.Parameter;
        }

        UpdateEditStates();
    }
    
    private void UpdateEditStates()
    {
        IsNoiseControlCycleModeEditable = LeftAction == TouchOptions.NoiseControl || RightAction == TouchOptions.NoiseControl;
        IsLeftCustomActionEditable = LeftAction == TouchOptions.OtherL;
        IsRightCustomActionEditable = RightAction == TouchOptions.OtherR;

        LeftActionDescription = IsLeftCustomActionEditable ? 
            ActionAsString(Settings.Instance.CustomActionLeft) : Loc.Resolve("touchpad_default_action");
        RightActionDescription = IsRightCustomActionEditable ? 
            ActionAsString(Settings.Instance.CustomActionRight) : Loc.Resolve("touchpad_default_action");
        return;

        string ActionAsString(ICustomAction action) => 
            $"{Loc.Resolve("touchoption_custom_prefix")} {new CustomAction(action.Action, action.Parameter)}";
    }
    
    private void UpdateTouchActions()
    {
        foreach (var device in (Devices[])Enum.GetValues(typeof(Devices)))
        {
            var table = BluetoothService.Instance.DeviceSpec.TouchMap.LookupTable;
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
    [Reactive] public bool IsTouchpadLocked { set; get; }
    [Reactive] public bool IsDoubleTapVolumeEnabled { set; get; }
    [Reactive] public bool IsSingleTapGestureEnabled { set; get; }
    [Reactive] public bool IsDoubleTapGestureEnabled { set; get; }
    [Reactive] public bool IsTripleTapGestureEnabled { set; get; }
    [Reactive] public bool IsHoldGestureEnabled { set; get; }

    [Reactive] public bool IsNoiseControlCycleModeEditable { set; get; }
    [Reactive] public bool IsLeftCustomActionEditable { set; get; }
    [Reactive] public bool IsRightCustomActionEditable { set; get; }
    [Reactive] public string? LeftActionDescription { set; get; }
    [Reactive] public string? RightActionDescription { set; get; }

    public override string TitleKey => "mainpage_touchpad";
    public override Symbol IconKey => Symbol.HandDraw;
    public override bool ShowsInFooter => false;
}