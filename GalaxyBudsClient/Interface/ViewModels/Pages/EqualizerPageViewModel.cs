using System.ComponentModel;
using Avalonia.Controls;
using FluentIcons.Common;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Message.Encoder;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public partial class EqualizerPageViewModel : MainPageViewModelBase
{
    public EqualizerPageViewModel()
    {
        SppMessageReceiver.Instance.ExtendedStatusUpdate += OnExtendedStatusUpdate;
        PropertyChanged += OnPropertyChanged;
    }

    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        switch (args.PropertyName)
        {
            case nameof(IsEqEnabled) or nameof(EqPreset):
                await BluetoothImpl.Instance.SendAsync(new SetEqualizerEncoder
                {
                    IsEnabled = IsEqEnabled,
                    Preset = EqPreset
                });
                EventDispatcher.Instance.Dispatch(Event.UpdateTrayIcon);
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
            // If EQ disabled, set to Dynamic (2) by default
            EqPreset = e.EqualizerMode == 0 ? 2 : e.EqualizerMode - 1;
        }
        
        StereoBalance = e.HearingEnhancements;
    }

    public override Control CreateView() => new EqualizerPage { DataContext = this };
    
    [Reactive] private bool _isEqEnabled;
    [Reactive] private int _eqPreset;
    [Reactive] private int _stereoBalance;

    public int MaximumEqPreset => 4;
    public override string TitleKey => Keys.EqHeader;
    public override Symbol IconKey => Symbol.DeviceEq;
    public override bool ShowsInFooter => false;
}
