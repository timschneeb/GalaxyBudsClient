using System.Threading;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Input;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.Services;
using GalaxyBudsClient.Interface.ViewModels.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Interface;
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
            DefaultButton = ContentDialogButton.Primary,
        };

        cd.PrimaryButtonClick += OnResetConfirmClick;
        cd.SecondaryButtonClick += (_, _) => cd.Hide();
        await cd.ShowAsync(MainWindow2.Instance);
        cd.PrimaryButtonClick -= OnResetConfirmClick;
    }

    private async void OnResetConfirmClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var cancelToken = new CancellationTokenSource();
        sender.Title = Loc.Resolve("factory_in_progress");
        sender.IsPrimaryButtonEnabled = false;
        sender.IsSecondaryButtonEnabled = false;
        
        var defer = args.GetDeferral();
        SppMessageHandler.Instance.ResetResponse += OnResetResponse;
        await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.RESET);
        
        // Wait for 10 seconds for the reset response
        await Task.Delay(10000, cancelToken.Token);
        
        SppMessageHandler.Instance.ResetResponse -= OnResetResponse;
        // If the reset response was not received, show an error message
        OnResetResponse(null, -1);
        return;
        
        void OnResetResponse(object? s, int code)
        {
            cancelToken.Cancel();
            defer.Complete();
            
            if (code != 0)
            {
                var info = code == -1 ? "No response from earbuds" : code.ToString();
                
                _ = new MessageBox
                {
                    Title = Loc.Resolve("factory_error_title"),
                    Description = Loc.Resolve("factory_error").Replace("{0}", info)
                }.ShowAsync(MainWindow2.Instance);
                return;
            }

            BluetoothImpl.Instance.UnregisterDevice();
        }
    }

    private async void OnPairingModeClicked(object? sender, RoutedEventArgs e)
    {
        await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.UNK_PAIRING_MODE);
        await BluetoothImpl.Instance.DisconnectAsync();
        await new MessageBox
        {
            Title = Loc.Resolve("connlost_disconnected"),
            Description = Loc.Resolve("pairingmode_done")
        }.ShowAsync();
    }

    private void OnTraceDumpDownloadClicked(object? sender, RoutedEventArgs e)
    {
        
    }

    private void OnSelfTestClicked(object? sender, RoutedEventArgs e)
    {
        
    }
}
