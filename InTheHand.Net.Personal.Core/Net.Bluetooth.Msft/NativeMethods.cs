// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.Msft.NativeMethods
// 
// Copyright (c) 2003-2011 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Runtime.InteropServices;

namespace InTheHand.Net.Bluetooth.Msft
{
    static class NativeMethods
    {

        // WINDEF.H
        internal const int MAX_PATH = 260;

#if NETCF
        private const string wsDll = "ws2.dll";
        private const string btdrtDll = "btdrt.dll";
        internal const string ceRegistryRoot = "\\SOFTWARE\\Microsoft\\Bluetooth\\";
#else
        private const string wsDll = "ws2_32.dll";
        private const string irpropsDll = "Irprops.cpl";
        private const string bthpropsDll = "bthprops.cpl";
#endif

#if NETCF

        [DllImport("coredll.dll", SetLastError = true)]
        internal static extern int GetModuleFileName(IntPtr hModule, System.Text.StringBuilder lpFilename, int nSize);

		//CE 5.0
        [DllImport(btdrtDll, SetLastError = false)]
		internal static extern int BthAcceptSCOConnections(int fAccept);

		[DllImport(btdrtDll, SetLastError=false)]
        internal static extern int BthAuthenticate(byte[] pbt);

		[DllImport(btdrtDll, SetLastError=false)]
		internal static extern int BthCancelInquiry();

		[DllImport(btdrtDll, SetLastError=false)]
		internal static extern int BthClearInquiryFilter();

		[DllImport(btdrtDll, SetLastError=false)]
		internal static extern int BthCloseConnection(ushort handle);

		[DllImport(btdrtDll, SetLastError=false)]
        internal static extern int BthCreateACLConnection(byte[] pbt, out ushort phandle);

		[DllImport(btdrtDll, SetLastError=false)]
        internal static extern int BthCreateSCOConnection(byte[] pbt, out ushort phandle);
        /*
		[DllImport(btdrtDll, SetLastError=true)]
        internal static extern int BthEnterHoldMode(ref long pba, ushort hold_mode_max, ushort hold_mode_min, ref ushort pinterval);

		[DllImport(btdrtDll, SetLastError=true)]
        internal static extern int BthEnterParkMode(ref long pba, ushort beacon_max, ushort beacon_min, ref ushort pinterval);

		[DllImport(btdrtDll, SetLastError=true)]
        internal static extern int BthEnterSniffMode(ref long pba, ushort sniff_mode_max, ushort sniff_mode_min, ushort sniff_attempt, ushort sniff_timeout, ref ushort pinterval);

		[DllImport(btdrtDll, SetLastError=true)]
        internal static extern int BthExitParkMode(ref long pba);

		[DllImport(btdrtDll, SetLastError=true)]
        internal static extern int BthExitSniffMode(ref long pba);
        */
		[DllImport(btdrtDll, SetLastError=false)]
		internal static extern int BthGetAddress(ushort handle, out long pba);

		[DllImport(btdrtDll, SetLastError=true)]
		internal static extern int BthGetBasebandConnections(int cConnections, byte[] pConnections, ref int pcConnectionsReturned);

		[DllImport(btdrtDll, SetLastError=true)]
		internal static extern int BthGetBasebandHandles(int cHandles, ref ushort pHandles, ref int pcHandlesReturned);

		[DllImport(btdrtDll, SetLastError=true)]
        internal static extern int BthGetCurrentMode(byte[] pba, ref byte pmode);

		[DllImport(btdrtDll, SetLastError=true)]
		internal static extern int BthGetHardwareStatus(ref HardwareStatus pistatus);

		[DllImport(btdrtDll, SetLastError=true)]
        internal static extern int BthGetLinkKey(byte[] pba, ref Guid key);

		[DllImport(btdrtDll, SetLastError=true)]
        internal static extern int BthGetPINRequest(byte[] pba);

		[DllImport(btdrtDll, SetLastError=true)]
        internal static extern int BthGetRemoteCOD(byte[] pbt, out uint pcod);

		//CE 5.0
		[DllImport(btdrtDll, SetLastError=true)]
        internal static extern int BthPairRequest(byte[] pba, int cPinLength, byte[] ppin);

		[DllImport(btdrtDll, SetLastError=true)]
		internal static extern int BthPerformInquiry(int LAP, byte length, byte num_responses,
			int cBuffer, ref int pcDiscoveredDevices, byte[] InquiryList);

		[DllImport(btdrtDll, SetLastError=true)]
		internal static extern int BthReadAuthenticationEnable(out byte pae);

		[DllImport(btdrtDll, SetLastError=true)]
		internal static extern int BthReadCOD(out uint pcod);

		[DllImport(btdrtDll, SetLastError=true)]
        internal static extern int BthReadLinkPolicySettings(byte[] pba, ref ushort plps);

		[DllImport(btdrtDll, SetLastError=true)]
        internal static extern int BthReadLocalAddr(byte[] pba);

		[DllImport(btdrtDll, SetLastError=true)]
		internal static extern int BthReadLocalVersion(out byte phci_version, out ushort phci_revision,
			out byte plmp_version, out ushort plmp_subversion, out ushort pmanufacturer, byte[] plmp_features);

		[DllImport(btdrtDll, SetLastError=true)]
		internal static extern int BthReadPageTimeout(out ushort ptimeout);

		[DllImport(btdrtDll, SetLastError=true)]
        internal static extern int BthReadRemoteVersion(byte[] pba, ref byte plmp_version,
			ref ushort plmp_subversion, ref ushort pmanufacturer, ref byte plmp_features);

		[DllImport(btdrtDll, SetLastError=true)]
        internal static extern int BthReadRemoteVersion(byte[] pba, out LmpVersion plmp_version,
			out ushort plmp_subversion, out Manufacturer pmanufacturer, out LmpFeatures plmp_features);

		[DllImport(btdrtDll, SetLastError=true)]
		internal static extern int BthReadScanEnableMask(out byte pmask);
        [DllImport(btdrtDll, SetLastError = true)]
        internal static extern int BthReadScanEnableMask(out WinCeScanMask pmask);

        //CE 6.0
        [DllImport(btdrtDll, SetLastError = true)]
        internal static extern int BthReadRSSI(byte[] pbt, out sbyte pbRSSI);

		[DllImport(btdrtDll, SetLastError=true)]
        internal static extern int BthRefusePINRequest(byte[] pbt);

		[DllImport(btdrtDll, SetLastError=true)]
        internal static extern int BthRemoteNameQuery(byte[] pba, int cBuffer, out int pcRequired, byte[] szString);

		[DllImport(btdrtDll, SetLastError=true)]
        internal static extern int BthRevokeLinkKey(byte[] pba);

		[DllImport(btdrtDll, SetLastError=true)]
        internal static extern int BthRevokePIN(byte[] pba);

		[DllImport(btdrtDll, SetLastError=true)]
        internal static extern int BthSetEncryption(byte[] pba, int fOn);

		[DllImport(btdrtDll, SetLastError=true)]
        internal static extern int BthSetInquiryFilter(byte[] pba);

		[DllImport(btdrtDll, SetLastError=true)]
        internal static extern int BthSetLinkKey(byte[] pba, ref Guid key);

		[DllImport(btdrtDll, SetLastError=true)]
        internal static extern int BthSetPIN(byte[] pba, int cPinLength, byte[] ppin);

		[DllImport(btdrtDll, SetLastError=true)]
		internal static extern int BthSetSecurityUI(IntPtr hEvent, int dwStoreTimeout, int dwProcTimeout);
		
		[DllImport(btdrtDll, SetLastError=true)]
		internal static extern int BthTerminateIdleConnections();

		[DllImport(btdrtDll, SetLastError=true)]
		internal static extern int BthWriteAuthenticationEnable(byte ae);

		[DllImport(btdrtDll, SetLastError=true)]
		internal static extern int BthWriteCOD(uint cod);

		[DllImport(btdrtDll, SetLastError=true)]
        internal static extern int BthWriteLinkPolicySettings(byte[] pba, ushort lps);

		[DllImport(btdrtDll, SetLastError=true)]
		internal static extern int BthWritePageTimeout(ushort timeout);
		
		[DllImport(btdrtDll, SetLastError=true)]
		internal static extern int BthWriteScanEnableMask(byte mask);
        [DllImport(btdrtDll, SetLastError = true)]
        internal static extern int BthWriteScanEnableMask(WinCeScanMask mask);
 


		//Utils
		[DllImport("BthUtil.dll", SetLastError=true)]
		internal static extern int BthSetMode(RadioMode dwMode);

		[DllImport("BthUtil.dll", SetLastError=true)]
		internal static extern int BthGetMode(out RadioMode dwMode);

        //msgqueue (for notifications)
        /*
        [DllImport("coredll.dll", SetLastError = true)]
        internal static extern IntPtr RequestBluetoothNotifications(BTE_CLASS dwClass, IntPtr hMsgQ);
        [DllImport("coredll.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool StopBluetoothNotifications(IntPtr h);

        internal enum BTE
        {
            CONNECTION = 100, 
            DISCONNECTION = 101, 
            ROLE_SWITCH = 102, 
            MODE_CHANGE = 103, 
            PAGE_TIMEOUT = 104, 
 
            KEY_NOTIFY = 200, 
            KEY_REVOKED = 201, 
 
            LOCAL_NAME = 300, 
            COD = 301, 
 
            STACK_UP = 400, 
            STACK_DOWN = 401, 
        }

        [Flags()]
        internal enum BTE_CLASS
        {
            CONNECTIONS = 1,
            PAIRING = 2, 
            DEVICE = 4,
            STACK = 8,
        }

        [DllImport("coredll.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseMsgQueue(IntPtr hMsgQ);

        [DllImport("coredll.dll", SetLastError = true)]
        internal static extern IntPtr CreateMsgQueue(string lpszName, ref MSGQUEUEOPTIONS lpOptions);

        [DllImport("coredll.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ReadMsgQueue(IntPtr hMsgQ, IntPtr lpBuffer, int cbBufferSize, out int lpNumberOfBytesRead, int dwTimeout, out int pdwFlags);

        [StructLayout(LayoutKind.Sequential)]
        internal struct MSGQUEUEOPTIONS
        {
            internal int dwSize;
            internal int dwFlags;
            internal int dwMaxMessages;
            internal int cbMaxMessage;
            [MarshalAs(UnmanagedType.Bool)]
            internal bool bReadAccess;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct BTEVENT
        {
            internal BTE dwEventId;
            internal int dwReserved;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=64)]
            internal byte[] baEventData;
        }

        internal struct BT_CONNECT_EVENT
        {
            internal int dwSize;
            internal ushort hConnection;
            internal long bta;
            internal byte ucLinkType;
            internal byte ucEncryptMode;
        }
        */

#if V1
		[DllImport("coredll.dll", SetLastError=true)]
		internal static extern int LocalFree(IntPtr ptr);
#endif

#endif

        //SetService
        [DllImport(wsDll, EntryPoint = "WSASetService", SetLastError = true)]
        internal static extern int WSASetService(ref WSAQUERYSET lpqsRegInfo, WSAESETSERVICEOP essoperation, int dwControlFlags);

        //LookupService
        [DllImport(wsDll, EntryPoint = "WSALookupServiceBegin", SetLastError = true)]
        internal static extern int WSALookupServiceBegin(byte[] pQuerySet, LookupFlags dwFlags, out IntPtr lphLookup);

        [DllImport(wsDll, EntryPoint = "WSALookupServiceBegin", SetLastError = true)]
        internal static extern int WSALookupServiceBegin(ref WSAQUERYSET pQuerySet, LookupFlags dwFlags, out IntPtr lphLookup);

        [DllImport(wsDll, EntryPoint = "WSALookupServiceNext", SetLastError = true)]
        internal static extern int WSALookupServiceNext(IntPtr hLookup, LookupFlags dwFlags, ref int lpdwBufferLength, byte[] pResults);

        [DllImport(wsDll, EntryPoint = "WSALookupServiceEnd", SetLastError = true)]
        internal static extern int WSALookupServiceEnd(IntPtr hLookup);





#if WinXP

        internal enum BTH_ERROR
        {
            SUCCESS = (0x00),
            UNKNOWN_HCI_COMMAND = (0x01),
            NO_CONNECTION = (0x02),
            HARDWARE_FAILURE = (0x03),
            PAGE_TIMEOUT = (0x04),
            AUTHENTICATION_FAILURE = (0x05),
            KEY_MISSING = (0x06),
            MEMORY_FULL = (0x07),
            CONNECTION_TIMEOUT = (0x08),
            MAX_NUMBER_OF_CONNECTIONS = (0x09),
            MAX_NUMBER_OF_SCO_CONNECTIONS = (0x0a),
            ACL_CONNECTION_ALREADY_EXISTS = (0x0b),
            COMMAND_DISALLOWED = (0x0c),
            HOST_REJECTED_LIMITED_RESOURCES = (0x0d),
            HOST_REJECTED_SECURITY_REASONS = (0x0e),
            HOST_REJECTED_PERSONAL_DEVICE = (0x0f),
            HOST_TIMEOUT = (0x10),
            UNSUPPORTED_FEATURE_OR_PARAMETER = (0x11),
            INVALID_HCI_PARAMETER = (0x12),
            REMOTE_USER_ENDED_CONNECTION = (0x13),
            REMOTE_LOW_RESOURCES = (0x14),
            REMOTE_POWERING_OFF = (0x15),
            LOCAL_HOST_TERMINATED_CONNECTION = (0x16),
            REPEATED_ATTEMPTS = (0x17),
            PAIRING_NOT_ALLOWED = (0x18),
            UKNOWN_LMP_PDU = (0x19),
            UNSUPPORTED_REMOTE_FEATURE = (0x1a),
            SCO_OFFSET_REJECTED = (0x1b),
            SCO_INTERVAL_REJECTED = (0x1c),
            SCO_AIRMODE_REJECTED = (0x1d),
            INVALID_LMP_PARAMETERS = (0x1e),
            UNSPECIFIED_ERROR = (0x1f),
            UNSUPPORTED_LMP_PARM_VALUE = (0x20),
            ROLE_CHANGE_NOT_ALLOWED = (0x21),
            LMP_RESPONSE_TIMEOUT = (0x22),
            LMP_TRANSACTION_COLLISION = (0x23),
            LMP_PDU_NOT_ALLOWED = (0x24),
            ENCRYPTION_MODE_NOT_ACCEPTABLE = (0x25),
            UNIT_KEY_NOT_USED = (0x26),
            QOS_IS_NOT_SUPPORTED = (0x27),
            INSTANT_PASSED = (0x28),
            PAIRING_WITH_UNIT_KEY_NOT_SUPPORTED = (0x29),
            DIFFERENT_TRANSACTION_COLLISION = (0x2a),
            QOS_UNACCEPTABLE_PARAMETER = (0x2c),
            QOS_REJECTED = (0x2d),
            CHANNEL_CLASSIFICATION_NOT_SUPPORTED = (0x2e),
            INSUFFICIENT_SECURITY = (0x2f),
            PARAMETER_OUT_OF_MANDATORY_RANGE = (0x30),
            ROLE_SWITCH_PENDING = (0x32),
            RESERVED_SLOT_VIOLATION = (0x34),
            ROLE_SWITCH_FAILED = (0x35),
            EXTENDED_INQUIRY_RESPONSE_TOO_LARGE = (0x36),
            SECURE_SIMPLE_PAIRING_NOT_SUPPORTED_BY_HOST = (0x37),
            HOST_BUSY_PAIRING = (0x38),

            // Added in Windows 8
            CONNECTION_REJECTED_DUE_TO_NO_SUITABLE_CHANNEL_FOUND = (0x39),
            CONTROLLER_BUSY = (0x3a),
            UNACCEPTABLE_CONNECTION_INTERVAL = (0x3b),
            DIRECTED_ADVERTISING_TIMEOUT = (0x3c),
            CONNECTION_TERMINATED_DUE_TO_MIC_FAILURE = (0x3d),
            CONNECTION_FAILED_TO_BE_ESTABLISHED = (0x3e),
            MAC_CONNECTION_FAILED = (0x3f),

            //
            UNSPECIFIED = (0xFF),
        }

        //for bluetooth events (Win32)
        // ... are now defined in BluetoothWin32Events.cs.

        internal const int BTH_MAX_NAME_SIZE = 248;
        internal const int BLUETOOTH_MAX_PASSKEY_SIZE = 16;

        //[DllImport(wsDll, EntryPoint = "WSAAddressToString", SetLastError = true)]
        //internal static extern int WSAAddressToString(byte[] lpsaAddress, int dwAddressLength, IntPtr lpProtocolInfo, System.Text.StringBuilder lpszAddressString, ref int lpdwAddressStringLength);

        //desktop methods

        /// <summary>
        /// The BluetoothAuthenticateDevice function sends an authentication request to a remote Bluetooth device.
        /// </summary>
        /// <param name="hwndParent">The window to parent the authentication wizard.
        /// If NULL, the wizard will be parented off the desktop.</param>
        /// <param name="hRadio">A valid local radio handle, or NULL. If NULL, authentication is attempted on all local radios; if any radio succeeds, the function call succeeds.</param>
        /// <param name="pbtdi">A structure of type BLUETOOTH_DEVICE_INFO that contains the record of the Bluetooth device to be authenticated.</param>
        /// <param name="pszPasskey">A Personal Identification Number (PIN) to be used for device authentication. If set to NULL, the user interface is displayed and and the user must follow the authentication process provided in the user interface. If pszPasskey is not NULL, no user interface is displayed. If the passkey is not NULL, it must be a NULL-terminated string. For more information, see the Remarks section.</param>
        /// <param name="ulPasskeyLength">The size, in characters, of pszPasskey.
        /// The size of pszPasskey must be less than or equal to BLUETOOTH_MAX_PASSKEY_SIZE.</param>
        /// <returns></returns>
        [DllImport(irpropsDll, SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern int BluetoothAuthenticateDevice(IntPtr hwndParent, IntPtr hRadio, ref BLUETOOTH_DEVICE_INFO pbtdi, string pszPasskey, int ulPasskeyLength);

        /// <summary>
        /// The BluetoothAuthenticateDeviceEx function sends an authentication request to a remote Bluetooth device. Additionally, this function allows for out-of-band data to be passed into the function call for the device being authenticated.
        /// Note  This API is supported in Windows Vista SP2 and Windows 7.
        /// </summary>
        /// <param name="hwndParentIn">The window to parent the authentication wizard.
        /// If NULL, the wizard will be parented off the desktop.</param>
        /// <param name="hRadioIn">A valid local radio handle or NULL.
        /// If NULL, then all radios will be tried. If any of the radios succeed, then the call will succeed.</param>
        /// <param name="pbtdiInout">A pointer to a BLUETOOTH_DEVICE_INFO structure describing the device being authenticated.</param>
        /// <param name="pbtOobData">Pointer to device specific out-of-band data to be provided with this API call.
        /// If NULL, then UI is displayed to continue the authentication process.
        /// If not NULL, no UI is displayed.</param>
        /// <param name="authenticationRequirement">An AUTHENTICATION_REQUIREMENTS enumeration that specifies the protection required for authentication.</param>
        /// <returns></returns>
        [DllImport(bthpropsDll, SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern int BluetoothAuthenticateDeviceEx(IntPtr hwndParentIn, IntPtr hRadioIn, ref BLUETOOTH_DEVICE_INFO pbtdiInout, byte[] pbtOobData, BluetoothAuthenticationRequirements authenticationRequirement);

        // This method is deprecated and other stacks have no equivalent so do not implement
        //[DllImport(irpropsDll, SetLastError = true, CharSet = CharSet.Unicode)]
        //internal static extern int BluetoothAuthenticateMultipleDevices(IntPtr hwndParent, IntPtr hRadio, int cDevices, BLUETOOTH_DEVICE_INFO[] rgbtdi);

        [DllImport(irpropsDll, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool BluetoothDisplayDeviceProperties(IntPtr hwndParent, ref BLUETOOTH_DEVICE_INFO pbtdi);

        [DllImport(irpropsDll, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool BluetoothEnableDiscovery(WindowsRadioHandle hRadio, bool fEnabled);

        [DllImport(irpropsDll, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool BluetoothEnableIncomingConnections(WindowsRadioHandle hRadio, bool fEnabled);

        [DllImport(irpropsDll, SetLastError = true)]
        internal static extern int BluetoothEnumerateInstalledServices(IntPtr hRadio, ref BLUETOOTH_DEVICE_INFO pbtdi, ref int pcServices, byte[] pGuidServices);

        [DllImport(irpropsDll, SetLastError = true)]
        internal static extern int BluetoothSetServiceState(IntPtr hRadio, ref BLUETOOTH_DEVICE_INFO pbtdi, ref Guid pGuidService, int dwServiceFlags);

        //Radio

        // (Tried to use WindowsRadioHandle instead of IntPtr for the second 
        // param but got a missing method type exception.)
        [DllImport(irpropsDll, SetLastError = true)]
        internal static extern IntPtr BluetoothFindFirstRadio(ref BLUETOOTH_FIND_RADIO_PARAMS pbtfrp, out IntPtr phRadio);

        [DllImport(irpropsDll, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool BluetoothFindNextRadio(IntPtr hFind, out IntPtr phRadio);

        [DllImport(irpropsDll, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool BluetoothFindRadioClose(IntPtr hFind);


        [DllImport(irpropsDll, SetLastError = true)]
        internal static extern int BluetoothGetDeviceInfo(IntPtr hRadio, ref BLUETOOTH_DEVICE_INFO pbtdi);

        [DllImport(irpropsDll, SetLastError = true)]
        internal static extern int BluetoothGetRadioInfo(WindowsRadioHandle hRadio, ref BLUETOOTH_RADIO_INFO pRadioInfo);


        [DllImport(irpropsDll, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool BluetoothIsConnectable(WindowsRadioHandle hRadio);

        [DllImport(irpropsDll, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool BluetoothIsDiscoverable(WindowsRadioHandle hRadio);

        [DllImport(irpropsDll, SetLastError = true)]
        internal static extern int BluetoothUpdateDeviceRecord(ref BLUETOOTH_DEVICE_INFO pbtdi);


        //XP SDP Parsing
        [DllImport(irpropsDll, SetLastError = true)]
        internal static extern int BluetoothSdpGetAttributeValue(byte[] pRecordStream, int cbRecordLength, ushort usAttributeId, IntPtr pAttributeData);

        [DllImport(irpropsDll, SetLastError = true)]
        internal static extern int BluetoothSdpGetContainerElementData(byte[] pContainerStream, uint cbContainerLength, ref IntPtr pElement, byte[] pData);

        [DllImport(irpropsDll, SetLastError = true)]
        internal static extern int BluetoothSdpGetElementData(byte[] pSdpStream, uint cbSpdStreamLength, byte[] pData);

        [DllImport(irpropsDll, SetLastError = true)]
        internal static extern int BluetoothSdpGetString(byte[] pRecordStream, uint cbRecordLength,
            /*PSDP_STRING_DATA_TYPE*/IntPtr pStringData, ushort usStringOffset, byte[] pszString, ref uint pcchStringLength);

        internal struct SDP_STRING_TYPE_DATA
        {
            internal ushort encoding;
            internal ushort mibeNum;
            internal ushort attributeID;

            void ShutUpCompiler()
            {
                encoding = 0;
                mibeNum = 0;
                attributeID = 0;
            }
        }

        [DllImport(irpropsDll, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool BluetoothSdpEnumAttributes(
            IntPtr pSDPStream,
            int cbStreamSize,
            BluetoothEnumAttributesCallback pfnCallback,
            IntPtr pvParam);

        [return: MarshalAs(UnmanagedType.Bool)]
        internal delegate bool BluetoothEnumAttributesCallback(
            uint uAttribId,
            IntPtr pValueStream,
            int cbStreamSize,
            IntPtr pvParam);

        //Authentication functions

        [DllImport(irpropsDll, SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern UInt32 BluetoothRegisterForAuthentication(
            ref BLUETOOTH_DEVICE_INFO pbtdi,
            out BluetoothAuthenticationRegistrationHandle phRegHandle,
            BluetoothAuthenticationCallback pfnCallback,
            IntPtr pvParam);

        //Requires Vista SP2 or later
        [DllImport(bthpropsDll, SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern UInt32 BluetoothRegisterForAuthenticationEx(
            ref BLUETOOTH_DEVICE_INFO pbtdi,
            out BluetoothAuthenticationRegistrationHandle phRegHandle,
            BluetoothAuthenticationCallbackEx pfnCallback,
            IntPtr pvParam);

        [return: MarshalAs(UnmanagedType.Bool)] // Does this have any effect?
        internal delegate bool BluetoothAuthenticationCallback(IntPtr pvParam, ref BLUETOOTH_DEVICE_INFO bdi);

        [return: MarshalAs(UnmanagedType.Bool)] // Does this have any effect?
        internal delegate bool BluetoothAuthenticationCallbackEx(IntPtr pvParam, ref BLUETOOTH_AUTHENTICATION_CALLBACK_PARAMS pAuthCallbackParams);

        [DllImport(irpropsDll, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        [System.Security.SuppressUnmanagedCodeSecurity] // Since called from SafeHandle
        internal static extern bool BluetoothUnregisterAuthentication(IntPtr hRegHandle);

        [DllImport(bthpropsDll, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        [System.Security.SuppressUnmanagedCodeSecurity] // Since called from SafeHandle
        internal static extern bool BluetoothUnregisterAuthenticationEx(IntPtr hRegHandle);

        [DllImport(irpropsDll, SetLastError = false, CharSet = CharSet.Unicode)]
        internal static extern Int32 BluetoothSendAuthenticationResponse(IntPtr hRadio, ref BLUETOOTH_DEVICE_INFO pbtdi, string pszPasskey);

        //[DllImport("bthprops.cpl", SetLastError = false, CharSet = CharSet.Unicode)]
        //internal static extern Int32 BluetoothSendAuthenticationResponseEx(IntPtr hRadio, ref BLUETOOTH_AUTHENTICATE_RESPONSE pauthResponse);
        [DllImport(bthpropsDll, SetLastError = false, CharSet = CharSet.Unicode)]
        internal static extern Int32 BluetoothSendAuthenticationResponseEx(IntPtr hRadio, ref BLUETOOTH_AUTHENTICATE_RESPONSE__PIN_INFO pauthResponse);
        [DllImport(bthpropsDll, SetLastError = false, CharSet = CharSet.Unicode)]
        internal static extern Int32 BluetoothSendAuthenticationResponseEx(IntPtr hRadio, ref BLUETOOTH_AUTHENTICATE_RESPONSE__OOB_DATA_INFO pauthResponse);
        [DllImport(bthpropsDll, SetLastError = false, CharSet = CharSet.Unicode)]
        internal static extern Int32 BluetoothSendAuthenticationResponseEx(IntPtr hRadio, ref BLUETOOTH_AUTHENTICATE_RESPONSE__NUMERIC_COMPARISON_PASSKEY_INFO pauthResponse);

        [DllImport(irpropsDll, SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern int BluetoothRemoveDevice(byte[] pAddress);

        // Code for setting radio name


        // devguid.h
        internal static readonly Guid GUID_DEVCLASS_BLUETOOTH = new Guid("{E0CBF06C-CD8B-4647-BB8A-263B43F0F974}");



        // SETUPAPI.H
        [Flags()]
        internal enum DIGCF
        {
            PRESENT = 0x00000002, // Return only devices that are currently present in a system.
            ALLCLASSES = 0x00000004, // Return a list of installed devices for all device setup classes or all device interface classes. 
            PROFILE = 0x00000008, // Return only devices that are a part of the current hardware profile.
        }

        internal enum SPDRP
        {
            HARDWAREID = 0x00000001,  // HardwareID (R/W)
            DRIVER = 0x00000009,  // Driver (R/W)
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SP_DEVINFO_DATA
        {
            internal int cbSize; // = (uint)Marshal.SizeOf(typeof(SP_DEVINFO_DATA));
            internal Guid ClassGuid;
            internal UInt32 DevInst;
            internal IntPtr Reserved;
        }

        // ref Int64 -> p
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool DeviceIoControl(
            IntPtr hDevice,
            uint dwIoControlCode,
            ref long InBuffer,
            int nInBufferSize,
            IntPtr OutBuffer,
            int nOutBufferSize,
            out int pBytesReturned,
            IntPtr lpOverlapped);

        // p -> byte[]
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool DeviceIoControl(
            IntPtr hDevice,
            uint dwIoControlCode,
            IntPtr InBuffer,
            int nInBufferSize,
            byte[] OutBuffer,
            int nOutBufferSize,
            out int pBytesReturned,
            IntPtr lpOverlapped);

        // p -> byte[]
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeviceIoControl(
            WindowsRadioHandle hDevice,
            uint dwIoControlCode,
            IntPtr InBuffer,
            int nInBufferSize,
            byte[] OutBuffer,
            int nOutBufferSize,
            out int pBytesReturned,
            IntPtr lpOverlapped);

        // byte[]-> byte[]
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool DeviceIoControl(
            IntPtr hDevice,
            uint dwIoControlCode,
            byte[] InBuffer,
            int nInBufferSize,
            byte[] OutBuffer,
            int nOutBufferSize,
            out int pBytesReturned,
            IntPtr lpOverlapped);

        // ref Int64 (btAddr) -> BTH_RADIO_INFO
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool DeviceIoControl(
            IntPtr hDevice,
            uint dwIoControlCode,
            ref long InBuffer,
            int nInBufferSize,
            ref BTH_RADIO_INFO OutBuffer,
            int nOutBufferSize,
            out int pBytesReturned,
            IntPtr lpOverlapped);

        // From WinDDK inc\ddk\bthioctl.h
        internal static class MsftWin32BthIOCTL //: uint
        {
            // Every one fails with ERROR_NOT_SUPPORTED (50) on WindowsXP.
            // Most (should) work on Vista.
            // Most do work on Windows 7.

            // Kernel
            const uint IOCTL_INTERNAL_BTH_SUBMIT_BRB = 0x410003;
            const uint IOCTL_INTERNAL_BTHENUM_GET_ENUMINFO = 0x410007;
            const uint IOCTL_INTERNAL_BTHENUM_GET_DEVINFO = 0x41000b;

            // Other

            /// <summary>
            /// Input:  none
            /// Output:  BTH_LOCAL_RADIO_INFO
            /// </summary>
            internal const uint IOCTL_BTH_GET_LOCAL_INFO = 0x410000;
            /// <summary>
            /// Input:  BTH_ADDR
            /// Output:  BTH_RADIO_INFO
            /// </summary>
            internal const uint IOCTL_BTH_GET_RADIO_INFO = 0x410004;
            /// <summary>
            /// use this ioctl to get a list of cached discovered devices in the port driver.
            ///
            /// Input: None
            /// Output: BTH_DEVICE_INFO_LIST
            /// </summary>
            internal const uint IOCTL_BTH_GET_DEVICE_INFO = 0x410008;
            /// <summary>
            /// Input:  BTH_ADDR
            /// Output:  none
            /// </summary>
            internal const uint IOCTL_BTH_DISCONNECT_DEVICE = 0x41000c;
            //
            //#if (NTDDI_VERSION > NTDDI_VISTASP1 || \
            //    (NTDDI_VERSION == NTDDI_VISTASP1 && defined(VISTA_KB942567)))
            //
            //#ifdef FULL_EIR_SUPPORT // in WUR this funcitonality is disabled
            //
            // These five fail with ERROR_NOT_SUPPORTED (50) on Windows 7
            /// <summary>
            /// Input:   BTH_GET_DEVICE_RSSI
            /// Output:  ULONG
            /// </summary>
            internal const uint IOCTL_BTH_GET_DEVICE_RSSI = 0x410014;
            /// <summary>
            /// Input:   BTH_EIR_GET_RECORDS
            /// Output:  UCHAR array, sequence of length + type + data fields triplets.
            /// </summary>
            internal const uint IOCTL_BTH_EIR_GET_RECORDS = 0x410040;
            /// <summary>
            /// Input:  BTH_EIR_SUBMIT_RECORD
            /// Output  HANDLE
            /// </summary>
            internal const uint IOCTL_BTH_EIR_SUBMIT_RECORD = 0x410044;
            /// <summary>
            /// Input:  BTH_EIR_SUBMIT_RECORD
            /// Output  None
            /// </summary>
            internal const uint IOCTL_BTH_EIR_UPDATE_RECORD = 0x410048;
            /// <summary>
            /// Input:   HANDLE
            /// Output:  None
            /// </summary>
            internal const uint IOCTL_BTH_EIR_REMOVE_RECORD = 0x41004c;
            //#endif // FULL_EIR_SUPPORT
            //
            /// <summary>
            /// Input:   BTH_VENDOR_SPECIFIC_COMMAND 
            /// Output:  PVOID
            /// </summary>
            internal const uint IOCTL_BTH_HCI_VENDOR_COMMAND = 0x410050;
            //#endif // >= SP1+KB942567
            //
            /// <summary>
            /// Input:  BTH_SDP_CONNECT
            /// Output:  BTH_SDP_CONNECT
            /// </summary>
            internal const uint IOCTL_BTH_SDP_CONNECT = 0x410200;
            /// <summary>
            /// Input:  HANDLE_SDP
            /// Output:  none
            /// </summary>
            internal const uint IOCTL_BTH_SDP_DISCONNECT = 0x410204;
            /// <summary>
            /// Input:  BTH_SDP_SERVICE_SEARCH_REQUEST
            /// Output:  ULONG * number of handles wanted
            /// </summary>
            internal const uint IOCTL_BTH_SDP_SERVICE_SEARCH = 0x410208;
            /// <summary>
            /// Input:  BTH_SDP_ATTRIBUTE_SEARCH_REQUEST
            /// Output:  BTH_SDP_STREAM_RESPONSE or bigger
            /// </summary>
            internal const uint IOCTL_BTH_SDP_ATTRIBUTE_SEARCH = 0x41020c;
            /// <summary>
            /// Input:  BTH_SDP_SERVICE_ATTRIBUTE_SEARCH_REQUEST
            /// Output:  BTH_SDP_STREAM_RESPONSE or bigger
            /// </summary>
            internal const uint IOCTL_BTH_SDP_SERVICE_ATTRIBUTE_SEARCH = 0x410210;
            /// <summary>
            /// Input:  raw SDP stream (at least 2 bytes)
            /// Ouptut: HANDLE_SDP
            /// </summary>
            internal const uint IOCTL_BTH_SDP_SUBMIT_RECORD = 0x410214;
            /// <summary>
            /// Input:  HANDLE_SDP
            /// Output:  none
            /// </summary>
            internal const uint IOCTL_BTH_SDP_REMOVE_RECORD = 0x410218;
            /// <summary>
            /// Input:  BTH_SDP_RECORD + raw SDP record
            /// Output:  HANDLE_SDP
            /// </summary>
            internal const uint IOCTL_BTH_SDP_SUBMIT_RECORD_WITH_INFO = 0x41021c;
        }

        // The SetupDiGetClassDevs function returns a handle to a device information set that contains requested device information elements for a local machine. 
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern IntPtr SetupDiGetClassDevs(
            ref Guid classGuid,
            [MarshalAs(UnmanagedType.LPTStr)] string enumerator,
            IntPtr hwndParent,
            DIGCF flags);

        // The SetupDiEnumDeviceInfo function returns a SP_DEVINFO_DATA structure that specifies a device information element in a device information set. 
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetupDiEnumDeviceInfo(
            IntPtr deviceInfoSet,
            uint memberIndex,
            ref SP_DEVINFO_DATA deviceInfoData);

        // The SetupDiDestroyDeviceInfoList function deletes a device information set and frees all associated memory.
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetupDiDestroyDeviceInfoList(IntPtr deviceInfoSet);

        // The SetupDiGetDeviceInstanceId function retrieves the device instance ID that is associated with a device information element.
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetupDiGetDeviceInstanceId(
           IntPtr deviceInfoSet,
           ref SP_DEVINFO_DATA deviceInfoData,
           System.Text.StringBuilder deviceInstanceId,
           int deviceInstanceIdSize,
           out int requiredSize);

        //The BluetoothIsVersionAvailable function indicates if the installed Bluetooth binary set supports the requested version.
        //Requires Windows XP SP2 or later
        [DllImport(bthpropsDll, SetLastError = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool BluetoothIsVersionAvailable(byte MajorVersion, byte MinorVersion);


#endif


        //--------
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseHandle(IntPtr handle);
    }

    internal enum WSAESETSERVICEOP : int
    {
        /// <summary>
        /// Register the service. For SAP, this means sending out a periodic broadcast.
        /// This is an NOP for the DNS namespace.
        /// For persistent data stores, this means updating the address information. 
        /// </summary>
        RNRSERVICE_REGISTER = 0,
        /// <summary>
        ///  Remove the service from the registry.
        ///  For SAP, this means stop sending out the periodic broadcast.
        ///  This is an NOP for the DNS namespace.
        ///  For persistent data stores this means deleting address information. 
        /// </summary>
        RNRSERVICE_DEREGISTER,
        /// <summary>
        /// Delete the service from dynamic name and persistent spaces.
        /// For services represented by multiple CSADDR_INFO structures (using the SERVICE_MULTIPLE flag), only the specified address will be deleted, and this must match exactly the corresponding CSADDR_INFO structure that was specified when the service was registered 
        /// </summary>
        RNRSERVICE_DELETE,

    }

    [Flags()]
    internal enum LookupFlags : uint
    {
        Containers = 0x0002,
        ReturnName = 0x0010,
        ReturnAddr = 0x0100,
        ReturnBlob = 0x0200,
        FlushCache = 0x1000,
        ResService = 0x8000,
    }

    [Flags()]
    internal enum WinCeScanMask : byte
    {
        None = 0,
        InquiryScan = 1,
        PageScan = 2,
    }
}
