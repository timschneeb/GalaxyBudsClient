using Avalonia.Controls;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;
using ReactiveUI.Fody.Helpers;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class SystemInfoPageViewModel : SubPageViewModelBase
{
    public override Control CreateView() => new SystemInfoPage();
    public override string TitleKey => "system_header";
    
    public SystemInfoPageViewModel()
    {
        SppMessageHandler.Instance.GetAllDataResponse += OnGetAllDataResponse;
        SppMessageHandler.Instance.BatteryTypeResponse += OnBatteryTypeReceived;
        SppMessageHandler.Instance.BuildStringResponse += OnDebugBuildInfoReceived;
        SppMessageHandler.Instance.DebugSkuUpdate += OnDebugSkuReceived;
        SppMessageHandler.Instance.SerialNumberResponse += OnDebugSerialNumberReceived;
        SppMessageHandler.Instance.CradleSerialNumberResponse += OnDebugSerialNumberReceived;
        SppMessageHandler.Instance.ExtendedStatusUpdate += OnExtendedStatusUpdateReceived;
        BluetoothImpl.Instance.Connected += (_, _) => RequestData();
        Loc.LanguageUpdated += RequestData;
    }

    private void OnDebugSerialNumberReceived(object? sender, CradleSerialNumberParser e)
    {
        CradleSerialNumber = e.SoftwareVersion ?? Loc.Resolve("placement_disconnected");
        CradleSwVersion = e.SerialNumber ?? Loc.Resolve("placement_disconnected");
    }

    private void OnExtendedStatusUpdateReceived(object? sender, ExtendedStatusUpdateParser e)
    {
        ProtocolVersion = e.Revision.ToString();
    }

    private void OnDebugSerialNumberReceived(object? sender, DebugSerialNumberParser e)
    {
        SerialNumber = e is { LeftSerialNumber: not null, RightSerialNumber: not null }
            ? $"{Loc.Resolve("left")}: {e.LeftSerialNumber}, {Loc.Resolve("right")}: {e.RightSerialNumber}"
            : Unknown;
    }

    private void OnDebugSkuReceived(object? sender, DebugSkuParser e)
    {
        DeviceSku = e is { LeftSku: not null, RightSku: not null }
            ? $"{Loc.Resolve("left")}: {e.LeftSku}, {Loc.Resolve("right")}: {e.RightSku}"
            : Unknown;
    }

    private void OnDebugBuildInfoReceived(object? sender, string e)
    {
        BuildString = e;
    }

    private void OnBatteryTypeReceived(object? sender, BatteryTypeParser e)
    {
        BatteryType = e is { LeftBatteryType: not null, RightBatteryType: not null }
            ? $"{Loc.Resolve("left")}: {e.LeftBatteryType}, {Loc.Resolve("right")}: {e.RightBatteryType}"
            : Unknown;
    }
    
    private void OnGetAllDataResponse(object? sender, DebugGetAllDataParser e)
    {
        HwVersion = e.HardwareVersion ?? Unknown;
        SwVersion = e.SoftwareVersion ?? Unknown;
        TouchSwVersion = e.TouchSoftwareVersion ?? Unknown;
        BluetoothAddress = e is { LeftBluetoothAddress: not null, RightBluetoothAddress: not null }
            ? $"{Loc.Resolve("left")}: {e.LeftBluetoothAddress}, {Loc.Resolve("right")}: {e.RightBluetoothAddress}"
            : Unknown;
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
    
    private static string Placeholder => Loc.Resolve("system_waiting_for_device");
    private static string Unknown => Loc.Resolve("unknown");
}


