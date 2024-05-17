using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.ViewModels.Dialogs;
using GalaxyBudsClient.Model;

namespace GalaxyBudsClient.Interface.Dialogs;

public partial class HotkeyEditorDialog : UserControl
{
    public HotkeyEditorDialog()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    public static async Task<Hotkey?> OpenEditDialogAsync(Hotkey? hotkey)
    {
        var dialog = new ContentDialog
        {
            Title = hotkey == null ? Strings.HotkeyAdd : Strings.HotkeyEditLong,
            PrimaryButtonText = Strings.Okay,
            CloseButtonText = Strings.Cancel,
            DefaultButton = ContentDialogButton.Primary
        };

        var viewModel = new HotkeyEditorDialogViewModel(hotkey);
        dialog.Content = new HotkeyEditorDialog
        {
            DataContext = viewModel
        };

        dialog.PrimaryButtonClick += OnPrimaryButtonClick;
        var result = await dialog.ShowAsync(MainWindow.Instance);
        dialog.PrimaryButtonClick -= OnPrimaryButtonClick;
            
        return result == ContentDialogResult.None ? null : viewModel.Hotkey;

        void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = viewModel.Hotkey == null;

            if (!args.Cancel)
                return;
            
            var resultHint = new ContentDialog
            {
                Title = Strings.HotkeyEditInvalid,
                Content = Strings.HotkeyEditInvalidDesc,
                PrimaryButtonText = Strings.WindowClose
            };
            _ = resultHint.ShowAsync(MainWindow.Instance);
        }
    }

}