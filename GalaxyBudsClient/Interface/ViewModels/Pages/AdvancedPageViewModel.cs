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
        IsSidetoneEnabled = e.SideToneEnabled;
        IsPassthroughEnabled = e.RelieveAmbient;
        IsSeamlessConnectionEnabled = e.SeamlessConnectionEnabled;
    }

    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IsSeamlessConnectionEnabled):
                await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.SET_SEAMLESS_CONNECTION, !IsSeamlessConnectionEnabled);
                break;
            case nameof(IsPassthroughEnabled):
                await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.PASS_THROUGH, IsPassthroughEnabled);
                break;
            case nameof(IsSidetoneEnabled):
                await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.SET_SIDETONE, IsSidetoneEnabled);
                break;
        }
    }

    [Reactive] public bool IsSeamlessConnectionEnabled { set; get; }
    [Reactive] public bool IsPassthroughEnabled { set; get; }
    [Reactive] public bool IsSidetoneEnabled { set; get; }
    
    public override string TitleKey => "mainpage_advanced";
    public override Symbol IconKey => Symbol.WrenchScrewdriver;
    public override bool ShowsInFooter => false;
}


