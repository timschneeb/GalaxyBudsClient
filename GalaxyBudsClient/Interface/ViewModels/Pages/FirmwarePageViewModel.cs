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
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Firmware;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.Extensions;
using ReactiveUI.Fody.Helpers;
using Serilog;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class FirmwarePageViewModel : SubPageViewModelBase
{
    public override Control CreateView() => new FirmwarePage();
    public override string TitleKey => Keys.FwSelectHeader;
    
    [Reactive] public bool IsDowngradingAllowed { set; get; }
    [Reactive] public bool NoResults { set; get; } = true;
    public ObservableCollection<FirmwareRemoteBinary> AvailableFirmware { get; } = [];
    
    private readonly FirmwareRemoteClient _client = new();
    private bool _isRefreshing;
    
    public FirmwarePageViewModel()
    {
        BluetoothImpl.Instance.Connected += (_, _) => RequestData();
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
        if(!BluetoothImpl.Instance.IsConnected)
            return;
        
        if (BluetoothImpl.Instance.DeviceSpec.Supports(Features.DebugSku))
            await BluetoothImpl.Instance.SendRequestAsync(MsgIds.DEBUG_SKU);
    }

    public override async void OnNavigatedTo()
    {
        RefreshList(true);
        
        if (LegacySettings.Instance.FirmwareWarningAccepted) 
            return;
        
        // Show disclaimer
        var result = await new QuestionBox
        {
            Title = Strings.FwDisclaimer,
            Description = Strings.FwDisclaimerDesc,
            ButtonText = Strings.ContinueButton
        }.ShowAsync();

        LegacySettings.Instance.FirmwareWarningAccepted = result;
        if (!result)
        {
            MainWindow.Instance.MainView.FrameView.GoBack();
        }
    }
    
    public async void DoInstallCommand(object? param)
    {
        if (param is not FirmwareRemoteBinary firmware) return;
        
        if (firmware.Model != BluetoothImpl.ActiveModel)
        {
            await new MessageBox
            {
                Title = Strings.Error,
                Description = Strings.FwSelectUnsupportedSelection
            }.ShowAsync();
            return;
        }
        
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
                message = $"{Strings.FwSelectHttpError} {infEx.ErrorCode}";
            }
            
            await new MessageBox
            {
                Title = Strings.Error,
                Description =
                    $"{Strings.FwSelectNetError}\n\n{message}"
            }.ShowAsync();

            Log.Error(ex, "FirmwareSelectionPage.Next: Network error");
            return;
        }
            
        await PrepareInstallation(binary, firmware.BuildName ?? Strings.FwSelectUnknownBuild);
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
            await new MessageBox
            {
                Title = Strings.FwSelectVerifyFail,
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
        var connectedModel = DeviceMessageCache.Instance.DebugSku?.ModelFromSku() ?? BluetoothImpl.ActiveModel;
        var firmwareModel = binary.DetectModel();
        if (firmwareModel == null)
        {
            Log.Warning("FirmwareSelectionPage.PrepareInstallation: Firmware model is null; skipping verification");
        }
        else if(connectedModel != firmwareModel)
        {
            await new MessageBox
            {
                Title = Strings.FwSelectVerifyFail,
                Description = string.Format(
                    Strings.FwSelectVerifyModelMismatchFail, 
                    firmwareModel.Value.GetModelMetadataAttribute()?.Name ?? Strings.Unknown, 
                    connectedModel.GetModelMetadataAttribute()?.Name
                )
            }.ShowAsync();
            return;
        }
            
        var result = await new QuestionBox
        {
            Title = string.Format(
                Strings.FwSelectConfirm,
                binary.BuildName, 
                BluetoothImpl.ActiveModel.GetModelMetadataAttribute()?.Name ?? Strings.Unknown
            ),
            Description = Strings.FwSelectConfirmDesc,
            ButtonText = Strings.ContinueButton
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
                    message = $"{Strings.FwSelectHttpError} {infEx.ErrorCode}";
                }
                
                await new MessageBox
                {
                    Title = Strings.Error,
                    Description =
                        $"{Strings.FwSelectNetIndexError}\n\n{message}"
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