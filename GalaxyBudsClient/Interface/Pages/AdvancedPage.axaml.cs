using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Interface.Services;
using GalaxyBudsClient.Interface.ViewModels.Pages;

namespace GalaxyBudsClient.Interface.Pages;

public partial class AdvancedPage : BasePage<AdvancedPageViewModel>
{
    public AdvancedPage()
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
}