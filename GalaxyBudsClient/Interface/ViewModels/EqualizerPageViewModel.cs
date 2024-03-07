using System.ComponentModel;
using Avalonia.Controls;
using FluentIcons.Common;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GalaxyBudsClient.Interface.ViewModels;

public class EqualizerPageViewModel : ViewModelBase, IMainPageViewModel
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

    private bool _isEqEnabled;
    public bool IsEqEnabled
    {
        get => _isEqEnabled;
        set => RaiseAndSetIfChanged(ref _isEqEnabled, value);
    }

    private int _eqPreset;

    public int EqPreset
    {
        get => _eqPreset;
        set => RaiseAndSetIfChanged(ref _eqPreset, value);
    }

    private int _stereoBalance;

    public int StereoBalance
    {
        get => _stereoBalance;
        set => RaiseAndSetIfChanged(ref _stereoBalance, value);
    }

    public string NavHeader => "Equalizer";
    public Symbol IconKey => Symbol.DeviceEq;
    public bool ShowsInFooter => false;
}
