using GalaxyBudsClient.Interface.ViewModels.Pages;

namespace GalaxyBudsClient.Interface.Pages;

public partial class RenamePage : BasePage<RenamePageViewModel>
{
    public RenamePage()
    {
        InitializeComponent();
        BudsName.TextChanged += (_, _) => ViewModel!.NameTextChanged(BudsName.Text ?? "");
    }
}