using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Interface.ViewModels.Pages;
using GalaxyBudsClient.Model.Config;

namespace GalaxyBudsClient.Interface.Pages;

public partial class BatteryHistoryPage : BasePage<BatteryHistoryPageViewModel>
{
    public BatteryHistoryPage()
    {
        InitializeComponent();
    }

    protected override void OnInitialized()
    {
        ViewModel!.Plot = PlotControl.Plot;
        base.OnInitialized();
    }

    private void OnHintClosed(InfoBar sender, InfoBarClosedEventArgs args)
    {
        Settings.Data.IsBatteryHistoryHintHidden = true;
    }
}