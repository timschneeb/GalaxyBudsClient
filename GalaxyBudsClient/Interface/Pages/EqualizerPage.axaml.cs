using GalaxyBudsClient.Interface.ViewModels;

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
