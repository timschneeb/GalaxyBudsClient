using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using FluentIcons.Common;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Message.Encoder;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using ReactiveUI;

using Serilog;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public partial class EqualizerPageViewModel : MainPageViewModelBase
{
    public EqualizerPageViewModel()
    {
        SppMessageReceiver.Instance.ExtendedStatusUpdate += OnExtendedStatusUpdate;
        SppMessageReceiver.Instance.AnyMessageDecoded += OnAnyMessageDecoded;
        PropertyChanged += OnPropertyChanged;
        SyncSelectedOption();
    }

    public override async void OnNavigatedTo()
    {
        // Ask the device for its stored custom EQ band values
        if (BluetoothImpl.Instance.DeviceSpec.Supports(Features.CustomEqualizer))
            await BluetoothImpl.Instance.SendRequestAsync(MsgIds.CUSTOM_EQUALIZE_RECV);
    }

    private void OnAnyMessageDecoded(object? sender, BaseMessageDecoder decoder)
    {
        // Custom EQ display is driven by the app's saved slots (see OnExtendedStatusUpdate and
        // ApplyEqOptionAsync), not the firmware read-back. The Buds4 Pro's CUSTOM_EQUALIZE_RECV
        // decode is unreliable (comes back flat) and would otherwise zero the sliders on launch,
        // so the firmware read-back is intentionally ignored — the app is the source of truth.
    }

    private async Task SendCustomEqAsync()
    {
        await BluetoothImpl.Instance.SendAsync(new SetCustomEqualizerEncoder
        {
            BandGains =
            [
                (sbyte)Band1, (sbyte)Band2, (sbyte)Band3,
                (sbyte)Band4, (sbyte)Band5, (sbyte)Band6,
                (sbyte)Band7, (sbyte)Band8, (sbyte)Band9
            ]
        });
        await BluetoothImpl.Instance.SendAsync(new SetEqualizerEncoder
        {
            IsEnabled = true,
            Preset = CustomEqPresetIndex
        });
        SaveBandsToActiveSlot();
        PersistCustomEq();
    }

    // Mirror the custom EQ state into the device's settings so it can be re-applied on the next
    // connect — the firmware does not retain the custom band table across power cycles. The active
    // slot's bands are also stored in CustomEqualizerBands so App.ReapplyCustomEqualizerAsync can
    // re-push them without needing to know about slots.
    private void PersistCustomEq()
    {
        var device = BluetoothImpl.Instance.Device.Current;
        if (device == null)
            return;

        device.CustomEqualizerBands =
            [Band1, Band2, Band3, Band4, Band5, Band6, Band7, Band8, Band9];
        device.CustomEqualizerEnabled = IsCustomEqEnabled;
        device.CustomEqualizerActiveSlot = _activeCustomSlot;
        device.CustomEqualizerSlots = CloneSlots();
    }

    // Translate a dropdown selection into the underlying EQ state and push exactly one update.
    // The state assignments run under _isSyncingEqOption so their own handlers don't re-send.
    private async Task ApplyEqOptionAsync(int option)
    {
        if (option < 0)
            return;

        var isCustom = option >= FirstCustomOptionIndex;

        _isSyncingEqOption = true;
        try
        {
            IsEqEnabled = option != OffOptionIndex;
            IsCustomEqEnabled = isCustom;
            if (option is >= FirstPresetOptionIndex and <= LastPresetOptionIndex)
                EqPreset = option - FirstPresetOptionIndex;
            if (isCustom)
            {
                _activeCustomSlot = Math.Clamp(option - FirstCustomOptionIndex, 0, CustomSlotCount - 1);
                // Move the band sliders to the selected slot's saved curve before pushing it
                LoadActiveSlotIntoBands();
            }
        }
        finally
        {
            _isSyncingEqOption = false;
        }

        if (isCustom)
        {
            await SendCustomEqAsync();
        }
        else
        {
            // Persist BEFORE awaiting the send: a NoiseControlUpdate landing in the await
            // window would otherwise re-push the still-flagged custom EQ over this preset.
            // DON'T overwrite the saved slot curves — a non-custom (or transient startup)
            // selection must never wipe stored custom EQs.
            PersistCustomDisabled();
            await BluetoothImpl.Instance.SendAsync(new SetEqualizerEncoder
            {
                IsEnabled = option != OffOptionIndex,
                Preset = EqPreset
            });
        }

        EventDispatcher.Instance.Dispatch(Event.UpdateTrayIcon);
    }

    // Mark custom EQ inactive without touching the persisted band/slot tables.
    private static void PersistCustomDisabled()
    {
        var device = BluetoothImpl.Instance.Device.Current;
        if (device != null)
            device.CustomEqualizerEnabled = false;
    }

    // Project the underlying EQ state back onto the dropdown (used after tray hotkeys and device
    // sync). Guarded so writing SelectedEqOption doesn't loop back into ApplyEqOptionAsync.
    private void SyncSelectedOption()
    {
        _isSyncingEqOption = true;
        try
        {
            var index = !IsEqEnabled
                ? OffOptionIndex
                : IsCustomEqEnabled
                    ? FirstCustomOptionIndex + Math.Clamp(_activeCustomSlot, 0, CustomSlotCount - 1)
                    : Math.Clamp(EqPreset + FirstPresetOptionIndex, FirstPresetOptionIndex, LastPresetOptionIndex);
            SelectedEqOption = index < EqOptions.Length ? EqOptions[index] : EqOptions[OffOptionIndex];
        }
        finally
        {
            _isSyncingEqOption = false;
        }
    }

    private static EqOption[] BuildEqOptions(bool supportsCustom)
    {
        var labels = new List<string>
        {
            EqOff, Strings.EqBass, Strings.EqSoft, Strings.EqDynamic, Strings.EqClear, Strings.EqTreble
        };
        if (supportsCustom)
            for (var slot = 1; slot <= CustomSlotCount; slot++)
                labels.Add($"Custom {slot}");
        var options = new EqOption[labels.Count];
        for (var i = 0; i < labels.Count; i++)
            options[i] = new EqOption(i, labels[i]);
        return options;
    }

    private static int[][] CreateEmptySlots()
    {
        var slots = new int[CustomSlotCount][];
        for (var i = 0; i < CustomSlotCount; i++)
            slots[i] = new int[9];
        return slots;
    }

    private int[][] CloneSlots()
    {
        var copy = new int[CustomSlotCount][];
        for (var i = 0; i < CustomSlotCount; i++)
            copy[i] = (int[])_customSlots[i].Clone();
        return copy;
    }

    private void LoadCustomSlotsFromDevice()
    {
        _customSlots = CreateEmptySlots();
        _activeCustomSlot = 0;

        var device = BluetoothImpl.Instance.Device.Current;
        if (device == null)
            return;

        _activeCustomSlot = Math.Clamp(device.CustomEqualizerActiveSlot, 0, CustomSlotCount - 1);
        var saved = device.CustomEqualizerSlots;
        if (saved == null)
            return;

        for (var i = 0; i < CustomSlotCount && i < saved.Length; i++)
            if (saved[i] is { Length: 9 })
                _customSlots[i] = (int[])saved[i].Clone();
    }

    private void LoadActiveSlotIntoBands()
    {
        var bands = _customSlots[_activeCustomSlot];
        Band1 = bands[0]; Band2 = bands[1]; Band3 = bands[2];
        Band4 = bands[3]; Band5 = bands[4]; Band6 = bands[5];
        Band7 = bands[6]; Band8 = bands[7]; Band9 = bands[8];
    }

    private void SaveBandsToActiveSlot()
    {
        _customSlots[_activeCustomSlot] =
            [Band1, Band2, Band3, Band4, Band5, Band6, Band7, Band8, Band9];
    }

    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        // async void: an unobserved exception from a BT send would crash the app — guard the handler.
        try
        {
        switch (args.PropertyName)
        {
            // The EQ dropdown (Off / presets / Custom) is the single user-facing control. Selecting
            // an entry drives the underlying IsEqEnabled / EqPreset / IsCustomEqEnabled state, which
            // is also moved by tray hotkeys and device sync — see ApplyEqOptionAsync/SyncSelectedOption.
            case nameof(SelectedEqOption):
                if (_isSyncingEqOption)
                    break;
                await ApplyEqOptionAsync(SelectedEqOption?.Index ?? -1);
                break;
            case nameof(EqPreset):
                if (_isSyncingEqOption)
                    break;
                // Guard the flip: an unguarded set re-enters this handler and double-sends
                _isSyncingEqOption = true;
                try { IsCustomEqEnabled = false; }
                finally { _isSyncingEqOption = false; }
                PersistCustomDisabled();
                await BluetoothImpl.Instance.SendAsync(new SetEqualizerEncoder
                {
                    IsEnabled = IsEqEnabled,
                    Preset = EqPreset
                });
                SyncSelectedOption();
                EventDispatcher.Instance.Dispatch(Event.UpdateTrayIcon);
                break;
            case nameof(IsEqEnabled):
                if (_isSyncingEqOption)
                    break;
                if (!IsEqEnabled)
                {
                    // Guarded flip: unguarded set re-enters this handler and double-sends
                    _isSyncingEqOption = true;
                    try { IsCustomEqEnabled = false; }
                    finally { _isSyncingEqOption = false; }
                    PersistCustomDisabled();
                }
                // When custom EQ just switched the EQ on, its own handler sends the messages
                if (!(IsEqEnabled && IsCustomEqEnabled))
                {
                    await BluetoothImpl.Instance.SendAsync(new SetEqualizerEncoder
                    {
                        IsEnabled = IsEqEnabled,
                        Preset = EqPreset
                    });
                }
                SyncSelectedOption();
                EventDispatcher.Instance.Dispatch(Event.UpdateTrayIcon);
                break;
            case nameof(IsCustomEqEnabled):
                if (_isSyncingEqOption)
                    break;
                if (IsCustomEqEnabled)
                {
                    IsEqEnabled = true;
                    await SendCustomEqAsync();
                }
                else
                {
                    await BluetoothImpl.Instance.SendAsync(new SetEqualizerEncoder
                    {
                        IsEnabled = IsEqEnabled,
                        Preset = EqPreset
                    });
                    PersistCustomDisabled();
                }
                SyncSelectedOption();
                break;
            case nameof(Band1) or nameof(Band2) or nameof(Band3) or
                nameof(Band4) or nameof(Band5) or nameof(Band6) or
                nameof(Band7) or nameof(Band8) or nameof(Band9):
                // Loading a slot's curve into the sliders sets these under the sync guard; don't
                // treat that programmatic change as a user edit to push back.
                if (_isSyncingEqOption || !IsCustomEqEnabled)
                    break;
                // The vertical sliders update per drag-tick; only send the settled values
                _bandDebounce?.Cancel();
                _bandDebounce?.Dispose();
                _bandDebounce = new CancellationTokenSource();
                try
                {
                    await Task.Delay(250, _bandDebounce.Token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                await SendCustomEqAsync();
                break;
            case nameof(StereoBalance):
                await BluetoothImpl.Instance.SendRequestAsync(MsgIds.SET_HEARING_ENHANCEMENTS, (byte)StereoBalance);
                break;
        }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Equalizer property-change handler failed");
        }
    }

    protected override void OnEventReceived(Event type, object? parameter)
    {
        switch (type)
        {
            case Event.EqualizerToggle:
                IsEqEnabled = !IsEqEnabled;
                EventDispatcher.Instance.Dispatch(Event.UpdateTrayIcon);
                break;
            case Event.EqualizerNextPreset:
            {
                IsEqEnabled = true;
                EqPreset++;
                if (EqPreset > MaximumEqPreset)
                {
                    EqPreset = 0;
                }
                break;
            }
        }
    }

    private void OnExtendedStatusUpdate(object? sender, ExtendedStatusUpdateDecoder e)
    {
        using (SuppressChangeNotifications())
        {
            if (BluetoothImpl.Instance.CurrentModel == Models.Buds)
            {
                IsEqEnabled = e.EqualizerEnabled;

                var preset = e.EqualizerMode;
                if (preset > MaximumEqPreset)
                {
                    /* 0 - 4: regular presets, 5 - 9: presets used when Dolby Atmos is enabled on the phone
                       There is no audible difference. */
                    preset -= 5;
                }

                EqPreset = preset;
                IsCustomEqEnabled = false;
            }
            else
            {
                IsEqEnabled = e.EqualizerMode != 0;
                IsCustomEqEnabled = e.EqualizerMode == CustomEqPresetIndex + 1;
                // If EQ disabled, set to Dynamic (2) by default; keep the preset
                // untouched while the custom preset is active (its index is out of range)
                if (e.EqualizerMode == 0)
                    EqPreset = 2;
                else if (!IsCustomEqEnabled)
                    EqPreset = e.EqualizerMode - 1;
            }

            StereoBalance = e.HearingEnhancements;
        }

        // SuppressChangeNotifications drops (not defers) PropertyChanged, so the band-slider
        // enable state would go stale; re-raise under the sync guard so nothing re-sends.
        _isSyncingEqOption = true;
        try { this.RaisePropertyChanged(nameof(IsCustomEqEnabled)); }
        finally { _isSyncingEqOption = false; }

        // Phone-side preset changes arrive here without going through ApplyEqOptionAsync;
        // keep the persisted re-push flag in sync so we don't override the phone's choice.
        if (BluetoothImpl.Instance.Device.Current is { } dev)
            dev.CustomEqualizerEnabled = IsCustomEqEnabled;

        // Load this device's saved custom slots so the dropdown can map to the active one
        LoadCustomSlotsFromDevice();

        // If custom is active, show the saved active-slot curve in the sliders (guarded so it
        // updates the UI without re-sending). The app's slots are authoritative, not the firmware.
        if (IsCustomEqEnabled)
        {
            _isSyncingEqOption = true;
            try { LoadActiveSlotIntoBands(); }
            finally { _isSyncingEqOption = false; }
        }

        // Only offer the Custom entries on devices whose firmware supports it, then reflect the
        // device's current EQ state in the dropdown without re-sending it.
        EqOptions = BuildEqOptions(BluetoothImpl.Instance.DeviceSpec.Supports(Features.CustomEqualizer));
        SyncSelectedOption();
    }

    public override Control CreateView() => new EqualizerPage { DataContext = this };
    
    [Reactive] private bool _isEqEnabled;
    [Reactive] private int _eqPreset;
    [Reactive] private int _stereoBalance;
    private CancellationTokenSource? _bandDebounce;

    // Single EQ dropdown: index 0 = Off, 1..5 = presets (Bass/Soft/Dynamic/Clear/Treble),
    // 6.. = Custom slots (only present when the device supports custom EQ). The firmware has a
    // single custom band table, so the slots are stored app-side and the selected one is pushed
    // into that single table.
    private const string EqOff = "Off";
    private const int OffOptionIndex = 0;
    private const int FirstPresetOptionIndex = 1;
    private const int LastPresetOptionIndex = 5;
    private const int FirstCustomOptionIndex = 6;
    private const int CustomSlotCount = 3;

    [Reactive] private EqOption[] _eqOptions = BuildEqOptions(false);
    [Reactive] private EqOption? _selectedEqOption;
    private bool _isSyncingEqOption;

    // The dropdown items carry an explicit Index so selection is identified by index, not by the
    // (possibly duplicated/empty in some locales) display string. Record value-equality makes the
    // ComboBox's SelectedValue match the right item even if two labels were ever equal.
    public sealed record EqOption(int Index, string Label)
    {
        public override string ToString() => Label;
    }
    private int _activeCustomSlot;
    private int[][] _customSlots = CreateEmptySlots();

    [Reactive] private bool _isCustomEqEnabled;
    [Reactive] private int _band1;
    [Reactive] private int _band2;
    [Reactive] private int _band3;
    [Reactive] private int _band4;
    [Reactive] private int _band5;
    [Reactive] private int _band6;
    [Reactive] private int _band7;
    [Reactive] private int _band8;
    [Reactive] private int _band9;

    public int MaximumEqPreset => 4;
    // Samsung preset index 6 = custom; the EQUALIZER message carries index + 1 (7)
    public int CustomEqPresetIndex => 6;
    public override string TitleKey => Keys.EqHeader;
    public override Symbol IconKey => Symbol.DeviceEq;
    public override bool ShowsInFooter => false;
}
