using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Controls;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.Services;
using GalaxyBudsClient.Interface.ViewModels.Pages;
using GalaxyBudsClient.Platform;

namespace GalaxyBudsClient.Interface.Pages;

public partial class SettingsPage : BasePage<SettingsPageViewModel>
{
    public SettingsPage()
    {
        InitializeComponent();
        DevSettings.AddHandler(SettingsExpander.ClickEvent, OnDevSettingsClicked);
        TrayIcon.AddHandler(SettingsSwitchItem.IsCheckedChangedEvent, OnIsTrayIconEnabledChanged);
    }

    private void OnIsTrayIconEnabledChanged(object? sender, RoutedEventArgs e)
    {
        // If the tray icon is disabled, also disable auto start
        if (TrayIcon.IsChecked == false && ViewModel?.IsAutoStartEnabled == true)
        {
            ViewModel.IsAutoStartEnabled = false;
        }
    }

    private static async void OnDevSettingsClicked(object? sender, RoutedEventArgs e)
    {
        await new DeveloperOptionsDialog().ShowAsync(true);
    }
    
    public void OnManageDevicesClicked(object? sender, RoutedEventArgs e)
    {
        NavigationService.Instance.Navigate(typeof(DevicesPageViewModel));
    }

    public async void OnApplyMultipointClicked(object? sender, RoutedEventArgs e)
    {
        /* The sequence disconnects and reconnects the earbuds, so keep it from being started twice */
        if (ViewModel != null)
            ViewModel.CanApplyMultipoint = false;

        var success = await MultipointPatcher.ApplyAsync();
        ViewModel?.RefreshMultipointState();

        await new MessageBox
        {
            Title = Strings.SettingsMultipointHeader,
            Description = success ? Strings.SettingsMultipointApplied : Strings.SettingsMultipointFailed
        }.ShowAsync();
    }
}