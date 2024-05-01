using Avalonia.Interactivity;
using GalaxyBudsClient.Interface.Services;
using GalaxyBudsClient.Interface.ViewModels.Pages;

namespace GalaxyBudsClient.Interface.Pages;

public partial class FindMyBudsPage : BasePage<FindMyBudsPageViewModel>
{
    public FindMyBudsPage()
    {
        InitializeComponent();
    }

    private void OnSmartThingsFindDataClicked(object? sender, RoutedEventArgs e)
    {
        NavigationService.Instance.Navigate(typeof(FmmConfigPageViewModel));
    }
}