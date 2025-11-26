using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.Services;
using GalaxyBudsClient.Interface.ViewModels.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Extensions;

namespace GalaxyBudsClient.Interface.Pages;

public partial class SystemPage : BasePage<SystemPageViewModel>
{
    private readonly string[] _restrictedItems = ["Firmware", "UsageReport", "HiddenMode"];
    
    public SystemPage()
    {
        InitializeComponent();
        AddHandler(SettingsExpanderItem.ClickEvent, OnSettingsItemClicked);
    }

    private async void OnSettingsItemClicked(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not SettingsExpanderItem item)
            return;
        
        
        if (!_restrictedItems.Contains(item.Name) ||
            await Utils.Interface.Dialogs.RequireFullVersion())
        {
            NavigationService.Instance.Navigate(item.Name);
        }
    }

    private async void OnFactoryResetClicked(object? sender, RoutedEventArgs e)
    {
        var cd = new ContentDialog
        {
            Title = Strings.FactoryHeader,
            Content = Strings.FactoryConfirmation,
            PrimaryButtonText = Strings.ContinueButton,
            SecondaryButtonText = Strings.Cancel,
            DefaultButton = ContentDialogButton.Primary
        };

        cd.PrimaryButtonClick += OnFactoryResetConfirmed;
        cd.SecondaryButtonClick += (_, _) => cd.Hide();
        await cd.ShowAsync(TopLevel.GetTopLevel(MainView.Instance));
        cd.PrimaryButtonClick -= OnFactoryResetConfirmed;
    }

    private async void OnFactoryResetConfirmed(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var cancelToken = new CancellationTokenSource();
        sender.Title = Strings.SystemWaitingForDevice;
        sender.IsPrimaryButtonEnabled = false;
        sender.IsSecondaryButtonEnabled = false;
        
        var defer = args.GetDeferral();
        SppMessageReceiver.Instance.ResetResponse += OnResetResponse;
        await BluetoothImpl.Instance.SendRequestAsync(MsgIds.RESET);
        
        // Wait for 10 seconds for the reset response
        try
        {
            await Task.Delay(10000, cancelToken.Token);
        }
        catch(TaskCanceledException){}

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
                var info = code == -1 ? Strings.SystemNoResponse : code.ToString();
                
                _ = new MessageBox
                {
                    Title = Strings.FactoryErrorTitle,
                    Description = Strings.FactoryError.Replace("{0}", info)
                }.ShowAsync();
                return;
            }

            BluetoothImpl.Instance.UnregisterDevice();
        }
    }

    private async void OnPairingModeClicked(object? sender, RoutedEventArgs e)
    {
        if (!await Utils.Interface.Dialogs.RequireFullVersion())
            return;
        
        await BluetoothImpl.Instance.SendRequestAsync(MsgIds.PAIRING_MODE);
        await new MessageBox
        {
            Title = Strings.ConnlostDisconnected,
            Description = Strings.PairingmodeDone
        }.ShowAsync();
    }
    
    private async void OnSelfTestClicked(object? sender, RoutedEventArgs e)
    {
        if (!await Utils.Interface.Dialogs.RequireFullVersion())
            return;
        
        await new SelfTestDialog().ExecuteTestAsync();
    }

    private async void OnSpatialSensorTestClicked(object? sender, RoutedEventArgs e)
    {
        if (!await Utils.Interface.Dialogs.RequireFullVersion())
            return;
        
        var textBlock = new TextBlock
        {
            Text = Strings.SystemWaitingForDevice,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 600
        };
        
        var cd = new ContentDialog
        {
            Title = Strings.SpatialHeader,
            Content = textBlock,
            CloseButtonText = Strings.WindowClose
        };

        using var sensorManager = new SpatialSensorManager();
        sensorManager.NewQuaternionReceived += OnNewQuaternionReceived;
        sensorManager.Attach();
        
        await cd.ShowAsync(TopLevel.GetTopLevel(MainView.Instance));
        
        sensorManager.NewQuaternionReceived -= OnNewQuaternionReceived;
        sensorManager.Detach();
        return;
        
        void OnNewQuaternionReceived(object? s, Quaternion quat)
        {
            var (roll, pitch, yaw) = quat.ToRollPitchYaw();
            textBlock.Text = $"{Strings.SpatialDumpQuaternion}\n" +
                             $"X={quat.X}\nY={quat.Y}\nZ={quat.Z}\nW={quat.W}\n\n" + 
                             $"{Strings.SpatialDumpRpy}\n" +
                             $"Roll={roll}\nPitch={pitch}\nYaw={yaw}\n";
        }
    }

    private async void OnTraceDumpDownloadClicked(object? sender, RoutedEventArgs e)
    {
        if (!await Utils.Interface.Dialogs.RequireFullVersion())
            return;
        
        var result = await new QuestionBox
        {
            Title = Strings.SystemTraceCoreDump,
            Description = Strings.CoredumpDlNote,
            ButtonText = Strings.ContinueButton
        }.ShowAsync();
        
        if (!result)
            return;
        
        await new TraceDownloaderDialog().BeginDownload();
    }
}