using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Model.Firmware;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.Extensions;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;
using ReactiveUI.Fody.Helpers;
using Serilog;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class FirmwarePageViewModel : SubPageViewModelBase
{
    public override Control CreateView() => new FirmwarePage();
    public override string TitleKey => "fw_select_header";
    
    [Reactive] public bool IsDowngradingAllowed { set; get; }
    [Reactive] public bool NoResults { set; get; } = true;
    public ObservableCollection<FirmwareRemoteBinary> AvailableFirmware { get; } = [];
    
    private readonly FirmwareRemoteClient _client = new();
    private bool _isRefreshing;
    
    public FirmwarePageViewModel()
    {
        BluetoothService.Instance.Connected += (_, _) => RequestData();
        AvailableFirmware.CollectionChanged += (_, _) => NoResults = !AvailableFirmware.Any(); 
        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(e.PropertyName == nameof(IsDowngradingAllowed))
            RefreshList(false);
    }

    private static async void RequestData()
    { 
        if(!BluetoothService.Instance.IsConnected)
            return;
        
        if (BluetoothService.Instance.DeviceSpec.Supports(Features.DebugSku))
            await BluetoothService.Instance.SendRequestAsync(SppMessage.MessageIds.DEBUG_SKU);
    }

    public override async void OnNavigatedTo()
    {
        RefreshList(true);
        
        if (Settings.Instance.FirmwareWarningAccepted) 
            return;
        
        // Show disclaimer
        var result = await new QuestionBox
        {
            Title = Loc.Resolve("fw_disclaimer"),
            Description = Loc.Resolve("fw_disclaimer_desc"),
            ButtonText = Loc.Resolve("continue_button")
        }.ShowAsync();

        Settings.Instance.FirmwareWarningAccepted = result;
        if (!result)
        {
            MainWindow.Instance.MainView.FrameView.GoBack();
        }
    }
    
    public async void DoInstallCommand(object? param)
    {
        if (param is not FirmwareRemoteBinary firmware) return;
        
        if (firmware.Model != BluetoothService.ActiveModel)
        {
            await new MessageBox
            {
                Title = Loc.Resolve("error"),
                Description = Loc.Resolve("fw_select_unsupported_selection")
            }.ShowAsync();
            return;
        }
        
        // TODO add downloading dialog
        
       // _navBarNextLabel.Content = Loc.Resolve("fw_select_downloading");

        byte[] binary;
        try
        {
            binary = await _client.DownloadFirmware(firmware);
        }
        catch (Exception ex)
        {
            var message = ex.Message;
            if (ex is NetworkInformationException infEx)
            {
                message = $"{Loc.Resolve("fw_select_http_error")} {infEx.ErrorCode}";
            }
            
            await new MessageBox()
            {
                Title = Loc.Resolve("error"),
                Description =
                    $"{Loc.Resolve("fw_select_net_error")}\n\n{message}"
            }.ShowAsync();

            Log.Error(ex, "FirmwareSelectionPage.Next: Network error");
            return;
        }
            
        await PrepareInstallation(binary, firmware.BuildName ?? Loc.Resolve("fw_select_unknown_build"));
    }
    
    public void DoRefreshCommand()
    { 
        RefreshList(false);
    }
    
    public async void DoSideloadCommand()
    { 
        var filters = new List<FilePickerFileType>
        {
            new("Firmware binary") { Patterns = new List<string> { "*.bin" } },
            new("All files") { Patterns = new List<string> { "*" } }
        };
                
        var file = await MainWindow.Instance.OpenFilePickerAsync(filters);
        if (file == null)
            return;
       
        await PrepareInstallation(await File.ReadAllBytesAsync(file), Path.GetFileName(file));
    }
    
    private async Task PrepareInstallation(byte[] data, string buildName)
    {
        FirmwareBinary? binary;
        try
        {
            binary = new FirmwareBinary(data, buildName);
        }
        catch (FirmwareParseException ex)
        {
            await new MessageBox()
            {
                Title = Loc.Resolve("fw_select_verify_fail"),
                Description = ex.ErrorMessage
            }.ShowAsync();
            return;
        }
            
        /*
         * Safety check: Verify whether the firmware binary is compatible with the current device to avoid hard bricks.
         *
         * We cannot rely on BluetoothImpl.Instance.ActiveModel here, because users can spoof the device model
         * during setup using the "Advanced" menu for troubleshooting. If available, we use the SKU instead.
         */
        var connectedModel = DeviceMessageCache.Instance.DebugSku?.ModelFromSku() ?? BluetoothService.ActiveModel;
        var firmwareModel = binary.DetectModel();
        if (firmwareModel == null)
        {
            Log.Warning("FirmwareSelectionPage.PrepareInstallation: Firmware model is null; skipping verification");
        }
        else if(connectedModel != firmwareModel)
        {
            await new MessageBox()
            {
                Title = Loc.Resolve("fw_select_verify_fail"),
                Description = string.Format(
                    Loc.Resolve("fw_select_verify_model_mismatch_fail"), 
                    firmwareModel.Value.GetModelMetadata()?.Name ?? Loc.Resolve("unknown"), 
                    connectedModel.GetModelMetadata()?.Name
                )
            }.ShowAsync();
            return;
        }
            
        var result = await new QuestionBox()
        {
            Title = string.Format(
                Loc.Resolve("fw_select_confirm"),
                binary.BuildName, 
                BluetoothService.ActiveModel.GetModelMetadata()?.Name ?? Loc.Resolve("unknown")
            ),
            Description = Loc.Resolve("fw_select_confirm_desc"),
            ButtonText = Loc.Resolve("continue_button")
        }.ShowAsync();

        if (result)
        {
            await Task.Delay(200);
            await BeginTransfer(binary);
        }
    }

    private async void RefreshList(bool silent = false)
    {
        if (_isRefreshing)
        {
            Log.Warning("FirmwarePage: Refresh already in progress");
            return;
        }
            
        _isRefreshing = true;

        FirmwareRemoteBinary[] firmwareBins;
        try
        {
            firmwareBins = await _client.SearchForFirmware(IsDowngradingAllowed);
            AvailableFirmware.Clear();
        }
        catch (Exception ex)
        {
            _isRefreshing = false;
                
            if (!silent)
            {
                var message = ex.Message;
                if (ex is NetworkInformationException infEx)
                {
                    message = $"{Loc.Resolve("fw_select_http_error")} {infEx.ErrorCode}";
                }
                
                await new MessageBox()
                {
                    Title = Loc.Resolve("error"),
                    Description =
                        $"{Loc.Resolve("fw_select_net_index_error")}\n\n{message}"
                }.ShowAsync();
            }
                
            Log.Error(ex, "FirmwarePage.RefreshList: Network error");
            return;
        }
            
        firmwareBins
            .ToList()
            .ForEach(x => AvailableFirmware.Add(x));

        _isRefreshing = false;
    }
    
    private async Task BeginTransfer(FirmwareBinary binary)
    {
        var dialog = new FirmwareUpdateDialog();
        await dialog.BeginTransfer(binary);
        await dialog.ShowAsync(true);
    }
}