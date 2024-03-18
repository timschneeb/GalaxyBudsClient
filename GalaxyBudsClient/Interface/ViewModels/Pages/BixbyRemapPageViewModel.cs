using System.ComponentModel;
using Avalonia.Controls;
using FluentIcons.Common;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using ReactiveUI.Fody.Helpers;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class BixbyRemapPageViewModel : SubPageViewModelBase
{
    public BixbyRemapPageViewModel()
    {
        SppMessageHandler.Instance.ExtendedStatusUpdate += OnExtendedStatusUpdate;
        PropertyChanged += OnPropertyChanged;
    }

    // TODO refresh localization
    private void OnExtendedStatusUpdate(object? sender, ExtendedStatusUpdateParser e)
    {
        PropertyChanged -= OnPropertyChanged;
        IsBixbyWakeUpEnabled = e.VoiceWakeUp;
        BixbyLanguage = (BixbyLanguages)e.VoiceWakeUpLang;
        PropertyChanged += OnPropertyChanged;
    }

    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IsBixbyWakeUpEnabled):
                await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.SET_VOICE_WAKE_UP, IsBixbyWakeUpEnabled);
                break;
            case nameof(BixbyLanguage):
                await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.VOICE_WAKE_UP_LANGUAGE, (byte)BixbyLanguage);
                break;
        }
    }
    
    [Reactive] public bool IsBixbyWakeUpEnabled { set; get; }
    [Reactive] public BixbyLanguages BixbyLanguage { set; get; }

    public override string TitleKey => "adv_bixby_remap";
    public override Control CreateView() => new BixbyRemapPage();
}


