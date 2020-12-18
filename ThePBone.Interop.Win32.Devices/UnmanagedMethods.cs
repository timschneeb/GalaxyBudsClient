using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Bluetooth.Msft;
using ThePBone.Interop.Win32.Devices.Bluetooth;

namespace ThePBone.Interop.Win32.Devices
{
    // ReSharper disable InconsistentNaming
#pragma warning disable 169, 649

    public static class UnmanagedMethods
    {
        internal delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        public enum WindowsMessage : uint
        {
            WM_NULL = 0x0000,
            WM_CREATE = 0x0001,
            WM_DESTROY = 0x0002,
            WM_MOVE = 0x0003,
            WM_SIZE = 0x0005,
            WM_ACTIVATE = 0x0006,
            WM_SETFOCUS = 0x0007,
            WM_KILLFOCUS = 0x0008,
            WM_ENABLE = 0x000A,
            WM_SETREDRAW = 0x000B,
            WM_SETTEXT = 0x000C,
            WM_GETTEXT = 0x000D,
            WM_GETTEXTLENGTH = 0x000E,
            WM_PAINT = 0x000F,
            WM_CLOSE = 0x0010,
            WM_QUERYENDSESSION = 0x0011,
            WM_QUERYOPEN = 0x0013,
            WM_ENDSESSION = 0x0016,
            WM_QUIT = 0x0012,
            WM_ERASEBKGND = 0x0014,
            WM_SYSCOLORCHANGE = 0x0015,
            WM_SHOWWINDOW = 0x0018,
            WM_WININICHANGE = 0x001A,
            WM_SETTINGCHANGE = WM_WININICHANGE,
            WM_DEVMODECHANGE = 0x001B,
            WM_ACTIVATEAPP = 0x001C,
            WM_FONTCHANGE = 0x001D,
            WM_TIMECHANGE = 0x001E,
            WM_CANCELMODE = 0x001F,
            WM_SETCURSOR = 0x0020,
            WM_MOUSEACTIVATE = 0x0021,
            WM_CHILDACTIVATE = 0x0022,
            WM_QUEUESYNC = 0x0023,
            WM_GETMINMAXINFO = 0x0024,
            WM_PAINTICON = 0x0026,
            WM_ICONERASEBKGND = 0x0027,
            WM_NEXTDLGCTL = 0x0028,
            WM_SPOOLERSTATUS = 0x002A,
            WM_DRAWITEM = 0x002B,
            WM_MEASUREITEM = 0x002C,
            WM_DELETEITEM = 0x002D,
            WM_VKEYTOITEM = 0x002E,
            WM_CHARTOITEM = 0x002F,
            WM_SETFONT = 0x0030,
            WM_GETFONT = 0x0031,
            WM_SETHOTKEY = 0x0032,
            WM_GETHOTKEY = 0x0033,
            WM_QUERYDRAGICON = 0x0037,
            WM_COMPAREITEM = 0x0039,
            WM_GETOBJECT = 0x003D,
            WM_COMPACTING = 0x0041,
            WM_WINDOWPOSCHANGING = 0x0046,
            WM_WINDOWPOSCHANGED = 0x0047,
            WM_COPYDATA = 0x004A,
            WM_CANCELJOURNAL = 0x004B,
            WM_NOTIFY = 0x004E,
            WM_INPUTLANGCHANGEREQUEST = 0x0050,
            WM_INPUTLANGCHANGE = 0x0051,
            WM_TCARD = 0x0052,
            WM_HELP = 0x0053,
            WM_USERCHANGED = 0x0054,
            WM_NOTIFYFORMAT = 0x0055,
            WM_CONTEXTMENU = 0x007B,
            WM_STYLECHANGING = 0x007C,
            WM_STYLECHANGED = 0x007D,
            WM_DISPLAYCHANGE = 0x007E,
            WM_GETICON = 0x007F,
            WM_SETICON = 0x0080,
            WM_NCCREATE = 0x0081,
            WM_NCDESTROY = 0x0082,
            WM_NCCALCSIZE = 0x0083,
            WM_NCHITTEST = 0x0084,
            WM_NCPAINT = 0x0085,
            WM_NCACTIVATE = 0x0086,
            WM_GETDLGCODE = 0x0087,
            WM_SYNCPAINT = 0x0088,
            WM_NCMOUSEMOVE = 0x00A0,
            WM_NCLBUTTONDOWN = 0x00A1,
            WM_NCLBUTTONUP = 0x00A2,
            WM_NCLBUTTONDBLCLK = 0x00A3,
            WM_NCRBUTTONDOWN = 0x00A4,
            WM_NCRBUTTONUP = 0x00A5,
            WM_NCRBUTTONDBLCLK = 0x00A6,
            WM_NCMBUTTONDOWN = 0x00A7,
            WM_NCMBUTTONUP = 0x00A8,
            WM_NCMBUTTONDBLCLK = 0x00A9,
            WM_NCXBUTTONDOWN = 0x00AB,
            WM_NCXBUTTONUP = 0x00AC,
            WM_NCXBUTTONDBLCLK = 0x00AD,
            WM_INPUT_DEVICE_CHANGE = 0x00FE,
            WM_INPUT = 0x00FF,
            WM_KEYFIRST = 0x0100,
            WM_KEYDOWN = 0x0100,
            WM_KEYUP = 0x0101,
            WM_CHAR = 0x0102,
            WM_DEADCHAR = 0x0103,
            WM_SYSKEYDOWN = 0x0104,
            WM_SYSKEYUP = 0x0105,
            WM_SYSCHAR = 0x0106,
            WM_SYSDEADCHAR = 0x0107,
            WM_UNICHAR = 0x0109,
            WM_KEYLAST = 0x0109,
            WM_IME_STARTCOMPOSITION = 0x010D,
            WM_IME_ENDCOMPOSITION = 0x010E,
            WM_IME_COMPOSITION = 0x010F,
            WM_IME_KEYLAST = 0x010F,
            WM_INITDIALOG = 0x0110,
            WM_COMMAND = 0x0111,
            WM_SYSCOMMAND = 0x0112,
            WM_TIMER = 0x0113,
            WM_HSCROLL = 0x0114,
            WM_VSCROLL = 0x0115,
            WM_INITMENU = 0x0116,
            WM_INITMENUPOPUP = 0x0117,
            WM_MENUSELECT = 0x011F,
            WM_MENUCHAR = 0x0120,
            WM_ENTERIDLE = 0x0121,
            WM_MENURBUTTONUP = 0x0122,
            WM_MENUDRAG = 0x0123,
            WM_MENUGETOBJECT = 0x0124,
            WM_UNINITMENUPOPUP = 0x0125,
            WM_MENUCOMMAND = 0x0126,
            WM_CHANGEUISTATE = 0x0127,
            WM_UPDATEUISTATE = 0x0128,
            WM_QUERYUISTATE = 0x0129,
            WM_CTLCOLORMSGBOX = 0x0132,
            WM_CTLCOLOREDIT = 0x0133,
            WM_CTLCOLORLISTBOX = 0x0134,
            WM_CTLCOLORBTN = 0x0135,
            WM_CTLCOLORDLG = 0x0136,
            WM_CTLCOLORSCROLLBAR = 0x0137,
            WM_CTLCOLORSTATIC = 0x0138,
            WM_MOUSEFIRST = 0x0200,
            WM_MOUSEMOVE = 0x0200,
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_LBUTTONDBLCLK = 0x0203,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205,
            WM_RBUTTONDBLCLK = 0x0206,
            WM_MBUTTONDOWN = 0x0207,
            WM_MBUTTONUP = 0x0208,
            WM_MBUTTONDBLCLK = 0x0209,
            WM_MOUSEWHEEL = 0x020A,
            WM_XBUTTONDOWN = 0x020B,
            WM_XBUTTONUP = 0x020C,
            WM_XBUTTONDBLCLK = 0x020D,
            WM_MOUSEHWHEEL = 0x020E,
            WM_MOUSELAST = 0x020E,
            WM_PARENTNOTIFY = 0x0210,
            WM_ENTERMENULOOP = 0x0211,
            WM_EXITMENULOOP = 0x0212,
            WM_NEXTMENU = 0x0213,
            WM_SIZING = 0x0214,
            WM_CAPTURECHANGED = 0x0215,
            WM_MOVING = 0x0216,
            WM_POWERBROADCAST = 0x0218,
            WM_DEVICECHANGE = 0x0219,
            WM_MDICREATE = 0x0220,
            WM_MDIDESTROY = 0x0221,
            WM_MDIACTIVATE = 0x0222,
            WM_MDIRESTORE = 0x0223,
            WM_MDINEXT = 0x0224,
            WM_MDIMAXIMIZE = 0x0225,
            WM_MDITILE = 0x0226,
            WM_MDICASCADE = 0x0227,
            WM_MDIICONARRANGE = 0x0228,
            WM_MDIGETACTIVE = 0x0229,
            WM_MDISETMENU = 0x0230,
            WM_ENTERSIZEMOVE = 0x0231,
            WM_EXITSIZEMOVE = 0x0232,
            WM_DROPFILES = 0x0233,
            WM_MDIREFRESHMENU = 0x0234,
            WM_IME_SETCONTEXT = 0x0281,
            WM_IME_NOTIFY = 0x0282,
            WM_IME_CONTROL = 0x0283,
            WM_IME_COMPOSITIONFULL = 0x0284,
            WM_IME_SELECT = 0x0285,
            WM_IME_CHAR = 0x0286,
            WM_IME_REQUEST = 0x0288,
            WM_IME_KEYDOWN = 0x0290,
            WM_IME_KEYUP = 0x0291,
            WM_MOUSEHOVER = 0x02A1,
            WM_MOUSELEAVE = 0x02A3,
            WM_NCMOUSEHOVER = 0x02A0,
            WM_NCMOUSELEAVE = 0x02A2,
            WM_WTSSESSION_CHANGE = 0x02B1,
            WM_TABLET_FIRST = 0x02c0,
            WM_TABLET_LAST = 0x02df,
            WM_DPICHANGED = 0x02E0,
            WM_CUT = 0x0300,
            WM_COPY = 0x0301,
            WM_PASTE = 0x0302,
            WM_CLEAR = 0x0303,
            WM_UNDO = 0x0304,
            WM_RENDERFORMAT = 0x0305,
            WM_RENDERALLFORMATS = 0x0306,
            WM_DESTROYCLIPBOARD = 0x0307,
            WM_DRAWCLIPBOARD = 0x0308,
            WM_PAINTCLIPBOARD = 0x0309,
            WM_VSCROLLCLIPBOARD = 0x030A,
            WM_SIZECLIPBOARD = 0x030B,
            WM_ASKCBFORMATNAME = 0x030C,
            WM_CHANGECBCHAIN = 0x030D,
            WM_HSCROLLCLIPBOARD = 0x030E,
            WM_QUERYNEWPALETTE = 0x030F,
            WM_PALETTEISCHANGING = 0x0310,
            WM_PALETTECHANGED = 0x0311,
            WM_HOTKEY = 0x0312,
            WM_PRINT = 0x0317,
            WM_PRINTCLIENT = 0x0318,
            WM_APPCOMMAND = 0x0319,
            WM_THEMECHANGED = 0x031A,
            WM_CLIPBOARDUPDATE = 0x031D,
            WM_DWMCOMPOSITIONCHANGED = 0x031E,
            WM_DWMNCRENDERINGCHANGED = 0x031F,
            WM_DWMCOLORIZATIONCOLORCHANGED = 0x0320,
            WM_DWMWINDOWMAXIMIZEDCHANGE = 0x0321,
            WM_GETTITLEBARINFOEX = 0x033F,
            WM_HANDHELDFIRST = 0x0358,
            WM_HANDHELDLAST = 0x035F,
            WM_AFXFIRST = 0x0360,
            WM_AFXLAST = 0x037F,
            WM_PENWINFIRST = 0x0380,
            WM_PENWINLAST = 0x038F,
            WM_TOUCH = 0x0240,
            WM_APP = 0x8000,
            WM_USER = 0x0400,
        }

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr CreateWindowEx(
           int dwExStyle,
           uint lpClassName,
           string lpWindowName,
           uint dwStyle,
           int x,
           int y,
           int nWidth,
           int nHeight,
           IntPtr hWndParent,
           IntPtr hMenu,
           IntPtr hInstance,
           IntPtr lpParam);

        [DllImport("user32.dll", EntryPoint = "DefWindowProcW")]
        internal static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        internal static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "RegisterClassExW")]
        internal static extern ushort RegisterClassEx(ref WNDCLASSEX lpwcx);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct WNDCLASSEX
        {
            public int cbSize;
            public int style;
            public WndProc lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public string lpszMenuName;
            public string lpszClassName;
            public IntPtr hIconSm;
        }


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
