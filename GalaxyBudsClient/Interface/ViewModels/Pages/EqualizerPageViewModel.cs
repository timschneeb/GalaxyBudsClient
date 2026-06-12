using System.ComponentModel;
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

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public partial class EqualizerPageViewModel : MainPageViewModelBase
{
    public EqualizerPageViewModel()
    {
        SppMessageReceiver.Instance.ExtendedStatusUpdate += OnExtendedStatusUpdate;
        SppMessageReceiver.Instance.AnyMessageDecoded += OnAnyMessageDecoded;
        PropertyChanged += OnPropertyChanged;
    }

    public override async void OnNavigatedTo()
    {
        // Ask the device for its stored custom EQ band values
        if (BluetoothImpl.Instance.DeviceSpec.Supports(Features.CustomEqualizer))
            await BluetoothImpl.Instance.SendRequestAsync(MsgIds.CUSTOM_EQUALIZE_RECV);
    }

    private void OnAnyMessageDecoded(object? sender, BaseMessageDecoder decoder)
    {
        if (decoder is not CustomEqualizerDataDecoder eq || eq.BandCount < 9)
            return;

        using var suppressor = SuppressChangeNotifications();
        Band1 = eq.CustomBands[0];
        Band2 = eq.CustomBands[1];
        Band3 = eq.CustomBands[2];
        Band4 = eq.CustomBands[3];
        Band5 = eq.CustomBands[4];
        Band6 = eq.CustomBands[5];
        Band7 = eq.CustomBands[6];
        Band8 = eq.CustomBands[7];
        Band9 = eq.CustomBands[8];
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
    }

    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        switch (args.PropertyName)
        {
            case nameof(IsEqEnabled) or nameof(EqPreset):
                IsCustomEqEnabled = false;
                await BluetoothImpl.Instance.SendAsync(new SetEqualizerEncoder
                {
                    IsEnabled = IsEqEnabled,
                    Preset = EqPreset
                });
                EventDispatcher.Instance.Dispatch(Event.UpdateTrayIcon);
                break;
            case nameof(IsCustomEqEnabled):
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
                }
                break;
            case nameof(Band1) or nameof(Band2) or nameof(Band3) or
                nameof(Band4) or nameof(Band5) or nameof(Band6) or
                nameof(Band7) or nameof(Band8) or nameof(Band9):
                if (IsCustomEqEnabled)
                    await SendCustomEqAsync();
                break;
            case nameof(StereoBalance):
                await BluetoothImpl.Instance.SendRequestAsync(MsgIds.SET_HEARING_ENHANCEMENTS, (byte)StereoBalance);
                break;
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
        using var suppressor = SuppressChangeNotifications();
        
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
        }
        else
        {
            IsEqEnabled = e.EqualizerMode != 0;
            IsCustomEqEnabled = e.EqualizerMode == CustomEqPresetIndex + 1;
            // If EQ disabled, set to Dynamic (2) by default; keep the preset slider
            // untouched while the custom preset is active (its index is out of range)
            if (e.EqualizerMode == 0)
                EqPreset = 2;
            else if (!IsCustomEqEnabled)
                EqPreset = e.EqualizerMode - 1;
        }
        
        StereoBalance = e.HearingEnhancements;
    }

    public override Control CreateView() => new EqualizerPage { DataContext = this };
    
    [Reactive] private bool _isEqEnabled;
    [Reactive] private int _eqPreset;
    [Reactive] private int _stereoBalance;
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
