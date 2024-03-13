using System;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using GalaxyBudsClient.Interface.Services;
using GalaxyBudsClient.Interface.ViewModels;
using GalaxyBudsClient.Interface.ViewModels.Pages;

namespace GalaxyBudsClient.Interface.Pages;

public partial class HomePage : BasePage<HomePageViewModel>
{
    public HomePage()
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

    protected override void OnLanguageUpdated()
    {
        // TODO: refresh bindings
        base.OnLanguageUpdated();
    }
}
