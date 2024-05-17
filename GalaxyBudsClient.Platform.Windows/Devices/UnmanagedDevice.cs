using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Bluetooth.Msft;

namespace GalaxyBudsClient.Platform.Windows.Devices
{
    // ReSharper disable InconsistentNaming
#pragma warning disable 169, 649

    public static class UnmanagedDevice
    {

        [DllImport("User32.dll", CharSet = CharSet.Unicode, SetLastError = true,
    EntryPoint = "RegisterDeviceNotification")]
        internal static extern RegisterDeviceNotificationSafeHandle RegisterDeviceNotification_SafeHandle(
   IntPtr hRecipient,
   ref DEV_BROADCAST_HANDLE notificationFilter,
   RegisterDeviceNotificationFlags flags
   );

        [DllImport("User32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool UnregisterDeviceNotification(IntPtr handle);

        internal static class BluetoothDeviceNotificationEvent
        {
            /// <summary>
            /// &#x201C;&#x201D;
            /// </summary>
            [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Bth")]
            public static readonly Guid BthPortDeviceInterface = new Guid("{0850302A-B344-4fda-9BE9-90576B8D46F0}");
            /// <summary>
            /// &#x201C;This message is sent when any of the following attributes of a remote Bluetooth device has changed:
            /// the device has been discovered, the class of device, name, connected state, or device remembered state.
            /// This message is also sent when these attributes are set or cleared.&#x201D;
            /// </summary>
            public static readonly Guid RadioInRange = new Guid("{EA3B5B82-26EE-450E-B0D8-D26FE30A3869}");
            /// <summary>
            /// &#x201C;This message is sent when a previously discovered device has not been found after the completion of the last inquiry.
            /// This message will not be sent for remembered devices.
            /// The BTH_ADDRESS structure is the address of the device that was not found.&#x201D;
            /// </summary>
            public static readonly Guid RadioOutOfRange = new Guid("{E28867C9-C2AA-4CED-B969-4570866037C4}");
            /// <summary>
            /// &#x201C;This message should be ignored by the application.
            /// If the application must receive PIN requests, the BluetoothRegisterForAuthentication function should be used.&#x201D;
            /// </summary>
            public static readonly Guid PinRequest = new Guid("{BD198B7C-24AB-4B9A-8C0D-A8EA8349AA16}");
            /// <summary>
            /// &#x201C;This message is sent when an L2CAP channel between the local radio and a remote Bluetooth device has been established or terminated. 
            /// For L2CAP channels that are multiplexers, such as RFCOMM, this message is only sent when the underlying channel is established, 
            /// not when each multiplexed channel, such as an RFCOMM channel, is established or terminated.&#x201D;
            /// </summary>
            public static readonly Guid L2capEvent = new Guid("{7EAE4030-B709-4AA8-AC55-E953829C9DAA}");
            /// <summary>
            /// &#x201C;This message is sent when a remote Bluetooth device connects or disconnects at the ACL level.&#x201D;
            /// </summary>
            [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Hci")]
            public static readonly Guid HciEvent = new Guid("{FC240062-1541-49BE-B463-84C4DCD7BF7F}");
            // New somewhere after WinXP.
            public static readonly Guid AuthenticationRequestEvent = new Guid("{5DC9136D-996C-46DB-84F5-32C0A3F47352}");
            public static readonly Guid KeyPressEvent = new Guid("{D668DFCD-0F4E-4EFC-BFE0-392EEEC5109C}");
            public static readonly Guid HciVendorEvent = new Guid("{547247e6-45bb-4c33-af8c-c00efe15a71d}");
        }

        internal struct BTH_RADIO_IN_RANGE
        {
            internal BTH_DEVICE_INFO deviceInfo;
            internal BluetoothDeviceInfoProperties previousDeviceFlags;
        }

        internal struct BTH_RADIO_OUT_OF_RANGE
        {
            internal Int64 deviceAddress;
        }

        /// <summary>
        /// Buffer associated with GUID_BLUETOOTH_L2CAP_EVENT
        /// </summary>
        internal struct BTH_L2CAP_EVENT_INFO
        {
            /// <summary>
            /// Remote radio address which the L2CAP event is associated with
            /// </summary>
            internal readonly Int64 bthAddress;

            /// <summary>
            /// The PSM that is either being connected to or disconnected from
            /// </summary>
            internal readonly ushort psm;

            /// <summary>
            /// If != 0, then the channel has just been established.  If == 0, then the
            /// channel has been destroyed.  Notifications for a destroyed channel will
            /// only be sent for channels successfully established.
            /// </summary>
            [MarshalAs(UnmanagedType.U1)]
            internal readonly bool connected;

            /// <summary>
            /// If != 0, then the local host iniated the l2cap connection.  If == 0, then
            /// the remote host initated the connection.  This field is only valid if
            /// connect is != 0.
            /// </summary>
            [MarshalAs(UnmanagedType.U1)]
            internal readonly bool initiated;

            [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "shutUpCompiler")]
            internal BTH_L2CAP_EVENT_INFO(Version shutUpCompiler)
            {
                this.bthAddress = 0;
                this.psm = 0;
                this.connected = false;
                this.initiated = false;
            }
        }

        internal enum HCI_CONNNECTION_TYPE : byte
        {
            ACL = (1),
            SCO = (2),
            LE = (3), // Added in Windows 8
        }

        /// <summary>
        /// Buffer associated with GUID_BLUETOOTH_HCI_EVENT
        /// </summary>
        internal struct BTH_HCI_EVENT_INFO
        {
            /// <summary>
            /// Remote radio address which the HCI event is associated with
            /// </summary>
            internal readonly Int64 bthAddress;

            /// <summary>
            /// HCI_CONNNECTION_TYPE_XXX value
            /// </summary>
            internal readonly HCI_CONNNECTION_TYPE connectionType;

            /// <summary>
            /// If != 0, then the underlying connection to the remote radio has just
            /// been estrablished.  If == 0, then the underlying conneciton has just been
            /// destroyed.
            /// </summary>
            [MarshalAs(UnmanagedType.U1)]
            internal readonly bool connected;

            [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "shutUpCompiler")]
            internal BTH_HCI_EVENT_INFO(Version shutUpCompiler)
            {
                this.bthAddress = 0;
                this.connectionType = 0;
                this.connected = false;
            }
        }

        internal enum RegisterDeviceNotificationFlags
        {
            DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000,
            DEVICE_NOTIFY_SERVICE_HANDLE = 0x00000001,
            DEVICE_NOTIFY_ALL_INTERFACE_CLASSES = 0x00000004
        };

        internal enum Dbt
        {
            /// <summary>
            /// A request to change the current configuration (dock or undock) has been canceled. 
            /// </summary>
            ConfigChangeCanceled = 0x0019,
            /// <summary>
            /// The current configuration has changed, due to a dock or undock. 
            /// </summary>
            ConfigChanged = 0x0018,
            /// <summary>
            /// A custom event has occurred. 
            /// </summary>
            /// <remarks>Windows NT 4.0 and Windows 95:  This value is not supported.</remarks>
            CustomEvent = 0x8006,
            /// <summary>
            /// A device or piece of media has been inserted and is now available. 
            /// </summary>
            DeviceArrival = 0x8000,
            /// <summary>
            /// Permission is requested to remove a device or piece of media. Any application can deny this request and cancel the removal. 
            /// </summary>
            DeviceQueryRemove = 0x8001,
            /// <summary>
            /// A request to remove a device or piece of media has been canceled. 
            /// </summary>
            DeviceQueryRemoveFailed = 0x8002,
            /// <summary>
            /// A device or piece of media has been removed. 
            /// </summary>
            DeviceRemoveComplete = 0x8004,
            /// <summary>
            /// A device or piece of media is about to be removed. Cannot be denied. 
            /// </summary>
            DeviceRemovePending = 0x8003,
            /// <summary>
            /// A device-specific event has occurred. 
            /// </summary>
            DeviceTypeSpecific = 0x8005,
            /// <summary>
            /// A device has been added to or removed from the system. 
            /// </summary>
            /// <remarks>Windows NT 4.0 and Windows Me/98/95:  This value is not supported.</remarks>
            DevNodesChanged = 0x0007,
            /// <summary>
            /// Permission is requested to change the current configuration (dock or undock). 
            /// </summary>
            QueryChangeConfig = 0x0017,
            /// <summary>
            /// The meaning of this message is user-defined. 
            /// </summary>
            UserDefined = 0xFFFF,
        }

        internal enum DbtDevTyp : uint
        {
            /// <summary>
            /// oem-defined device type
            /// </summary>
            Oem = 0x00000000,
            /// <summary>
            /// devnode number
            /// /// </summary>
            DevNode = 0x00000001,
            /// <summary>
            /// 
            /// </summary>
            Volume = 0x00000002,
            /// <summary>
            /// l
            /// </summary>
            Port = 0x00000003,
            /// <summary>
            /// network resource
            /// </summary>
            Network = 0x00000004,
            /// <summary>
            /// device interface class
            /// </summary>
            DeviceInterface = 0x00000005,
            /// <summary>
            /// file system handle
            /// </summary>
            Handle = 0x00000006
        }

        internal struct DEV_BROADCAST_HDR
        {
            internal Int32 dbch_size;
            internal DbtDevTyp dbch_devicetype;
            internal Int32 dbch_reserved;
        }

        //[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential,
        //   CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        internal struct DEV_BROADCAST_PORT
        {
            internal DEV_BROADCAST_HDR header;
            //[//error CS0647: Error emitting 'System.Runtime.InteropServices.MarshalAsAttribute' attribute -- 'SizeConst is required for a fixed string.'
            //System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValTStr)]
            //public IntPtr ____dbcp_name;//String
            internal byte[] ____dbcp_name;//String
        }

        internal struct DEV_BROADCAST_HANDLE
        {
            // 32-bit:     3*4 + 2*4 + 16 + 4 + a = 6*4+16 = 24+16 + a = 40 + a = 44
            // 64-bit: (3+1)*4 + 2*8 + 16 + 4 + a = 16+16+16+4 + a     = 52 + a = 56
            const int SizeWithoutFakeDataArray = 40;
            const int SizeOfOneByteWithPadding = 4;
            const int SizeWithFakeDataArray = SizeWithoutFakeDataArray + SizeOfOneByteWithPadding;
            static int ActualSizeWithFakeDataArray;

            public DEV_BROADCAST_HDR header;
            //--
            internal readonly IntPtr dbch_handle;
            internal readonly IntPtr dbch_hdevnotify;
            internal readonly Guid dbch_eventguid;
            internal readonly Int32 dbch_nameoffset;
            // We can't include the fake data array because we use this struct as 
            // the first field in other structs!
            // byte dbch_data__0; //dbch_data[1];

            //----
            public DEV_BROADCAST_HANDLE(IntPtr deviceHandle)
            {
                this.header.dbch_reserved = 0;
                this.dbch_hdevnotify = IntPtr.Zero;
                this.dbch_eventguid = Guid.Empty;
                this.dbch_nameoffset = 0;
                //--
                this.header.dbch_devicetype = DbtDevTyp.Handle;
                this.dbch_handle = deviceHandle;
                //System.Diagnostics.Debug.Assert(
                //    SizeWithoutFakeDataArray == System.Runtime.InteropServices.Marshal.SizeOf(typeof(DEV_BROADCAST_HANDLE)),
                //    "Size not as expected");
                if (ActualSizeWithFakeDataArray == 0)
                {
                    int actualSizeWithoutFakeDataArray = System.Runtime.InteropServices.Marshal.SizeOf(typeof(DEV_BROADCAST_HANDLE));
                    ActualSizeWithFakeDataArray = Pad(1 + actualSizeWithoutFakeDataArray, IntPtr.Size);
                }
                this.header.dbch_size = ActualSizeWithFakeDataArray;
                //this.header.dbch_size = actualSizeWithoutFakeDataArray;

            }

            private static int Pad(int size, int alignment)
            {
                int x = size + alignment - 1;
                x /= alignment;
                x *= alignment;
                return x;
            }

        }

        internal struct DEV_BROADCAST_HANDLE__withData
        {
            public DEV_BROADCAST_HDR header;
            //--
            readonly IntPtr dbch_handle;
            readonly IntPtr dbch_hdevnotify;
            readonly Guid dbch_eventguid;
            readonly Int32 dbch_nameoffset;
            readonly byte dbch_data__0; //dbch_data[1];

            [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "weAreForPinvoke")]
            private DEV_BROADCAST_HANDLE__withData(int weAreForPinvoke)
            {
                this.header = new DEV_BROADCAST_HDR();
                this.dbch_handle = IntPtr.Zero;
                this.dbch_hdevnotify = IntPtr.Zero;
                this.dbch_eventguid = Guid.Empty;
                this.dbch_nameoffset = this.dbch_nameoffset = 0;
                this.dbch_data__0 = this.dbch_data__0 = 0;
            }
        }
    }
}
