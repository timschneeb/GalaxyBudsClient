using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Bluetooth;
using GalaxyBudsClient.Interface.ViewModels.Dialogs;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;

namespace GalaxyBudsClient.Interface.Dialogs;

public partial class ManualPairDialog : UserControl
{
    public ManualPairDialog()
    {
        InitializeComponent();
    }
    
    public static async Task<(Models model, BluetoothDevice device)?> OpenDialogAsync()
    {
        var dialog = new ContentDialog
        {
            Title = Loc.Resolve("devsel_manual_pair"),
            PrimaryButtonText = Loc.Resolve("okay"),
            CloseButtonText = Loc.Resolve("cancel"),
            DefaultButton = ContentDialogButton.Primary
        };

        var viewModel = new ManualPairDialogViewModel
        {
            Devices = await BluetoothService.Instance.GetDevicesAsync()
        };
        dialog.Content = new ManualPairDialog
        {
            DataContext = viewModel
        };

        dialog.PrimaryButtonClick += OnPrimaryButtonClick;
        var result = await dialog.ShowAsync(MainWindow.Instance);
        dialog.PrimaryButtonClick -= OnPrimaryButtonClick;
            
        return result == ContentDialogResult.None ||
               viewModel.SelectedDevice == null || 
               viewModel.SelectedModel == null ? null : (viewModel.SelectedModel.Value, viewModel.SelectedDevice);

        void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = viewModel.SelectedModel == null ||
                          viewModel.SelectedModel == Models.NULL || 
                          viewModel.SelectedDevice == null;
            if (args.Cancel)
            {
                _ = new MessageBox
                {
                    Title = Loc.Resolve("error"),
                    Description = Loc.Resolve("devsel_invalid_selection")
                }.ShowAsync();
            }
        }
    }
}