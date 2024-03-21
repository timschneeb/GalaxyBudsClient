using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using FluentIcons.Common;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Platform;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class HomePageViewModel : MainPageViewModelBase
{
    public override Control CreateView() => new HomePage();
    public override string TitleKey => "mainpage_header";
    public override Symbol IconKey => Symbol.Home;
    public override bool ShowsInFooter => false;
    
    private readonly DispatcherTimer _refreshTimer = new();
    
    public HomePageViewModel()
    {
        // Low refresh rate since we rely on status updates as cues
        _refreshTimer.Interval = new TimeSpan(0, 0, 12);
        _refreshTimer.Tick += async (_, _) =>
        {
            if (BluetoothImpl.Instance.IsConnected)
                await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.DEBUG_GET_ALL_DATA);
        };
        
        BluetoothImpl.Instance.InvalidDataReceived += OnInvalidDataReceived;
        SppMessageHandler.Instance.StatusUpdate += OnStatusUpdateReceived;
        SppMessageHandler.Instance.AnyMessageReceived += (_, _) =>
        {
            /* A warning label is shown when a corrupted/invalid message has been received.
               As soon as we receive the next valid message, we can hide the warning. */
            // TODO SetWarning(false);
        };
    }

    private void OnInvalidDataReceived(object? sender, InvalidPacketException e)
    {
        // TODO display error message
        Dispatcher.UIThread.Post((async () =>
        {
           // ...
        }), DispatcherPriority.Render);
        
        _ = BluetoothImpl.Instance.DisconnectAsync()
            .ContinueWith(_ => Task.Delay(500))
            .ContinueWith(_ =>
            {
                // TODO hide corrupted data warning
                return BluetoothImpl.Instance.ConnectAsync();
            });
    }

    private void OnStatusUpdateReceived(object? sender, StatusUpdateParser e)
    {
        /* Status updates are only sent if something has changed.
           We use this knowledge to request updated debug data. */
        _ = BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.DEBUG_GET_ALL_DATA);
    }

    public override void OnNavigatedTo() => _refreshTimer.Start();
    public override void OnNavigatedFrom() => _refreshTimer.Stop();
}


