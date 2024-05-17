using System;

using System.Text.RegularExpressions;
using Windows.Devices.Bluetooth;

namespace GalaxyBudsClient.Platform.WindowsRT
{
    internal static class Utils
    {
        public static byte[] ShrinkTo(this byte[] byteArray, int len)
        {
            var tmp = new byte[len];
            Array.Copy(byteArray, tmp, len);
            return tmp;
        }

        public static string BluetoothAddressAsString(this BluetoothDevice dev)
        {
            return Regex.Replace(dev.BluetoothAddress.ToString("X12"), 
                "(.{2})(.{2})(.{2})(.{2})(.{2})(.{2})",
                "$1:$2:$3:$4:$5:$6");
        }
    }
}