using System.ComponentModel;
using Avalonia.Controls;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Interface;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public partial class FitTestPageViewModel : SubPageViewModelBase
{
    public override Control CreateView() => new FitTestPage { DataContext = this };
    public override string TitleKey => Keys.MainpageFitTest;
    
    [Reactive] private string? _warningText;
    [Reactive] private bool _isActive;
    [Reactive] private string? _leftStatus;
    [Reactive] private string? _rightStatus;

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

