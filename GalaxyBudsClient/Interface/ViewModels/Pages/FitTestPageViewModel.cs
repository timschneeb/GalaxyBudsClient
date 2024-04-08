using System.ComponentModel;
using Avalonia.Controls;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Interface;
using ReactiveUI.Fody.Helpers;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class FitTestPageViewModel : SubPageViewModelBase
{
    public override Control CreateView() => new FitTestPage();
    public override string TitleKey => Keys.MainpageFitTest;
    
    [Reactive] public string? WarningText { set; get; }
    [Reactive] public bool IsActive { set; get; }
    [Reactive] public string? LeftStatus { set; get; }
    [Reactive] public string? RightStatus { set; get; }

    public FitTestPageViewModel()
    {
        SppMessageReceiver.Instance.BaseUpdate += OnStatusUpdated;
        SppMessageReceiver.Instance.FitTestResult += OnFitTestResultReceived;
        Loc.LanguageUpdated += OnLanguageUpdated;
        PropertyChanged += OnPropertyChanged;
    }

    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IsActive):
            {
                if (IsActive)
                {
                    LeftStatus = null;
                    RightStatus = null;
                }

                await BluetoothImpl.Instance.SendRequestAsync(MsgIds.CHECK_THE_FIT_OF_EARBUDS, IsActive);
                break;
            }
        }
    }
    
    private void OnLanguageUpdated()
    {
        if(DeviceMessageCache.Instance.BasicStatusUpdate is {} status)
            OnStatusUpdated(null, status);
    }

    private void OnStatusUpdated(object? sender, IBasicStatusUpdate e)
    {
        WarningText = e.WearState != LegacyWearStates.Both ? Strings.GftWarning : null;
    }
   
    private void OnFitTestResultReceived(object? sender, FitTestDecoder result)
    {
        _ = BluetoothImpl.Instance.SendRequestAsync(MsgIds.CHECK_THE_FIT_OF_EARBUDS, 0);

        IsActive = false;
        LeftStatus = result.Left.GetLocalizedDescription();
        RightStatus = result.Right.GetLocalizedDescription();
    }
    
    public override void OnNavigatedFrom()
    {
        // Stop when leaving the page
        IsActive = false;
        _ = BluetoothImpl.Instance.SendRequestAsync(MsgIds.CHECK_THE_FIT_OF_EARBUDS, 0);
    }
}


