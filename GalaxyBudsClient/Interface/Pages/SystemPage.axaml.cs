using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.Services;
using GalaxyBudsClient.Interface.ViewModels.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Extensions;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;

namespace GalaxyBudsClient.Interface.Pages;

public partial class SystemPage : BasePage<SystemPageViewModel>
{
    public SystemPage()
    {
        InitializeComponent();
        AddHandler(SettingsExpanderItem.ClickEvent, OnSettingsItemClicked);
    }

    private void OnSettingsItemClicked(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not SettingsExpanderItem item)
            return;
        
        NavigationService.Instance.Navigate(item.Name);
    }

    private async void OnFactoryResetClicked(object? sender, RoutedEventArgs e)
    {
        var cd = new ContentDialog
        {
            Title = Loc.Resolve("factory_header"),
            Content = Loc.Resolve("factory_confirmation"),
            PrimaryButtonText = Loc.Resolve("continue_button"),
            SecondaryButtonText = Loc.Resolve("cancel"),
            DefaultButton = ContentDialogButton.Primary
        };

        cd.PrimaryButtonClick += OnFactoryResetConfirmed;
        cd.SecondaryButtonClick += (_, _) => cd.Hide();
        await cd.ShowAsync(MainWindow.Instance);
        cd.PrimaryButtonClick -= OnFactoryResetConfirmed;
    }

    private async void OnFactoryResetConfirmed(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var cancelToken = new CancellationTokenSource();
        sender.Title = Loc.Resolve("system_waiting_for_device");
        sender.IsPrimaryButtonEnabled = false;
        sender.IsSecondaryButtonEnabled = false;
        
        var defer = args.GetDeferral();
        SppMessageReceiver.Instance.ResetResponse += OnResetResponse;
        await BluetoothImpl.Instance.SendRequestAsync(MsgIds.RESET);
        
        // Wait for 10 seconds for the reset response
        await Task.Delay(10000, cancelToken.Token);
        
        SppMessageReceiver.Instance.ResetResponse -= OnResetResponse;
        // If the reset response was not received, show an error message
        OnResetResponse(null, -1);
        return;
        
        void OnResetResponse(object? s, int code)
        {
            cancelToken.Cancel();
            defer.Complete();
            
            if (code != 0)
            {
                var info = code == -1 ? Loc.Resolve("system_no_response") : code.ToString();
                
                _ = new MessageBox
                {
                    Title = Loc.Resolve("factory_error_title"),
                    Description = Loc.Resolve("factory_error").Replace("{0}", info)
                }.ShowAsync(MainWindow.Instance);
                return;
            }

            BluetoothImpl.Instance.UnregisterDevice();
        }
    }

    private async void OnPairingModeClicked(object? sender, RoutedEventArgs e)
    {
        await BluetoothImpl.Instance.SendRequestAsync(MsgIds.UNK_PAIRING_MODE);
        await new MessageBox
        {
            Title = Loc.Resolve("connlost_disconnected"),
            Description = Loc.Resolve("pairingmode_done")
        }.ShowAsync();
    }
    
    private async void OnSelfTestClicked(object? sender, RoutedEventArgs e)
    {
        await new SelfTestDialog().ExecuteTestAsync();
    }

    private async void OnSpatialSensorTestClicked(object? sender, RoutedEventArgs e)
    {
        var textBlock = new TextBlock
        {
            Text = Loc.Resolve("system_waiting_for_device"),
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 600
        };
        
        var cd = new ContentDialog
        {
            Title = Loc.Resolve("spatial_header"),
            Content = textBlock,
            CloseButtonText = Loc.Resolve("window_close")
        };

        using var sensorManager = new SpatialSensorManager();
        sensorManager.NewQuaternionReceived += OnNewQuaternionReceived;
        sensorManager.Attach();
        
        await cd.ShowAsync(MainWindow.Instance);
        
        sensorManager.NewQuaternionReceived -= OnNewQuaternionReceived;
        sensorManager.Detach();
        return;
        
        void OnNewQuaternionReceived(object? s, Quaternion quat)
        {
            var rpy = quat.ToRollPitchYaw();
            textBlock.Text = $"{Loc.Resolve("spatial_dump_quaternion")}\n" +
                             $"X={quat.X}\nY={quat.Y}\nZ={quat.Z}\nW={quat.W}\n\n" + 
                             $"{Loc.Resolve("spatial_dump_rpy")}\n" +
                             $"Roll={rpy[0]}\nPitch={rpy[1]}\nYaw={rpy[2]}\n";
        }
    }

    private async void OnTraceDumpDownloadClicked(object? sender, RoutedEventArgs e)
    {
        var result = await new QuestionBox()
        {
            Title = Loc.Resolve("system_trace_core_dump"),
            Description = Loc.Resolve("coredump_dl_note"),
            ButtonText = Loc.Resolve("continue_button")
        }.ShowAsync();
        
        if (!result)
            return;
        
        await new TraceDownloaderDialog().BeginDownload();
    }
}