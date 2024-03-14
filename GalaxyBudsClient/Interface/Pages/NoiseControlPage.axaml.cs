using Avalonia.Diagnostics;
using GalaxyBudsClient.Interface.ViewModels;
using GalaxyBudsClient.Interface.ViewModels.Pages;

namespace GalaxyBudsClient.Interface.Pages;

public partial class NoiseControlPage : BasePage<NoiseControlPageViewModel>
{
    public NoiseControlPage()
    {
        InitializeComponent();
    }

    protected override void OnLanguageUpdated()
    {
        // TODO Sliders descriptions don't update
        base.OnLanguageUpdated();
    }
}
