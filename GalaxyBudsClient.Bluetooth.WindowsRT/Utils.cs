using System;
using System.Text.RegularExpressions;

namespace GalaxyBudsClient.Bluetooth.WindowsRT
{
    internal static class Utils
    {
        public static byte[] ShrinkTo(this byte[] byteArray, int len)
        {
            byte[] tmp = new byte[len];
            Array.Copy(byteArray, tmp, len);
            return tmp;
        }

        public static string BluetoothAddressAsString(this Windows.Devices.Bluetooth.BluetoothDevice dev)
        {
            return Regex.Replace(dev.BluetoothAddress.ToString("X12"), 
                "(.{2})(.{2})(.{2})(.{2})(.{2})(.{2})",
                "$1:$2:$3:$4:$5:$6");
        }
    }
}