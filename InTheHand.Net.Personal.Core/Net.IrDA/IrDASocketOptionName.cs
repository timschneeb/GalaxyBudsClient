// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Sockets.IrDASocketOptionName
// 
// Copyright (c) 2003-2006 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

#region Using directives

using System;
using System.Net.Sockets;

#endregion

namespace InTheHand.Net.Sockets
{
    // AJMcF
    /// <summary>
    /// Socket option constants to set IrDA specific connection modes, and 
    /// get/set IrDA specific features.
    /// </summary>
    /// <remarks>
    /// Socket option constants to set IrDA specific connection modes, and 
    /// get/set IrDA specific features: 
    /// for instance to set IrLMP mode, or get the maximum send size.  Pass 
    /// to <see cref="M:System.Net.Sockets.Socket.GetSocketOption(System.Net.Sockets.SocketOptionLevel,System.Net.Sockets.SocketOptionName,System.Byte[])"/>/etc and
    /// <see cref="M:System.Net.Sockets.Socket.SetSocketOption(System.Net.Sockets.SocketOptionLevel,System.Net.Sockets.SocketOptionName,System.Byte[])"/>/etc,  
    /// along with optionLevel <c>IrDASocketOptionLevel.</c><see 
    /// cref="F:InTheHand.Net.Sockets.IrDASocketOptionLevel.IrLmp"/>; 
    /// see the examples below.
    /// <para><b>New in v1.5.51015</b></para>
    /// </remarks>
    /// <example><para>For instance, where <c>cli</c> is an instance of 
    /// <see cref="T:InTheHand.Net.Sockets.IrDAClient"/>.</para>
    /// In VB.NET, to set IrLMP mode (<c>IrLptMode</c>).
    /// <code lang="VB.NET">
    /// cli.Client.SetSocketOption(IrDASocketOptionLevel.Irlmp,  _
    ///    IrDASocketOptionName.IrLptMode, _
    ///    1) 'representing true; can use True itself in FXv2.
    /// </code>
    /// In C#, to retrieve the maximum send size.
    /// <code lang="C#">
    /// int maxSendSize = (int)cli.Client.GetSocketOption(
    ///    IrDASocketOptionLevel.Irlmp,
    ///    IrDASocketOptionName.SendPduLength);
    /// </code>
    /// </example>
	public static class IrDASocketOptionName
	{
        /// <summary>
        /// Gets the list of discovered devices.  
        /// Is used internally by <c>IrDAClient.DiscoverDevices</c>.  
        /// </summary>
        /// <remarks>
        /// In native terms takes a <c>DEVICE_LIST</c> struct.
        /// </remarks>
        public const SocketOptionName EnumDevice = (SocketOptionName)0x00000010;

        /// <summary>
        /// Sets an entry in the local IAS (Information Access Service) database.
        /// </summary>
        /// <remarks>
        /// In native terms takes a <c>IAS_SET</c> struct.
        /// </remarks>
        public const SocketOptionName IasSet = (SocketOptionName)0x00000011;

        /// <summary>
        /// Queries an entry in the peer's IAS (Information Access Service) database.
        /// </summary>
        /// <remarks>
        /// In native terms takes a <c>IAS_QUERY</c> struct.
        /// </remarks>
        public const SocketOptionName IasQuery = (SocketOptionName)0x00000012;

        /// <summary>
        /// Retrieve the maximum send size when using IrLMP directly 
        /// (<see cref="F:InTheHand.Net.Sockets.IrDASocketOptionName.IrLptMode"/>).  
        /// IrLMP requires sent data to fit in one frame.
        /// </summary>
        /// <remarks>
        /// <c>Integer</c>
        /// </remarks>
        public const SocketOptionName SendPduLength = (SocketOptionName)0x00000013;

        /// <summary>
        /// Restricts the link to one application-level (IrLMP) connection; 
        /// for use when low latency is required.
        /// Returns an error on all tested platforms.
        /// </summary>
        /// <remarks>
        /// Returns an error on all tested platforms.  <c>Boolean</c>
        /// </remarks>
        public const SocketOptionName ExclusiveMode = (SocketOptionName)0x00000014;

        /// <summary>
        /// Sets IrLMP mode, disabling TinyTP.  Used for instance when 
        /// printing with IrLPT.
        /// </summary>
        /// <remarks>
        /// On Windows NT platforms at least, is ignored on server-side sockets.
        /// <c>Boolean</c>
        /// </remarks>
        public const SocketOptionName IrLptMode = (SocketOptionName)0x00000015;

        /// <summary>
        /// Sets IrCOMM 9-Wire/Cooked mode.  Used for instance when connecting 
        /// to the modem in a mobile phone (service name <c>IrDA:IrCOMM</c>).  
        /// </summary>
        /// <remarks>
        /// In operation, received IrCOMM control information is discarded and 
        /// null information is sent.
        /// <c>Boolean</c>
        /// </remarks>
        public const SocketOptionName NineWireMode = (SocketOptionName)0x00000016;

        /// <summary>
        /// Reportedly sets non-IrDA Sharp ASK mode on the Windows CE 
        /// platform.  Presence unverified.
        /// </summary>
        public const SocketOptionName SharpMode = (SocketOptionName)0x00000020;

        // AJMcF: I do not recognize this value.  There is an IOCTL value
        // --which shouldn't be defined here--but its value is 0x4004747f...
        //LazyDiscovery   = 0x00000030,
	}
}
