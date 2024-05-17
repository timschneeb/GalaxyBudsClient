using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.ViewModels.Dialogs;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Platform.Interfaces;
using GalaxyBudsClient.Platform.Model;

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
            Title = Strings.DevselHeader,
            CloseButtonText = Strings.Cancel
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
                    Title = Strings.Error,
                    Description = Strings.DevselInvalidSelection
                }.ShowAsync();
            }
        }
    }

    public async void OnManualConnectClicked(object? sender, RoutedEventArgs e)
    {
        var result = await ManualPairDialog.OpenDialogAsync();
        if (result != null)
        {
            ViewModel.RegisterDevice(result.Value.model, result.Value.device.Address, result.Value.device.Name);
        }
    }

    public void OnUseWinRtCheckedChanged(object? sender, RoutedEventArgs e)
    {
        PlatformImpl.SwitchWindowsBackend();
        BluetoothImpl.Reallocate();
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