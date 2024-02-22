using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Scripting.Experiment;
using Serilog;

namespace GalaxyBudsClient.Model.Firmware
{
    public class FirmwareRemoteBinary
    {
        public FirmwareRemoteBinary(string? buildName, Models? model, string? region, string? bootloaderVersion, string? reservedField, int? year, int? month, int? revision)
        {
            BuildName = buildName;
            Model = model;
            Region = region;
            BootloaderVersion = bootloaderVersion;
            ReservedField = reservedField;
            Year = year;
            Month = month;
            Revision = revision;
        }

        public string? BuildName { set; get; }
        public Models? Model { set; get; }
        public string? Region { set; get; }
        public string? BootloaderVersion { set; get; }
        public string? ReservedField { set; get; }
        public int? Year { set; get; }
        public int? Month { set; get; }
        public int? Revision { set; get; }
    }
    
    public static class FirmwareRemoteBinaryFilters
    {
        
        private static readonly string CHAR_ORDER = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        
        public static bool FilterByModel(FirmwareRemoteBinary item)
        {
            return BluetoothImpl.Instance.ActiveModel == item.Model;
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
                    if (CHAR_ORDER.IndexOf(versionData[i]) < CHAR_ORDER.IndexOf(versionData2[i])) {
                        // Newer version
                        return true;
                    }
                    if (CHAR_ORDER.IndexOf(versionData[i]) > CHAR_ORDER.IndexOf(versionData2[i])) {
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

}