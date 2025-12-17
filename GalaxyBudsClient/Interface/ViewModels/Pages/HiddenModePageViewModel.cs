using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Message.Encoder;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Interface;
using Serilog;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public partial class HiddenModePageViewModel : SubPageViewModelBase
{
    public override Control CreateView() => new HiddenModePage { DataContext = this };
    public override string TitleKey => Keys.SystemHiddenAtMode;
    
    [Reactive] private bool _isUartEnabled;
    [Reactive] private string _targetHeader = Strings.SystemWaitingForDevice;
    [Reactive] private string _targetDescription = Strings.PleaseWait;

    private bool _isInForeground;
    private bool _ignorePropertyEvents;
    private DevicesInverted? _previousMainConnection;
    
    public HiddenModePageViewModel()
    {
        PropertyChanged += OnPropertyChanged;
        Loc.LanguageUpdated += OnLanguageUpdated;
        SppMessageReceiver.Instance.BaseUpdate += OnBaseStatusUpdate;
        SppMessageReceiver.Instance.HiddenCmdData += OnHiddenCmdDataReceived;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IsUartEnabled) && !_ignorePropertyEvents)
        {
            SendHiddenData(IsUartEnabled ? "F011" : "F010");
        }
    }

    private void OnHiddenCmdDataReceived(object? sender, HiddenCmdDataDecoder e)
    {
        if(_isInForeground)
            Log.Debug("OnHiddenCmdDataReceived: {Content}", e.Content.ReplaceLineEndings().Replace("\n", "\\n"));
        
        // F012: Returns UART status as string
        _ignorePropertyEvents = true;
        if (e.Content.Contains("Get -> TRUE"))
            IsUartEnabled = true;
        else if (e.Content.Contains("Get -> FALSE"))
            IsUartEnabled = false;
        _ignorePropertyEvents = false;
        
        // F010/F011: Confirms UART status change
        string? uartMsg = null;
        if (_isInForeground && e.Content.Contains("uart_enable_set") &&
            e.Content.Contains("TRUE", StringComparison.OrdinalIgnoreCase))
            uartMsg = Strings.HiddenModeUartOnConfirm;
        else if (_isInForeground && e.Content.Contains("uart_enable_set") &&
                 e.Content.Contains("FALSE", StringComparison.OrdinalIgnoreCase))
            uartMsg = Strings.HiddenModeUartOffConfirm;

        if (uartMsg != null)
        {
            Dispatcher.UIThread.Post(() =>
            {
                _ = new MessageBox
                {
                    Title = Strings.HiddenModeUart,
                    Description = uartMsg,
                    ButtonText = Strings.Okay
                }.ShowAsync();
            });
        }
    }

    private void OnBaseStatusUpdate(object? sender, IBasicStatusUpdate e)
    {
        if (_previousMainConnection != e.MainConnection)
        {
            UpdateState();
            _previousMainConnection = e.MainConnection;
        }
    }

    private void UpdateState()
    {
        var host = DeviceMessageCache.Instance.BasicStatusUpdate?.MainConnection;
        if (host == DevicesInverted.L)
        {
            TargetHeader = string.Format(Strings.HiddenModeTarget, Strings.Left);
            TargetDescription = string.Format(Strings.HiddenModeTargetRDesc);
        }
        else if (host == DevicesInverted.R)
        {
            TargetHeader = string.Format(Strings.HiddenModeTarget, Strings.Right);
            TargetDescription = string.Format(Strings.HiddenModeTargetLDesc);
        }
        
        if(!_isInForeground)
            return;
        
        // Request current UART status
        SendHiddenMode(1);
        SendHiddenData("F012");
    }

    private void OnLanguageUpdated()
    {
        UpdateState();
    }

    public override void OnNavigatedTo()
    {
        BluetoothImpl.Instance.Connected += OnConnected;
        
        SendHiddenMode(1);

        _isInForeground = true;
        UpdateState();
        
        base.OnNavigatedTo();
    }
    
    private async void OnConnected(object? sender, EventArgs e)
    {
        await Task.Delay(300);
        SendHiddenMode(1);
    }

    public override void OnNavigatedFrom()
    {
        BluetoothImpl.Instance.Connected -= OnConnected;

        _isInForeground = false;
        SendHiddenMode(0);
        
        // Reconnect
        Task.Run(() =>
        {
            _ = BluetoothImpl.Instance.DisconnectAsync(true).ContinueWith(
                lastTask =>
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        Task.Run(() => { _ = BluetoothImpl.Instance.ConnectAsync(); });
                    });
                });
        });
        base.OnNavigatedFrom();
    }
    
    private async void SendHiddenData(string cmdId)
    {
        await BluetoothImpl.Instance.SendAsync(new HiddenCmdDataEncoder { CommandId = cmdId });
    }

    private async void SendHiddenMode(int mode)
    {
        /*
         * 0: Disable
         * 1: Enable for host device only
         * 2: Enable for both devices
         * 3: Enable for host device only (duplicate?)
         * 4: Enable for both devices (duplicate?)
         * 5: Enable for both devices and merge their responses into one message
         */
        Log.Debug("SendHiddenMode: Sending hidden mode: {Mode}", mode);
        await BluetoothImpl.Instance.SendRequestAsync(MsgIds.HIDDEN_CMD_MODE, (byte)mode);
    }
}

