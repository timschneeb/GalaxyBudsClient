using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Interface.ViewModels.Dialogs;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;

namespace GalaxyBudsClient.Interface.Dialogs;

public partial class TouchActionEditorDialog : UserControl
{
    public TouchActionEditorDialog()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
        
    public static async Task<CustomAction?> OpenEditDialogAsync(CustomAction? action)
    {
        var dialog = new ContentDialog
        {
            Title = Loc.Resolve("cact_header"),
            PrimaryButtonText = Loc.Resolve("okay"),
            CloseButtonText = Loc.Resolve("cancel"),
            DefaultButton = ContentDialogButton.Primary
        };

        var viewModel = new TouchActionEditorDialogViewModel(action);
        dialog.Content = new TouchActionEditorDialog
        {
            DataContext = viewModel
        };

        dialog.PrimaryButtonClick += OnPrimaryButtonClick;
        var result = await dialog.ShowAsync(MainWindow.Instance);
        dialog.PrimaryButtonClick -= OnPrimaryButtonClick;
            
        return result == ContentDialogResult.None ? null : viewModel.Action;

        void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = viewModel.Action == null;
        }
    }
}