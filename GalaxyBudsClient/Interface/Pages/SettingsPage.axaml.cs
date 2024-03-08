using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Interface.Controls;
using GalaxyBudsClient.Interface.Services;
using GalaxyBudsClient.Interface.ViewModels;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Utils;

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
        await DialogService.ShowDeveloperOptions();
    }

    protected override void OnLanguageUpdated()
    {
        /* TODO:  Workaround because Avalonia does not expose enough of its Binding/MarkupExtension API
                  to allow for dynamic language updates with ValueConverters.
                  They have plans to implement a WPF-like API to obtain a BindingExpression from an existing Binding,
                  which can be used to call expr.UpdateTarget(). 
                  This could be implemented in an XAML custom behavior or markup extension */ 
        AppTheme.ItemsSource = Enum.GetValues(typeof(DarkModes));
        AppTheme.SelectedValue = Settings.Instance.DarkMode;
        DynamicTrayIcon.ItemsSource = Enum.GetValues(typeof(DynamicTrayIconModes));
        DynamicTrayIcon.SelectedValue = Settings.Instance.DynamicTrayIconMode;
        PopupPosition.ItemsSource = Enum.GetValues(typeof(PopupPlacement));
        PopupPosition.SelectedValue = Settings.Instance.Popup.Placement;
    }
}