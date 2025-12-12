using System;
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
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Interface;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public partial class FindMyBudsPageViewModel : MainPageViewModelBase
{
    public override Control CreateView() => new FindMyBudsPage { DataContext = this };
    public override string TitleKey => Keys.FmgHeader;
    public override Symbol IconKey => Symbol.LocationLive;
    public override bool ShowsInFooter => false;
    
    [Reactive] private string? _warningText;
    [Reactive] private bool _isSearching;
    [Reactive] private bool _isLeftMuted;
    [Reactive] private bool _isRightMuted;
    
    public FindMyBudsPageViewModel()
    {
        SppMessageReceiver.Instance.FindMyGearStopped += OnFindMyGearStopped;
        SppMessageReceiver.Instance.FindMyGearMuteUpdate += OnFindMyGearMuteUpdated;
        SppMessageReceiver.Instance.BaseUpdate += OnStatusUpdated;
        Loc.LanguageUpdated += OnLanguageUpdated;
        PropertyChanged += OnPropertyChanged;
    }

    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IsSearching):
            {
                MsgIds cmd;
                if (IsSearching && BluetoothImpl.Instance.DeviceSpec.Supports(Features.FmgRingWhileWearing))
                    cmd = MsgIds.FIND_MY_EARBUDS_ON_WEARING_START;
                else if (IsSearching)
                    cmd = MsgIds.FIND_MY_EARBUDS_START;
                else
                    cmd = MsgIds.FIND_MY_EARBUDS_STOP;
                await BluetoothImpl.Instance.SendRequestAsync(cmd);
                break;
            }
            case nameof(IsLeftMuted) or nameof(IsRightMuted):
                await BluetoothImpl.Instance.SendAsync(new FmgMuteEarbudEncoder
                {
                    IsLeftMuted = IsLeftMuted,
                    IsRightMuted = IsRightMuted
                });
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
            LegacyWearStates.Both => Strings.FmgWarningBoth,
            LegacyWearStates.L => Strings.FmgWarningLeft,
            LegacyWearStates.R => Strings.FmgWarningRight,
            _ => null
        };
    }

    private void OnFindMyGearMuteUpdated(object? sender, MuteUpdateDecoder e)
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
        _ = BluetoothImpl.Instance.SendRequestAsync(MsgIds.FIND_MY_EARBUDS_STOP);
        IsSearching = false;
    }
}

