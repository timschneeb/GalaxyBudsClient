using System;
using System.ComponentModel;
using Avalonia.Controls;
using FluentIcons.Common;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;
using ReactiveUI.Fody.Helpers;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class FindMyBudsPageViewModel : MainPageViewModelBase
{
    public override Control CreateView() => new FindMyBudsPage();
    public override string TitleKey => "fmg_header";
    public override Symbol IconKey => Symbol.LocationLive;
    public override bool ShowsInFooter => false;
    
    [Reactive] public string? WarningText { set; get; }
    [Reactive] public bool IsSearching { set; get; }
    [Reactive] public bool IsLeftMuted { set; get; }
    [Reactive] public bool IsRightMuted { set; get; }
    
    public FindMyBudsPageViewModel()
    {
        SppMessageHandler.Instance.FindMyGearStopped += OnFindMyGearStopped;
        SppMessageHandler.Instance.FindMyGearMuteUpdate += OnFindMyGearMuteUpdated;
        SppMessageHandler.Instance.BaseUpdate += OnStatusUpdated;
        Loc.LanguageUpdated += OnLanguageUpdated;
        PropertyChanged += OnPropertyChanged;
    }

    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IsSearching):
            {
                var cmd = IsSearching ?
                    SppMessage.MessageIds.FIND_MY_EARBUDS_START : SppMessage.MessageIds.FIND_MY_EARBUDS_STOP;
                await BluetoothImpl.Instance.SendRequestAsync(cmd);
                break;
            }
            case nameof(IsLeftMuted) or nameof(IsRightMuted):
                await MessageComposer.FindMyGear.MuteEarbud(IsLeftMuted, IsRightMuted);
                break;
        }
    }

    protected override void OnEventReceived(Event @event, object? arg)
    {
        IsSearching = @event switch
        {
            Event.StartFind => true,
            Event.StopFind => false,
            Event.StartStopFind => !IsSearching,
            _ => IsSearching
        };
    }
    
    private void OnLanguageUpdated()
    {
        if(DeviceMessageCache.Instance.BasicStatusUpdate is {} status)
            OnStatusUpdated(null, status);
    }

    private void OnStatusUpdated(object? sender, IBasicStatusUpdate e)
    {
        WarningText = e.WearState switch
        {
            LegacyWearStates.Both => Loc.Resolve("fmg_warning_both"),
            LegacyWearStates.L => Loc.Resolve("fmg_warning_left"),
            LegacyWearStates.R => Loc.Resolve("fmg_warning_right"),
            _ => null
        };
    }

    private void OnFindMyGearMuteUpdated(object? sender, MuteUpdateParser e)
    {
        IsLeftMuted = e.LeftMuted;
        IsRightMuted = e.RightMuted;
    }

    private void OnFindMyGearStopped(object? sender, EventArgs e)
    {
        IsSearching = false;
    }
    
    public override void OnNavigatedFrom()
    {
        // Stop the search when leaving the page
        _ = BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.FIND_MY_EARBUDS_STOP);
        IsSearching = false;
    }
}


