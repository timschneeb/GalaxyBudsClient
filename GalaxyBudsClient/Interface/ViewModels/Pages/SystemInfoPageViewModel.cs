using Avalonia.Controls;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Interface;
using ReactiveUI.Fody.Helpers;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class SystemInfoPageViewModel : SubPageViewModelBase
{
    public override Control CreateView() => new SystemInfoPage { DataContext = this };
    public override string TitleKey => Keys.SystemHeader;
    
    public SystemInfoPageViewModel()
    {
        SppMessageReceiver.Instance.GetAllDataResponse += OnGetAllDataResponse;
        SppMessageReceiver.Instance.BatteryTypeResponse += OnBatteryTypeReceived;
        SppMessageReceiver.Instance.BuildStringResponse += OnDebugBuildInfoReceived;
        SppMessageReceiver.Instance.VersionInfoResponse += OnVersionInfoResponse;
        SppMessageReceiver.Instance.DebugSkuUpdate += OnDebugSkuReceived;
        SppMessageReceiver.Instance.SerialNumberResponse += OnDebugSerialNumberReceived;
        SppMessageReceiver.Instance.CradleSerialNumberResponse += OnDebugSerialNumberReceived;
        SppMessageReceiver.Instance.ExtendedStatusUpdate += OnExtendedStatusUpdateReceived;
        BluetoothImpl.Instance.Connected += (_, _) => RequestData();
        Loc.LanguageUpdated += RequestData;
    }

    private void OnVersionInfoResponse(object? sender, DebugModeVersionDecoder? e)
    {
        if (e is not null)
        {
            HwVersion =
                $"{Strings.Left}: {e.LeftHardwareVersion ?? Unknown}, {Strings.Right}: {e.RightHardwareVersion ?? Unknown}";
            SwVersion =
                $"{Strings.Left}: {e.LeftSoftwareVersion ?? Unknown}, {Strings.Right}: {e.RightSoftwareVersion ?? Unknown}";
            TouchSwVersion =
                $"{Strings.Left}: {e.LeftTouchSoftwareVersion ?? Unknown}, {Strings.Right}: {e.RightTouchSoftwareVersion ?? Unknown}";
        }

        // Fallback to GET_ALL_DATA if the version info is incomplete
        if(e is null or { LeftTouchSoftwareVersion: "0", RightTouchSoftwareVersion: "0" })
            TouchSwVersion = DeviceMessageCache.Instance.DebugGetAllData?.TouchSoftwareVersion ?? Unknown;
        if(e is null or { LeftHardwareVersion: "rev0.0", RightHardwareVersion: "rev0.0" })
            HwVersion = DeviceMessageCache.Instance.DebugGetAllData?.HardwareVersion ?? Unknown;
        if(e is null || (e.LeftSoftwareVersion?.StartsWith('R') != true && e.RightSoftwareVersion?.StartsWith('R') != true))
            SwVersion = DeviceMessageCache.Instance.DebugGetAllData?.SoftwareVersion ?? Unknown;
    }

    private void OnDebugSerialNumberReceived(object? sender, CradleSerialNumberDecoder e)
    {
        CradleSerialNumber = e.SerialNumber ?? Strings.PlacementDisconnected;
        CradleSwVersion = e.SoftwareVersion ?? Strings.PlacementDisconnected;
    }

    private void OnExtendedStatusUpdateReceived(object? sender, ExtendedStatusUpdateDecoder e)
    {
        ProtocolVersion = e.Revision.ToString();
    }

    private void OnDebugSerialNumberReceived(object? sender, DebugSerialNumberDecoder e)
    {
        SerialNumber = e is { LeftSerialNumber: not null, RightSerialNumber: not null }
            ? $"{Strings.Left}: {e.LeftSerialNumber}, {Strings.Right}: {e.RightSerialNumber}"
            : Unknown;
    }

    private void OnDebugSkuReceived(object? sender, DebugSkuDecoder e)
    {
        DeviceSku = e is { LeftSku: not null, RightSku: not null }
            ? $"{Strings.Left}: {e.LeftSku}, {Strings.Right}: {e.RightSku}"
            : Unknown;
    }

    private void OnDebugBuildInfoReceived(object? sender, string e)
    {
        BuildString = e;
    }

    private void OnBatteryTypeReceived(object? sender, BatteryTypeDecoder e)
    {
        BatteryType = e is { LeftBatteryType: not null, RightBatteryType: not null }
            ? $"{Strings.Left}: {e.LeftBatteryType}, {Strings.Right}: {e.RightBatteryType}"
            : Unknown;
    }
    
    private void OnGetAllDataResponse(object? sender, DebugGetAllDataDecoder e)
    {
        BluetoothAddress = e.LocalBluetoothAddress != null || e.PeerBluetoothAddress != null
            ? string.Format(Strings.SystemBtaddrTemplate, e.LocalBluetoothAddress ?? Unknown, e.PeerBluetoothAddress ?? Unknown)
            : Unknown;

        // Buds3 and up don't respond to VERSION_INFO. Force load the info from GET_ALL_DEBUG by passing a null object
        if (BluetoothImpl.Instance.CurrentModel >= Models.Buds3)
        {
            OnVersionInfoResponse(this, null);
        }
    }
    
    private static async void RequestData()
    {
        if (BluetoothImpl.Instance.DeviceSpec.Supports(Features.BatteryType))
            await BluetoothImpl.Instance.SendRequestAsync(MsgIds.BATTERY_TYPE);
        if (BluetoothImpl.Instance.DeviceSpec.Supports(Features.BuildInfo))
            await BluetoothImpl.Instance.SendRequestAsync(MsgIds.DEBUG_BUILD_INFO);
        if (BluetoothImpl.Instance.DeviceSpec.Supports(Features.DebugSku))
            await BluetoothImpl.Instance.SendRequestAsync(MsgIds.DEBUG_SKU);
        if (BluetoothImpl.Instance.DeviceSpec.Supports(Features.CradleSerialNumber))
            await BluetoothImpl.Instance.SendRequestAsync(MsgIds.CRADLE_SERIAL_NUMBER);
        
        await BluetoothImpl.Instance.SendRequestAsync(MsgIds.DEBUG_SERIAL_NUMBER);
        await BluetoothImpl.Instance.SendRequestAsync(MsgIds.DEBUG_GET_ALL_DATA);
        await BluetoothImpl.Instance.SendRequestAsync(MsgIds.VERSION_INFO);
    }
    
    public override void OnNavigatedTo() => RequestData();

    [Reactive] public string HwVersion { set; get; } = Placeholder;
    [Reactive] public string SwVersion { set; get; } = Placeholder;
    [Reactive] public string TouchSwVersion { set; get; } = Placeholder;
    [Reactive] public string ProtocolVersion { set; get; } = Placeholder;
    [Reactive] public string BluetoothAddress { set; get; } = Placeholder;
    [Reactive] public string SerialNumber { set; get; } = Placeholder;
    [Reactive] public string CradleSerialNumber { set; get; } = Placeholder;
    [Reactive] public string CradleSwVersion { set; get; } = Placeholder;
    [Reactive] public string BuildString { set; get; } = Placeholder;
    [Reactive] public string DeviceSku { set; get; } = Placeholder;
    [Reactive] public string BatteryType { set; get; } = Placeholder;
    
    private static string Placeholder => Strings.SystemWaitingForDevice;
    private static string Unknown => Strings.Unknown;
}


