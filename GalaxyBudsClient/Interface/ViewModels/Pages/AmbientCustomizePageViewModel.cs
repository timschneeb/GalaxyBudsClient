using Avalonia.Controls;
using FluentIcons.Common;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Model.Constants;
using ReactiveUI.Fody.Helpers;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class AmbientCustomizePageViewModel : SubPageViewModelBase
{
    public override Control CreateView() => new AmbientCustomizePage();
    
    public AmbientCustomizePageViewModel()
    {
        
    }
    
    public override string TitleKey => "nc_as_header";
}


