using GalaxyBudsClient.Interface.ViewModels;
using GalaxyBudsClient.Interface.ViewModels.Pages;

namespace GalaxyBudsClient.Interface.Pages;

public partial class EqualizerPage : BasePage<EqualizerPageViewModel>
{
    public EqualizerPage()
    {
        InitializeComponent();
    }

    protected override void OnLanguageUpdated()
    {
        // TODO Sliders descriptions don't update
        base.OnLanguageUpdated();
    }
}
