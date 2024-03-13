using System;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Interface.Services;
using GalaxyBudsClient.Interface.ViewModels;

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
        
        var targetType = Type.GetType($"GalaxyBudsClient.Interface.ViewModels.{item.Name}PageViewModel");
        if (targetType != null) 
            NavigationService.Instance.Navigate(targetType);
    }

    protected override void OnLanguageUpdated()
    {
        // TODO: refresh bindings
        base.OnLanguageUpdated();
    }
}
