using Avalonia.Controls;
using FluentIcons.Common;
using GalaxyBudsClient.Interface.Pages;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class SystemPageViewModel : MainPageViewModelBase
{
    public override Control CreateView() => new SystemPage();
    
    public SystemPageViewModel()
    {
        
    }

    public override string TitleKey => "mainpage_system";
    public override Symbol IconKey => Symbol.Apps;
    public override bool ShowsInFooter => false;
}


