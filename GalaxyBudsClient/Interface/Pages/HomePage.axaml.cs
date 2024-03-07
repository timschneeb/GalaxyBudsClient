using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Interface.Controls;
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
        
        // TODO
    }

    protected override void OnLanguageUpdated()
    {
        // TODO: refresh bindings
        base.OnLanguageUpdated();
    }
}
