using GalaxyBudsClient.Interface.ViewModels.Pages;

namespace GalaxyBudsClient.Interface.Pages;

public partial class SystemInfoPage : BasePage<SystemInfoPageViewModel>
{
    public SystemInfoPage()
    {
        InitializeComponent();
    }

    protected override void OnLanguageUpdated()
    {
        SystemInfoPageViewModel.RequestData();
    }
}