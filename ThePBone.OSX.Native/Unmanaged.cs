using System;
using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo
#pragma warning disable 649

namespace ThePBone.OSX.Native.Unmanaged
{
    internal struct BluetoothImpl
    {
        internal unsafe void* client;
        internal unsafe void* watcher;
    }

    public struct Device {
        internal IntPtr mac_address;
        internal IntPtr device_name;
        internal bool is_connected;
        internal bool is_paired;
        internal uint cod;
    };

    public struct EnumerationResult
    {
        internal int length;
        internal unsafe Device* devices;
    };

    enum BT_CONN_RESULT {
        BT_CONN_SUCCESS = 0x00,
        BT_CONN_EBASECONN,
        BT_CONN_ENOTFOUND,
        BT_CONN_ENOTPAIRED,
        BT_CONN_ESDP,
        BT_CONN_ECID,
        BT_CONN_EOPEN,
        BT_CONN_EUNKNOWN
    };

    enum BT_ENUM_RESULT {
        BT_ENUM_SUCCESS = 0x00,
        BT_ENUM_EUNKNOWN
    };

    enum BT_SEND_RESULT {
        BT_SEND_SUCCESS = 0x00,
        BT_SEND_EPARTIAL,
        BT_SEND_EUNKNOWN,
        BT_SEND_ENULL
    };

    internal static class DSO
    {
        internal const string Name = "NativeInterop.dylib";
    }
    
    internal static class Bluetooth
    {
        /* Bluetooth manager */
        [DllImport(DSO.Name)]
        internal static extern unsafe bool bt_alloc(BluetoothImpl** self);
        [DllImport(DSO.Name)]
        internal static extern unsafe void bt_free(BluetoothImpl* self);
        
        [DllImport(DSO.Name)]
        internal static extern unsafe BT_CONN_RESULT bt_connect(BluetoothImpl* self, string mac, byte* uuid);
        [DllImport(DSO.Name)]
        internal static extern unsafe bool bt_disconnect(BluetoothImpl* self);
        [DllImport(DSO.Name)]
        internal static extern unsafe BT_SEND_RESULT bt_send(BluetoothImpl* self, byte* data, uint length);
        [DllImport(DSO.Name)]
        internal static extern unsafe BT_ENUM_RESULT bt_enumerate(BluetoothImpl* self, ref EnumerationResult result);
        [DllImport(DSO.Name)]
        internal static extern unsafe bool bt_is_connected(BluetoothImpl* self);
        
        [DllImport(DSO.Name)]
        internal static extern unsafe bool bt_set_on_channel_data(BluetoothImpl* self, Bt_OnChannelData cb);
        [DllImport(DSO.Name)]
        internal static extern unsafe bool bt_set_on_channel_closed(BluetoothImpl* self, Bt_OnChannelClosed cb);
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void Bt_OnChannelData(IntPtr data, ulong size);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void Bt_OnChannelClosed();
        
        /* Bluetooth watcher */
        [DllImport(DSO.Name)]
        internal static extern unsafe bool bt_register_disconnect_notification(BluetoothImpl* self, string mac);
        [DllImport(DSO.Name)]
        internal static extern unsafe void bt_set_on_connected(BluetoothImpl* self, BtDev_OnConnected cb);
        [DllImport(DSO.Name)]
        internal static extern unsafe void bt_set_on_disconnected(BluetoothImpl* self, BtDev_OnDisconnected cb);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void BtDev_OnConnected(IntPtr mac, IntPtr name);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void BtDev_OnDisconnected(IntPtr mac);

    }

    internal static class Memory
    {
        [DllImport(DSO.Name)]
        internal static extern void btdev_free(ref Device result);
        [DllImport(DSO.Name)]
        internal static extern unsafe void mem_free(void* ptr);
    }

    public static class AppUtils
    {
        [DllImport(DSO.Name)]
        public static extern void setHideInDock(bool hide);
        [DllImport(DSO.Name)]
        public static extern bool setAutoStartEnabled(bool autoStart);
        [DllImport(DSO.Name)]
        public static extern bool isAutoStartEnabled();
    }
}