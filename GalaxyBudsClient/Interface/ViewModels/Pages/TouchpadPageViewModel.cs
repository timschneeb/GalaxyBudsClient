using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Threading;
using FluentIcons.Common;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Parameter;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Message.Encoder;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Interface;
using Serilog;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public partial class TouchpadPageViewModel : MainPageViewModelBase
{
    public override Control CreateView() => new TouchpadPage { DataContext = this };

    public TouchpadPageViewModel()
    {
        SppMessageReceiver.Instance.ExtendedStatusUpdate += OnExtendedStatusUpdate;
        SppMessageReceiver.Instance.AcknowledgementResponse += (_, ack) =>
        {
            if (ack.Id == MsgIds.LOCK_TOUCHPAD &&
                ack.Parameters is LockTouchpadAckParameter { Lighting: not null } param)
            {
                // Cache the device-reported lighting style so the trailing
                // "earbuds control" byte can be echoed back unmodified
                _lightingStyle = param.Lighting.Value;
            }
        };
        Settings.TouchActionPropertyChanged += (_, _) => UpdateEditStates();

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

    private void OnExtendedStatusUpdate(object? sender, ExtendedStatusUpdateDecoder e)
    {
        using var suppressor = SuppressChangeNotifications();

        LeftAction = e.TouchpadOptionL;
        RightAction = e.TouchpadOptionR;
        if (BluetoothImpl.Instance.DeviceSpec.Supports(Features.TouchpadLock))
        {
            IsTouchpadLocked = e.TouchpadLock;
        }
        IsDoubleTapVolumeEnabled = e.OutsideDoubleTap;
        _quickLaunchAdvanced = e.QuickLaunchAdvanced;
        _gestureEchoBits = e.GestureEchoBits;

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
            NoiseControlCycleMode = ToCycleMode(e.NoiseControlTouchLeftAnc, e.NoiseControlTouchLeftAmbient,
                e.NoiseControlTouchLeftAdaptive, e.NoiseControlTouchLeftOff);
            NoiseControlCycleModeRight = ToCycleMode(e.NoiseControlTouchAnc, e.NoiseControlTouchAmbient,
                e.NoiseControlTouchAdaptive, e.NoiseControlTouchOff);
        }
        else
        {
            NoiseControlCycleMode = ToCycleMode(e.NoiseControlTouchAnc, e.NoiseControlTouchAmbient,
                false, e.NoiseControlTouchOff);
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
                    await BluetoothImpl.Instance.SendAsync(new LockTouchpadEncoder
                    {
                        LockAll = IsTouchpadLocked,
                        TapOn = IsSingleTapGestureEnabled,
                        DoubleTapOn = IsDoubleTapGestureEnabled,
                        TripleTapOn = IsTripleTapGestureEnabled,
                        HoldTapOn = IsHoldGestureEnabled,
                        DoubleTapCallOn = IsDoubleTapGestureForCallsEnabled,
                        HoldTapCallOn = IsHoldGestureForCallsEnabled,
                        GestureEchoBits = _gestureEchoBits,
                        Lighting = _lightingStyle,
                        QuickLaunchAdvanced = _quickLaunchAdvanced
                    });
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
                if (!IsValidCycleMode(NoiseControlCycleMode) ||
                    (BluetoothImpl.Instance.DeviceSpec.Supports(Features.NoiseControlModeDualSide) &&
                     !IsValidCycleMode(NoiseControlCycleModeRight)))
                {
                    Log.Warning("TouchpadPage: Not sending noise control cycle mode, " +
                                "one or both sides are in an unsupported state " +
                                "(left={Left}, right={Right})",
                        NoiseControlCycleMode, NoiseControlCycleModeRight);
                    break;
                }

                await BluetoothImpl.Instance.SendAsync(new TouchAndHoldNoiseControls
                {
                    CycleMode = NoiseControlCycleMode,
                    CycleModeRight = NoiseControlCycleModeRight
                });

                break;
            case nameof(LeftAction):
            case nameof(RightAction):
                await BluetoothImpl.Instance.SendAsync(new SetTouchOptionsEncoder
                {
                    LeftAction = LeftAction,
                    RightAction = RightAction
                });
                
                // Custom actions are only available on desktop
                if (!PlatformUtils.IsDesktop && (LeftAction == TouchOptions.OtherL || RightAction == TouchOptions.OtherR))
                {
                    _ = new MessageBox
                    {
                        Title = Strings.Error,
                        Description = Strings.FeatureUnsupportedPlatform
                    }.ShowAsync();
                }
                
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

    /// <summary>
    /// Maps the per-side noise control flags reported by the earbuds to a
    /// <see cref="NoiseControlCycleModes"/> bitmask value. Returns
    /// <see cref="NoiseControlCycleModes.Unknown"/> for configurations that
    /// cycle through fewer than two modes.
    /// </summary>
    private static NoiseControlCycleModes ToCycleMode(bool anc, bool ambient, bool adaptive, bool off)
    {
        var mask = (anc ? 8 : 0) | (ambient ? 1 : 0) | (adaptive ? 2 : 0) | (off ? 4 : 0);
        return System.Numerics.BitOperations.PopCount((uint)mask) >= 2
            ? (NoiseControlCycleModes)mask
            : NoiseControlCycleModes.Unknown;
    }

    /// <summary>
    /// Adaptive bit of the Buds3+ noise control cycle bitmask
    /// (Ambient=1, Adaptive=2, Off=4, ANC=8).
    /// </summary>
    private const NoiseControlCycleModes AdaptiveFlag = (NoiseControlCycleModes)0x02;

    /// <summary>
    /// Cycle modes offered in the touch-and-hold dropdowns. Adaptive
    /// combinations are only offered on devices that support
    /// <see cref="Features.NoiseControlAdaptive"/>.
    /// </summary>
    public NoiseControlCycleModes[] NoiseControlCycleModeOptions =>
        Enum.GetValues<NoiseControlCycleModes>()
            .Where(IsValidCycleMode)
            .ToArray();

    private static bool IsValidCycleMode(NoiseControlCycleModes mode)
    {
        if (mode == NoiseControlCycleModes.Unknown)
        {
            return false;
        }

        if (BluetoothImpl.Instance.DeviceSpec.Supports(Features.NoiseControlAdaptive))
        {
            return true;
        }

        // Below Buds4 the encoder only supports two-mode combinations
        // (exactly two of Ambient/ANC/Off; no Adaptive bit).
        return (mode & AdaptiveFlag) == 0 &&
               System.Numerics.BitOperations.PopCount((uint)mode) == 2;
    }

    private void UpdateEditStates()
    {
        IsNoiseControlCycleModeEditable = LeftAction == TouchOptions.NoiseControl || 
                                          (!BluetoothImpl.Instance.DeviceSpec.Supports(Features.NoiseControlModeDualSide) && RightAction == TouchOptions.NoiseControl);
        IsNoiseControlCycleModeRightEditable = RightAction == TouchOptions.NoiseControl;

        LeftControlCycleModeLabel = BluetoothImpl.Instance.DeviceSpec.Supports(Features.NoiseControlModeDualSide) ? 
            Strings.TouchpadNoiseControlModeL : Strings.TouchpadNoiseControlMode;

        // Custom actions are only available on desktop
        IsLeftCustomActionEditable = PlatformUtils.IsDesktop && LeftAction == TouchOptions.OtherL;
        IsRightCustomActionEditable = PlatformUtils.IsDesktop && RightAction == TouchOptions.OtherR;

        LeftActionDescription = IsLeftCustomActionEditable
            ? ActionAsString(Settings.Data.CustomActionLeft)
            : Strings.TouchpadDefaultAction;
        RightActionDescription = IsRightCustomActionEditable
            ? ActionAsString(Settings.Data.CustomActionRight)
            : Strings.TouchpadDefaultAction;
        return;

        string ActionAsString(TouchAction action) =>
            $"{Strings.TouchoptionCustomPrefix} {new CustomAction(action.Action, action.Parameter)}";
    }

    private void UpdateTouchActions()
    {
        foreach (var device in DevicesExtensions.GetValues())
        {
            var table = BluetoothImpl.Instance.DeviceSpec.TouchMap.LookupTable;
            var actions = table
                .Where(pair => !pair.Key.HasIgnoreDataMember())
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

    [Reactive] private IEnumerable<TouchOptions>? _leftActions;
    [Reactive] private IEnumerable<TouchOptions>? _rightActions;
    [Reactive] private TouchOptions _leftAction;
    [Reactive] private TouchOptions _rightAction;
    [Reactive] private NoiseControlCycleModes _noiseControlCycleMode;
    [Reactive] private NoiseControlCycleModes _noiseControlCycleModeRight;

    [Reactive] private bool _isTouchpadLocked;
    [Reactive] private bool _isDoubleTapVolumeEnabled;

    // Buds4 "earbuds control" trailing bytes; lighting is only reported in
    // acknowledgement responses, so the last known value is cached here.
    // 0x01 is the firmware default; sending 0 gets coerced back to 1 anyway.
    private byte _lightingStyle = 0x01;
    private byte _quickLaunchAdvanced;
    private byte _gestureEchoBits;
    [Reactive] private bool _isSingleTapGestureEnabled;
    [Reactive] private bool _isDoubleTapGestureEnabled;
    [Reactive] private bool _isTripleTapGestureEnabled;
    [Reactive] private bool _isHoldGestureEnabled;
    [Reactive] private bool _isDoubleTapGestureForCallsEnabled;
    [Reactive] private bool _isHoldGestureForCallsEnabled;
    
    [Reactive] private bool _isNoiseControlCycleModeEditable;
    [Reactive] private bool _isNoiseControlCycleModeRightEditable;
    [Reactive] private string _leftControlCycleModeLabel = Strings.TouchpadNoiseControlMode;

    [Reactive] private bool _isLeftCustomActionEditable;
    [Reactive] private bool _isRightCustomActionEditable;
    [Reactive] private string? _leftActionDescription;
    [Reactive] private string? _rightActionDescription;

    public override string TitleKey => Keys.MainpageTouchpad;
    public override Symbol IconKey => Symbol.HandDraw;
    public override bool ShowsInFooter => false;
}
