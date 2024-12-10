using System;
using Android.Bluetooth;
using Serilog;

namespace GalaxyBudsClient.Android.Utils;

public static class Extensions
{
    public static void CloseSafely(this BluetoothSocket? socket)
    {
        try
        {
            socket?.Close();
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Failed to close BluetoothSocket properly");
        }
    }
}