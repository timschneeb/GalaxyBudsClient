using Avalonia.Controls;
using FluentIcons.Common;
using GalaxyBudsClient.Interface.Pages;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class HomePageViewModel : MainPageViewModelBase
{
    public override Control CreateView() => new HomePage();
    
    public HomePageViewModel()
    {
        
    }

    public override string TitleKey => "mainpage_header";
    public override Symbol IconKey => Symbol.Home;
    public override bool ShowsInFooter => false;
}


