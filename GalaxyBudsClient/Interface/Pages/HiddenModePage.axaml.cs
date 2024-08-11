using Avalonia.Interactivity;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.ViewModels.Pages;

namespace GalaxyBudsClient.Interface.Pages;

public partial class HiddenModePage : BasePage<HiddenModePageViewModel>
{
    public HiddenModePage()
    {
        InitializeComponent();
    }

    private void OnChangeSerialClicked(object? sender, RoutedEventArgs e)
    {
        // TODO
    }

    private void OnHiddenTerminalClicked(object? sender, RoutedEventArgs e)
    {
        _ = HiddenModeTerminalDialog.OpenDialogAsync();
    }
}