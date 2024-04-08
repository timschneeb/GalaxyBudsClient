using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using FluentIcons.Common;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Utils.Interface;
using ReactiveUI;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class WelcomePageViewModel : MainPageViewModelBase
{
    public override Control CreateView() => new WelcomePage();
    public override string TitleKey => Keys.WelcomeTextblockHeader;
    public override Symbol IconKey => Symbol.LinkMultiple;
    public override bool ShowsInFooter => false;
}


