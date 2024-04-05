using Avalonia.Controls;
using FluentIcons.Common;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Pages;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class SystemPageViewModel : MainPageViewModelBase
{
    public override Control CreateView() => new SystemPage();
    public override string TitleKey => Keys.MainpageSystem;
    public override Symbol IconKey => Symbol.Apps;
    public override bool ShowsInFooter => false;
}


