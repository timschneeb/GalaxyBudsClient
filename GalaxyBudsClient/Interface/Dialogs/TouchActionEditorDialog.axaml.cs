using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.ViewModels.Dialogs;
using GalaxyBudsClient.Model;

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
        var dialog = new FAContentDialog
        {
            Title = Strings.CactHeader,
            PrimaryButtonText = Strings.Okay,
            CloseButtonText = Strings.Cancel,
            DefaultButton = FAContentDialogButton.Primary
        };

        var viewModel = new TouchActionEditorDialogViewModel(action);
        dialog.Content = new TouchActionEditorDialog
        {
            DataContext = viewModel
        };

        dialog.PrimaryButtonClick += OnPrimaryButtonClick;
        var result = await dialog.ShowAsync(TopLevel.GetTopLevel(MainView.Instance));
        dialog.PrimaryButtonClick -= OnPrimaryButtonClick;
            
        return result == FAContentDialogResult.None ? null : viewModel.Action;

        void OnPrimaryButtonClick(FAContentDialog sender, FAContentDialogButtonClickEventArgs args)
        {
            args.Cancel = viewModel.Action == null;
        }
    }
}