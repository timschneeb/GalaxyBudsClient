// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Sockets.BluetoothProtocolType
// 
// Copyright (c) 2003-2008 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

#region Using directives

using System;
using System.Net.Sockets;

#endregion

namespace InTheHand.Net.Sockets
{
    /// <summary>
    /// Specifies additional protocols that the <see cref="Socket"/> class supports.
    /// </summary>
    /// <remarks>
    /// <para>These constants are defined by the Bluetooth SIG - <see href="https://www.bluetooth.org/Technical/AssignedNumbers/service_discovery.htm"/>
    /// </para>
    /// </remarks>
    public static class BluetoothProtocolType
    {
        /// <summary>
        /// Service Discovery Protocol (bt-sdp)
        /// </summary>
        public const ProtocolType Sdp = (ProtocolType)0x0001;
        /*
        /// <summary>
        /// 
        /// </summary>
        public const ProtocolType Udp = (ProtocolType)0x0002;*/

        /// <summary>
        /// Bluetooth RFComm protocol (bt-rfcomm)
        /// </summary>
        public const ProtocolType RFComm = (ProtocolType)0x0003;

        /*
        /// <summary>
        /// 
        /// </summary>
        public const ProtocolType Tcp = (ProtocolType)0x0004;
        /// <summary>
        /// Telephony Control Specification Binary (bt-tcs)
        /// </summary>
        public const ProtocolType TcsBin = (ProtocolType)0x0005;
        /// <summary>
        /// (modem)
        /// </summary>
        public const ProtocolType TcsAt = (ProtocolType)0x0006;
        /// <summary>
        /// Object Exchange (obex)
        /// </summary>
        public const ProtocolType Obex = (ProtocolType)0x0008;
        /// <summary>
        /// 
        /// </summary>
        public const ProtocolType IP = (ProtocolType)0x0009;
        /// <summary>
        /// File Transfer Protocol (ftp)
        /// </summary>
        public const ProtocolType Ftp = (ProtocolType)0x000A;
        /// <summary>
        /// Hypertext Transfer Protocol (http)
        /// </summary>
        public const ProtocolType Http = (ProtocolType)0x000C;
        /// <summary>
        /// (wsp)
        /// </summary>
        public const ProtocolType Wsp = (ProtocolType)0x000E;
        /// <summary>
        /// BNEP
        /// </summary>
        public const ProtocolType BNEP = (ProtocolType)0x000F;
        /// <summary>
        /// ESDP
        /// </summary>
        public const ProtocolType Upnp = (ProtocolType)0x0010; 
        /// <summary>
        /// Human Interface Device Protocol
        /// </summary>
        public const ProtocolType Hidp = (ProtocolType)0x0011;
        /// <summary>
        /// See Hardcopy Cable Replacement Profile (HCRP), Bluetooth SIG
        /// </summary>
        public const ProtocolType HardcopyControlChannel = (ProtocolType)0x0012;
        /// <summary>
        /// See Hardcopy Cable Replacement Profile (HCRP), Bluetooth SIG
        /// </summary>
        public const ProtocolType HardcopyDataChannel = (ProtocolType)0x0014;
        /// <summary>
        /// See Hardcopy Cable Replacement Profile (HCRP), Bluetooth SIG
        /// </summary>
        public const ProtocolType HardcopyNotification = (ProtocolType)0x0016;
        /// <summary>
        /// Audio/Video Control Transport Protocol, Bluetooth SIG
        /// </summary>
        public const ProtocolType AVCTP = (ProtocolType)0x0017;
        /// <summary>
        /// Audio/Video Distribution Transport Protocol, Bluetooth SIG
        /// </summary>
        public const ProtocolType AVDTP = (ProtocolType)0x0019;
        /// <summary>
        /// CAPI Message Transport Protocol (bt-cmtp)
        /// </summary>
        public const ProtocolType CMTP = (ProtocolType)0x001B;
        /// <summary>
        /// See the Unrestricted Digital Information Profile [UDI], Bluetooth SIG
        /// </summary>
        public const ProtocolType UdiCPlane = (ProtocolType)0x001D;
        /// <summary>
        /// mcap-control See Multi-Channel Adaptation Protocol (MCAP), Bluetooth SIG
        /// </summary>
        public const ProtocolType MCAPControlChannel = (ProtocolType)0x001E;  
        /// <summary>
        ///  mcap-data See Multi-Channel Adaptation Protocol (MCAP), Bluetooth SIG
        /// </summary>
        public const ProtocolType MCAPDataChannel = (ProtocolType)0x001F;
*/
        /// <summary>
        /// Logical Link Control and Adaptation Protocol (bt-l2cap)
        /// </summary>
        public const ProtocolType L2Cap = (ProtocolType)0x0100;
    }
}
