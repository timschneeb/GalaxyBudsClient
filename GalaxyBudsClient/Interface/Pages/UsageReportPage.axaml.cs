using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Interface.ViewModels.Pages;
using GalaxyBudsClient.Model.Config;

namespace GalaxyBudsClient.Interface.Pages;

public partial class UsageReportPage : BasePage<UsageReportPageViewModel>
{
    public UsageReportPage()
    {
        InitializeComponent();
    }

    private void OnHintClosed(InfoBar sender, InfoBarClosedEventArgs args)
    {
        Settings.Data.IsUsageReportHintHidden = true;
    }
}