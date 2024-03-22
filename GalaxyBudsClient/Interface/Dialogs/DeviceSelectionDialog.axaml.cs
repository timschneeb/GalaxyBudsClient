using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Bluetooth;
using GalaxyBudsClient.Interface.ViewModels.Dialogs;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;

namespace GalaxyBudsClient.Interface.Dialogs;

public partial class DeviceSelectionDialog : UserControl
{
    public DeviceSelectionDialog()
    {
        InitializeComponent();
    }
    
    private DeviceSelectionDialogViewModel ViewModel => (DeviceSelectionDialogViewModel)DataContext!;
    
    public static async Task<bool> OpenDialogAsync()
    {
        var dialog = new ContentDialog
        {
            Title = Loc.Resolve("devsel_header"),
            CloseButtonText = Loc.Resolve("cancel")
        };

        var viewModel = new DeviceSelectionDialogViewModel(dialog);
        dialog.Content = new DeviceSelectionDialog
        {
            DataContext = viewModel
        };

        dialog.PrimaryButtonClick += OnPrimaryButtonClick;
        var result = await dialog.ShowAsync(MainWindow.Instance);
        dialog.PrimaryButtonClick -= OnPrimaryButtonClick;

        return result != ContentDialogResult.None;

        void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = viewModel.SelectedModel == Models.NULL || viewModel.SelectedDevice == null;
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

    public async void OnManualConnectClicked(object? sender, RoutedEventArgs e)
    {
        var result = await ManualPairDialog.OpenDialogAsync();
        if (result != null)
        {
            ViewModel.RegisterDevice(result.Value.model, result.Value.device.Address);
        }
    }

    public void OnUseWinRtCheckedChanged(object? sender, RoutedEventArgs e)
    {
        BluetoothService.Reallocate();
        ViewModel.DoRefreshCommand();
    }

    private void OnListItemPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is StyledElement { DataContext: BluetoothDevice device })
        {
            ViewModel.DoConnectCommand(device); 
        }
    }
}