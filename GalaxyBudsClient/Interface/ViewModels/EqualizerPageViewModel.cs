using System.ComponentModel;
using Avalonia.Controls;
using FluentIcons.Common;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using ReactiveUI.Fody.Helpers;

namespace GalaxyBudsClient.Interface.ViewModels;

public class EqualizerPageViewModel : MainPageViewModelBase
{
    public EqualizerPageViewModel()
    {
        SppMessageHandler.Instance.ExtendedStatusUpdate += OnExtendedStatusUpdate;
        PropertyChanged += OnPropertyChanged;
    }

    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        switch (args.PropertyName)
        {
            case nameof(IsEqEnabled) or nameof(EqPreset):
                await MessageComposer.SetEqualizer(IsEqEnabled, (EqPreset)EqPreset, false);
                break;
            case nameof(StereoBalance):
                await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.SET_HEARING_ENHANCEMENTS, (byte)StereoBalance);
                break;
        }
    }

    private void OnExtendedStatusUpdate(object? sender, ExtendedStatusUpdateParser e)
    {
        PropertyChanged -= OnPropertyChanged;
        IsEqEnabled = e.EqualizerEnabled;
        EqPreset = e.EqualizerMode;
        StereoBalance = e.HearingEnhancements;
        PropertyChanged += OnPropertyChanged;
    }

    public override Control CreateView() => new Pages.EqualizerPage();
    
    [Reactive] public bool IsEqEnabled { set; get; }
    [Reactive] public int EqPreset { set; get; }
    [Reactive] public int StereoBalance { set; get; }

    public override string TitleKey => "eq_header";
    public override Symbol IconKey => Symbol.DeviceEq;
    public override bool ShowsInFooter => false;
}
