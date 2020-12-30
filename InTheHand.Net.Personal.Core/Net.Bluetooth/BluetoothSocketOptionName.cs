// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.BluetoothSocketOptionName
// 
// Copyright (c) 2003-2008 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Net.Sockets;

namespace InTheHand.Net.Sockets
{
    /// <summary>
    /// Defines <see cref="T:System.Net.Sockets.Socket"/> configuration option names for the <see cref="T:System.Net.Sockets.Socket"/> class.
    /// </summary>
    public static class BluetoothSocketOptionName
    {
        /// <summary>
		/// Toggles authentication under Windows.
		/// </summary>
		/// <remarks>optlen=sizeof(ULONG), optval = &amp;(ULONG)TRUE/FALSE</remarks>
        public const SocketOptionName Authenticate = unchecked((SocketOptionName) 0x80000001); // optlen=sizeof(ULONG), optval = &(ULONG)TRUE/FALSE 

        /// <summary>
		/// On a connected socket, this command turns encryption on or off.
		/// On an unconnected socket, this forces encryption to be on or off on connection.
		/// For an incoming connection, this means that the connection is rejected if the encryption cannot be turned on.
		/// </summary>
        public const SocketOptionName Encrypt = (SocketOptionName)0x00000002; // optlen=sizeof(unsigned int), optval = &amp;(unsigned int)TRUE/FALSE

        /// <summary>
        /// Get or set the default MTU on Windows.
        /// </summary>
        /// <remarks>optlen=sizeof(ULONG), optval = &amp;mtu</remarks>
        public const SocketOptionName Mtu = unchecked((SocketOptionName)0x80000007); // optlen=sizeof(ULONG), optval = &mtu

        /// <summary>
		/// Get or set the maximum MTU on Windows.
		/// </summary>
		/// <remarks>optlen=sizeof(ULONG), optval = &amp;max. mtu</remarks>
        public const SocketOptionName MtuMaximum = unchecked((SocketOptionName)0x80000008);// - 2147483640;

        /// <summary>
		/// Get or set the minimum MTU on Windows.
		/// </summary>
		/// <remarks>optlen=sizeof(ULONG), optval = &amp;min. mtu</remarks>
        public const SocketOptionName MtuMinimum = unchecked((SocketOptionName)0x8000000a);// - 2147483638;

#if NETCF
        /// <summary>
        /// On connected socket, triggers authentication.
        /// On not connected socket, forces authentication on connection.
        /// For incoming connection this means that connection is rejected if authentication cannot be performed.
        /// </summary>
        /// <remarks>Windows CE only. The optval and optlen parameters are ignored; however, Winsock implementation on Windows CE requires optlen to be at least 4 and optval to point to at least an integer datum.</remarks>
        public const SocketOptionName AuthenticateCE	= (SocketOptionName)0x00000001;
		/// <summary>
		/// This sets or revokes PIN code to use with a connection or socket.
		/// </summary>
        public const SocketOptionName SetPin = (SocketOptionName)0x00000003;	// bound only! survives socket! optlen=sizeof(BTH_SOCKOPT_SECURITY), optval=&amp;BTH_SOCKOPT_SECURITY
		/// <summary>
		/// This sets or revokes link key to use with a connection or peer device.
		/// </summary>
        public const SocketOptionName SetLink = (SocketOptionName)0x00000004;	// bound only! survives socket! optlen=sizeof(BTH_SOCKOPT_SECURITY), optval=&amp;BTH_SOCKOPT_SECURITY
		/// <summary>
		/// Returns link key associated with peer Bluetooth device.
		/// </summary>
        public const SocketOptionName GetLink = (SocketOptionName)0x00000005;	// bound only! optlen=sizeof(BTH_SOCKOPT_SECURITY), optval=&amp;BTH_SOCKOPT_SECURITY
		/// <summary>
		/// This sets default MTU (maximum transmission unit) for connection negotiation.
		/// While allowed for connected socket, it has no effect if the negotiation has already completed.
		/// Setting it on listening socket will propagate the value for all incoming connections.
		/// </summary>
        public const SocketOptionName SetMtu = (SocketOptionName)0x00000006;	// unconnected only! optlen=sizeof(unsigned int), optval = &mtu
		/// <summary>
		/// Returns MTU (maximum transmission unit).
		/// For connected socket, this is negotiated value, for server (accepting) socket it is MTU proposed for negotiation on connection request.
		/// </summary>
        public const SocketOptionName GetMtu = (SocketOptionName)0x00000007;	// optlen=sizeof(unsigned int), optval = &amp;mtu
		/// <summary>
		/// This sets maximum MTU for connection negotiation.
		/// While allowed for connected socket, it has no effect if the negotiation has already completed.
		/// Setting it on listening socket will propagate the value for all incoming connections.
		/// </summary>
        public const SocketOptionName SetMtuMaximum = (SocketOptionName)0x00000008;	// unconnected only! optlen=sizeof(unsigned int), optval = &max. mtu
		/// <summary>
		/// Returns maximum MTU acceptable MTU value for a connection on this socket.
		/// Because negotiation has already happened, has little meaning for connected socket.
		/// </summary>
        public const SocketOptionName GetMtuMaximum = (SocketOptionName)0x00000009;	// bound only! optlen=sizeof(unsigned int), optval = &amp;max. mtu
		/// <summary>
		/// This sets minimum MTU for connection negotiation.
		/// While allowed for connected socket, it has no effect if the negotiation has already completed.
		/// Setting it on listening socket will propagate the value for all incoming connections.
		/// </summary>
        public const SocketOptionName SetMtuMinimum = (SocketOptionName)0x0000000a;	// unconnected only! optlen=sizeof(unsigned int), optval = &amp;min. mtu
		/// <summary>
		/// Returns minimum MTU acceptable MTU value for a connection on this socket.
		/// Because negotiation has already happened, has little meaning for connected socket. 
		/// </summary>
        public const SocketOptionName GetMtuMinimum = (SocketOptionName)0x0000000b;	// bound only! optlen=sizeof(unsigned int), optval = &min. mtu
		/// <summary>
		/// This sets XON limit.
		/// Setting it on listening socket will propagate the value for all incoming connections.
		/// </summary>
        public const SocketOptionName SetXOnLimit = (SocketOptionName)0x0000000c;	// optlen=sizeof(unsigned int), optval = &xon limit (set flow off)
		/// <summary>
		/// Returns XON limit for a connection.
		/// XON limit is only used for peers that do not support credit-based flow control (mandatory in the Bluetooth Core Specification version 1.1).
		/// When amount of incoming data received, but not read by an application for a given connection grows past this limit, a flow control command is sent to the peer requiring suspension of transmission.
		/// </summary>
        public const SocketOptionName GetXOnLimit = (SocketOptionName)0x0000000d;	// optlen=sizeof(unsigned int), optval = &xon
		/// <summary>
		/// This sets XOFF limit.
		/// Setting it on listening socket will propagate the value for all incoming connections.
		/// </summary>
        public const SocketOptionName SetXOffLimit = (SocketOptionName)0x0000000e;	// optlen=sizeof(unsigned int), optval = &xoff limit (set flow on)
		/// <summary>
		/// Returns XOFF limit for a connection.
		/// XOFF limit is only used for peers that do not support credit-based flow control (mandatory in the Bluetooth Core Specification 1.1).
		/// If flow has been suspended because of buffer run-up, when amount of incoming data received, but not read by an application for a given connection falls below this limit, a flow control command is sent to the peer allowing continuation of transmission.
		/// </summary>
        public const SocketOptionName GetXOffLimit = (SocketOptionName)0x0000000f;	// optlen=sizeof(unsigned int), optval = &xoff
		/// <summary>
		/// Specifies maximum amount of data that can be buffered inside RFCOMM (this is amount of data before call to send blocks).
		/// </summary>
        public const SocketOptionName SetSendBuffer = (SocketOptionName)0x00000010;	// optlen=sizeof(unsigned int), optval = &max buffered size for send
		/// <summary>
		///  Returns maximum amount of data that can be buffered inside RFCOMM (this is amount of data before call to send blocks).
		/// </summary>
        public const SocketOptionName GetSendBuffer = (SocketOptionName)0x00000011;	// optlen=sizeof(unsigned int), optval = &max buffered size for send
		/// <summary>
		/// Specifies maximum amount of data that can be buffered for a connection.
		/// This buffer size is used to compute number of credits granted to peer device when credit-based flow control is implemented.
		/// This specifies the maximum amount of data that can be buffered.
		/// </summary>
        public const SocketOptionName SetReceiveBuffer = (SocketOptionName)0x00000012;	// optlen=sizeof(unsigned int), optval = &max buffered size for recv
		/// <summary>
		/// Returns maximum amount of data that can be buffered for a connection.
		/// This buffer size is used to compute number of credits granted to peer device when credit-based flow control is implemented.
		/// This specifies the maximum amount of data that can be buffered.
		/// </summary>
        public const SocketOptionName GetReceiveBuffer = (SocketOptionName)0x00000013;	// optlen=sizeof(unsigned int), optval = &max buffered size for recv
		/// <summary>
		/// Retrieves last v24 and break signals set through MSC command from peer device.
		/// </summary>
        public const SocketOptionName GetV24Break = (SocketOptionName)0x00000014;	// connected only! optlen=2*sizeof(unsigned int), optval = &{v24 , br}
		/// <summary>
		/// Retrieves last line status signals set through RLS command from peer device.
		/// </summary>
        public const SocketOptionName GetRls = (SocketOptionName)0x00000015;	// connected only! optlen=sizeof(unsigned int), optval = &rls
		/// <summary>
		/// Sends MSC command. V24 and breaks are as specified in RFCOMM Specification.
		/// Only modem signals and breaks can be controlled, RFCOMM reserved fields such as flow control are ignored and should be set to 0.
		/// </summary>
        public const SocketOptionName SendMsc = (SocketOptionName)0x00000016;	// connected only! optlen=2*sizeof(unsigned int), optval = &{v24, br}
		/// <summary>
		/// Sends RLS command.
		/// Argument is as specified in RFCOMM Specification.
		/// </summary>
        public const SocketOptionName SendRls = (SocketOptionName)0x00000017;	// connected only! optlen=sizeof(unsigned int), optval = &rls
		/// <summary>
		/// Gets flow control type on the connected socket.
		/// </summary>
        public const SocketOptionName GetFlowType = (SocketOptionName)0x00000018;	// connected only! optlen=sizeof(unsigned int), optval=&1=credit-based, 0=legacy
		/// <summary>
		/// Sets the page timeout for the card.
		/// The socket does not have to be connected.
		/// </summary>
        public const SocketOptionName SetPageTimeout = (SocketOptionName)0x00000019;	// no restrictions. optlen=sizeof(unsigned int), optval = &page timeout
		/// <summary>
		/// Gets the current page timeout.
		/// The socket does not have to be connected.
		/// </summary>
        public const SocketOptionName GetPageTimeout = (SocketOptionName)0x0000001a;	// no restrictions. optlen=sizeof(unsigned int), optval = &page timeout
		/// <summary>
		/// Sets the scan mode for the card.
		/// The socket does not have to be connected.
		/// </summary>
        public const SocketOptionName SetScan = (SocketOptionName)0x0000001b;	// no restrictions. optlen=sizeof(unsigned int), optval = &scan mode
		/// <summary>
		/// Gets the current scan mode.
		/// The socket does not have to be connected.
		/// </summary>
        public const SocketOptionName GetScan = (SocketOptionName)0x0000001c;	// no restrictions. optlen=sizeof(unsigned int), optval = &scan mode

		/// <summary>
		/// Sets the class of the device.
		/// The socket does not have to be connected.
		/// </summary>
        public const SocketOptionName SetCod = (SocketOptionName)0x0000001d;	// no restrictions. 
		/// <summary>
		/// Retrieve the Class of Device.
		/// </summary>
        public const SocketOptionName GetCod = (SocketOptionName)0x0000001e;	// no restrictions. optlen=sizeof(unsigned int), optval = &cod
		/// <summary>
		/// Get the version information from the Bluetooth adapter.
		/// </summary>
        public const SocketOptionName GetLocalVersion = (SocketOptionName)0x0000001f; // no restrictions. 

		/// <summary>
		/// Get the version of the remote adapter.
		/// </summary>
        public const SocketOptionName GetRemoteVersion = (SocketOptionName)0x00000020;	// connected only! optlen=sizeof(BTH_REMOTE_VERSION), optval = &BTH_REMOTE_VERSION
		
		/// <summary>
		/// Retrieves the authentication settings.
		/// The socket does not have to be connected.
		/// </summary>
        public const SocketOptionName GetAuthenticationEnabled = (SocketOptionName)0x00000021;	// no restrictions. optlen=sizeof(unsigned int), optval = &authentication enable
		/// <summary>
		/// Sets the authentication policy of the device.
		/// </summary>
        public const SocketOptionName SetAuthenticationEnabled = (SocketOptionName)0x00000022;	// no restrictions. optlen=sizeof(unsigned int), optval = &authentication enable

		/// <summary>
		/// Reads the remote name of the device.
		/// The socket does not have to be connected.
		/// </summary>
        public const SocketOptionName ReadRemoteName = (SocketOptionName)0x00000023;	// no restrictions.

		/// <summary>
		/// Retrieves the link policy of the device.
		/// </summary>
        public const SocketOptionName GetLinkPolicy = (SocketOptionName)0x00000024;	// connected only! optlen=sizeof(unsigned int), optval = &link policy
		/// <summary>
		/// Sets the link policy for an existing baseband connection.
		/// The socket must be connected.
		/// </summary>
        public const SocketOptionName SetLinkPolicy = (SocketOptionName)0x00000025;	// connected only! optlen=sizeof(unsigned int), optval = &link policy
		
		/// <summary>
        /// Places the ACL connection to the specified peer device in HOLD mode.
        /// The device must be connected.
		/// </summary>
        public const SocketOptionName EnterHoldMode = (SocketOptionName)0x00000026;  // connected only! optlen=sizeof(BTH_HOLD_MODE), optval = &BTH_HOLD_MODE
		/// <summary>
        /// Places the ACL connection to the specified peer device in SNIFF mode.
        /// The device must be connected.
		/// </summary>
        public const SocketOptionName EnterSniffMode = (SocketOptionName)0x00000027;  // connected only! optlen=sizeof(BTH_SNIFF_MODE), optval = &BTH_SNIFF_MODE
		/// <summary>
        /// Forces the ACL connection to the peer device to leave SNIFF mode.
        /// The device must be connected.
		/// </summary>
        public const SocketOptionName ExitSniffMode = (SocketOptionName)0x00000028;  // connected only! optlen=0, optval - ignored
		/// <summary>
        /// Places the ACL connection to the peer device in PARK mode.
        /// The device must be connected.
		/// </summary>
        public const SocketOptionName EnterParkMode = (SocketOptionName)0x00000029;  // connected only! optlen=sizeof(BTH_PARK_MODE), optval = &BTH_PARK_MODE
		/// <summary>
        /// Forces the ACL connection to the peer device to leave PARK mode.
        /// The device must be connected.
		/// </summary>
        public const SocketOptionName ExitParkMode = (SocketOptionName)0x0000002a;  // connected only! optlen=0, optval - ignored
		/// <summary>
		/// Gets the current mode of the connection.
		/// The mode can either be sniff, park, or hold. The socket must be connected.
		/// </summary>
        public const SocketOptionName GetMode = (SocketOptionName)0x0000002b;	// connected only! optlen=sizeof(int), optval = &mode
#endif
    }
}
