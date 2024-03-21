using GalaxyBudsClient.Interface.ViewModels.Pages;

namespace GalaxyBudsClient.Interface.Pages;

public partial class AmbientCustomizePage : BasePage<AmbientCustomizePageViewModel>
{
    public AmbientCustomizePage()
    {
        InitializeComponent();
    }

    protected override void OnLanguageUpdated()
    {
        // TODO Sliders descriptions don't update
        base.OnLanguageUpdated();
    }
}
