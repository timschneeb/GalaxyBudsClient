using System.ComponentModel;
using Avalonia.Controls;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public partial class BixbyRemapPageViewModel : SubPageViewModelBase
{
    public BixbyRemapPageViewModel()
    {
        SppMessageReceiver.Instance.ExtendedStatusUpdate += OnExtendedStatusUpdate;
        PropertyChanged += OnPropertyChanged;
    }
    
    private void OnExtendedStatusUpdate(object? sender, ExtendedStatusUpdateDecoder e)
    {
        using var suppressor = SuppressChangeNotifications();
        
        IsBixbyWakeUpEnabled = e.VoiceWakeUp;
        BixbyLanguage = (BixbyLanguages)e.VoiceWakeUpLang;
    }

    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IsBixbyWakeUpEnabled):
                await BluetoothImpl.Instance.SendRequestAsync(MsgIds.SET_VOICE_WAKE_UP, IsBixbyWakeUpEnabled);
                break;
            case nameof(BixbyLanguage):
                await BluetoothImpl.Instance.SendRequestAsync(MsgIds.VOICE_WAKE_UP_LANGUAGE, (byte)BixbyLanguage);
                break;
        }
    }
    
    [Reactive] private bool _isBixbyWakeUpEnabled;
    [Reactive] private BixbyLanguages _bixbyLanguage;

    public override string TitleKey => Keys.AdvBixbyRemap;
    public override Control CreateView() => new BixbyRemapPage { DataContext = this };
}

