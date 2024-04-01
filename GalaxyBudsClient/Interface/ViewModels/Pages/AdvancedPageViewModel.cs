using System.ComponentModel;
using Avalonia.Controls;
using FluentIcons.Common;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Platform;
using ReactiveUI.Fody.Helpers;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class AdvancedPageViewModel : MainPageViewModelBase
{
    public override Control CreateView() => new AdvancedPage();

    public AdvancedPageViewModel()
    {
        SppMessageHandler.Instance.ExtendedStatusUpdate += OnExtendedStatusUpdate;
        PropertyChanged += OnPropertyChanged;
    }

    private void OnExtendedStatusUpdate(object? sender, ExtendedStatusUpdateParser e)
    {
        using var suppressor = SuppressChangeNotifications();
        IsSidetoneEnabled = e.SideToneEnabled;
        IsPassthroughEnabled = e.RelieveAmbient;
        IsSeamlessConnectionEnabled = e.SeamlessConnectionEnabled;
        IsCallpathControlEnabled = e.CallPathControl;
        IsExtraClearCallEnabled = e.ExtraClearCallSound;
    }

    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IsSeamlessConnectionEnabled):
                await BluetoothImpl.Instance.SendRequestAsync(MsgIds.SET_SEAMLESS_CONNECTION, !IsSeamlessConnectionEnabled);
                break;
            case nameof(IsPassthroughEnabled):
                await BluetoothImpl.Instance.SendRequestAsync(MsgIds.PASS_THROUGH, IsPassthroughEnabled);
                break;
            case nameof(IsSidetoneEnabled):
                await BluetoothImpl.Instance.SendRequestAsync(MsgIds.SET_SIDETONE, IsSidetoneEnabled);
                break; 
            case nameof(IsCallpathControlEnabled):
                // Boolean value has to be inverted
                await BluetoothImpl.Instance.SendRequestAsync(MsgIds.SET_CALL_PATH_CONTROL, !IsCallpathControlEnabled);
                break;
            case nameof(IsExtraClearCallEnabled):
                await BluetoothImpl.Instance.SendRequestAsync(MsgIds.EXTRA_CLEAR_SOUND_CALL, IsExtraClearCallEnabled);
                break;
        }
    }

    [Reactive] public bool IsSeamlessConnectionEnabled { set; get; }
    [Reactive] public bool IsPassthroughEnabled { set; get; }
    [Reactive] public bool IsSidetoneEnabled { set; get; }
    [Reactive] public bool IsCallpathControlEnabled { set; get; }
    [Reactive] public bool IsExtraClearCallEnabled { set; get; }

    public override string TitleKey => "mainpage_advanced";
    public override Symbol IconKey => Symbol.WrenchScrewdriver;
    public override bool ShowsInFooter => false;
}


