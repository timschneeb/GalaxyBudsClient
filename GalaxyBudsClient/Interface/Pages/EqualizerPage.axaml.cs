using Avalonia.Controls;
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
        // TODO: refresh slider bindings
        base.OnLanguageUpdated();
    }
}
