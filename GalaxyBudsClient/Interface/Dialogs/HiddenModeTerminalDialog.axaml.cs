using System.Threading.Tasks;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.ViewModels.Dialogs;

namespace GalaxyBudsClient.Interface.Dialogs;

public partial class HiddenModeTerminalDialog : UserControl
{
    private HiddenModeTerminalDialogViewModel? ViewModel => DataContext as HiddenModeTerminalDialogViewModel;
    
    public HiddenModeTerminalDialog()
    {
        InitializeComponent();
    }
    
    public static async Task OpenDialogAsync()
    {
        var dialog = new ContentDialog
        {
            Title = Strings.HiddenModeTerminal,
            PrimaryButtonText = Strings.AtTerminalSend,
            CloseButtonText = Strings.WindowClose,
            DefaultButton = ContentDialogButton.Primary
        };

        var viewModel = new HiddenModeTerminalDialogViewModel();
        dialog.Content = new HiddenModeTerminalDialog
        {
            DataContext = viewModel
        };

        dialog.PrimaryButtonClick += OnPrimaryButtonClick;
        await dialog.ShowAsync(TopLevel.GetTopLevel(MainView.Instance));
        dialog.PrimaryButtonClick -= OnPrimaryButtonClick;

        void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            viewModel.DoSendCommand();
            args.Cancel = true;
        }
    }
}