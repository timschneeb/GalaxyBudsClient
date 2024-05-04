using Avalonia.Controls;
using FluentIcons.Common;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Pages;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class WelcomePageViewModel : MainPageViewModelBase
{
    public override Control CreateView() => new WelcomePage { DataContext = this };
    public override string TitleKey => Keys.WelcomeTextblockHeader;
    public override Symbol IconKey => Symbol.LinkMultiple;
    public override bool ShowsInFooter => false;
}


