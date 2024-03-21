using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using Serilog;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace GalaxyBudsClient.Model.Firmware;

public class FirmwareRemoteBinary(
    string? buildName,
    Models? model,
    string? region,
    string? bootloaderVersion,
    string? reservedField,
    int? year,
    int? month,
    int? revision)
{
    public string? BuildName { set; get; } = buildName;
    public Models? Model { set; get; } = model;
    public string? Region { set; get; } = region;
    public string? BootloaderVersion { set; get; } = bootloaderVersion;
    public string? ReservedField { set; get; } = reservedField;
    public int? Year { set; get; } = year;
    public int? Month { set; get; } = month;
    public int? Revision { set; get; } = revision;
}
    
public static class FirmwareRemoteBinaryFilters
{
    private const string CharOrder = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public static bool FilterByModel(FirmwareRemoteBinary item)
    {
        return BluetoothService.ActiveModel == item.Model;
    }
        
    public static bool FilterByVersion(FirmwareRemoteBinary item)
    {
        var current = DeviceMessageCache.Instance.DebugGetAllData?.SoftwareVersion;
        if (current == null || item.BuildName == null)
        {
            return false;
        }

        if (current.Length == 12 && item.BuildName.Length == 12) {
            var versionData = current.Substring(current.Length - 3, 3).ToCharArray();
            var versionData2 = item.BuildName.Substring(item.BuildName.Length - 3, 3).ToCharArray();

            for (var i = 0; i < 3; i++) {
                if (CharOrder.IndexOf(versionData[i]) < CharOrder.IndexOf(versionData2[i])) {
                    // Newer version
                    return true;
                }
                if (CharOrder.IndexOf(versionData[i]) > CharOrder.IndexOf(versionData2[i])) {
                    // Older version
                    return false;
                }
            }
            // Equal version
            return false;
        }
        else
        {   
            Log.Warning("FirmwareRemoteBinary: Device version info length is wrong: {Current} vs {New}", 
                current.Length, item.BuildName.Length);
            return false;
        }
    }
}