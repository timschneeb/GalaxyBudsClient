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
        
        SppMessageHandler.Instance.StatusUpdate += OnStatusUpdateReceived;
        BluetoothImpl.Instance.InvalidDataReceived += OnInvalidDataReceived;
    }

    private void OnInvalidDataReceived(object? sender, InvalidPacketException e)
    {
        // TODO display error message
        Dispatcher.UIThread.Post((async() =>
        {
            await BluetoothImpl.Instance.DisconnectAsync();
            //SetWarning(true, $"{Loc.Resolve("mainpage_corrupt_data")} ({e.ErrorCode})");
            await Task.Delay(500).ContinueWith(async(_)=>
            {
                await Task.Factory.StartNew(() => BluetoothImpl.Instance.ConnectAsync());
                // SetWarning(false);
            });
        }), DispatcherPriority.Render);
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


