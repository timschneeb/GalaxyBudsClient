using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Interface.ViewModels.Dialogs;
using GalaxyBudsClient.Model.Hotkeys;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;

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
            Title = Loc.Resolve(hotkey == null ? "hotkey_add" : "hotkey_edit_long"),
            PrimaryButtonText = Loc.Resolve("okay"),
            CloseButtonText = Loc.Resolve("cancel"),
            DefaultButton = ContentDialogButton.Primary
        };

        var viewModel = new HotkeyEditorDialogViewModel(hotkey);
        dialog.Content = new HotkeyEditorDialog()
        {
            DataContext = viewModel
        };

        dialog.PrimaryButtonClick += OnPrimaryButtonClick;
        var result = await dialog.ShowAsync(MainWindow.Instance);
        dialog.PrimaryButtonClick -= OnPrimaryButtonClick;
            
        return result == ContentDialogResult.None ? null : viewModel.Hotkey;

        void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var defer = args.GetDeferral();
            args.Cancel = viewModel.Hotkey == null;

            if (args.Cancel)
            {
                var resultHint = new ContentDialog()
                {
                    Content = Loc.Resolve("hotkey_edit_invalid"),
                    Title = Loc.Resolve("hotkey_edit_invalid_desc"),
                    PrimaryButtonText = Loc.Resolve("window_close")
                };
                _ = resultHint.ShowAsync(MainWindow.Instance)
                    .ContinueWith(_ => defer.Complete());
            }
            else
            {
                defer.Complete();
            }
        }
    }

}