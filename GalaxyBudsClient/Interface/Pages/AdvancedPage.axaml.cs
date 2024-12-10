using System.Linq;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Interface.Services;
using GalaxyBudsClient.Interface.ViewModels.Pages;

namespace GalaxyBudsClient.Interface.Pages;

public partial class AdvancedPage : BasePage<AdvancedPageViewModel>
{
    private readonly string[] _restrictedItems = ["Rename"];
    
    public AdvancedPage()
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
}