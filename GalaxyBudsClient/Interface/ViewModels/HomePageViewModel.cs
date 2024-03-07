using System;
using Avalonia.Controls;
using FluentIcons.Common;
using GalaxyBudsClient.Interface.Pages;

namespace GalaxyBudsClient.Interface.ViewModels;

public class HomePageViewModel : ViewModelBase, IMainPageViewModel
{
    public override Control CreateView() => new HomePage();
    
    public HomePageViewModel()
    {
        
    }

    public string NavHeader => "Home";
    public Symbol IconKey => Symbol.Home;
    public bool ShowsInFooter => false;
}


