using Avalonia.Controls;
using FluentIcons.Common;
using GalaxyBudsClient.Interface.Pages;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class WelcomePageViewModel : MainPageViewModelBase
{
    public override Control CreateView() => new WelcomePage();
    public override string TitleKey => "welcome_textblock_header";
    public override Symbol IconKey => Symbol.LinkMultiple;
    public override bool ShowsInFooter => false;
}


