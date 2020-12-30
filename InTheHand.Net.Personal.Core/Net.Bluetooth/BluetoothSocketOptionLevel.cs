// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Sockets.BluetoothSocketOptionLevel
// 
// Copyright (c) 2003-2017 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License - see LICENSE

#region Using directives

using System;
using System.Net.Sockets;

#endregion

namespace InTheHand.Net.Sockets
{
    /// <summary>
    /// Defines additional Bluetooth socket option levels for the <see cref="M:System.Net.Sockets.Socket.SetSocketOption(System.Net.Sockets.SocketOptionLevel,System.Net.Sockets.SocketOptionName,System.Int32)"/> and <see cref="System.Net.Sockets.Socket.GetSocketOption(System.Net.Sockets.SocketOptionLevel,System.Net.Sockets.SocketOptionName)"/> methods.
    /// </summary>
    public static class BluetoothSocketOptionLevel
    {
        /// <summary>
        /// Bluetooth RFComm protocol (bt-rfcomm)
        /// </summary>
        public const SocketOptionLevel RFComm = (SocketOptionLevel)0x03;
        /// <summary>
        /// Logical Link Control and Adaptation Protocol (bt-l2cap)
        /// </summary>
        public const SocketOptionLevel L2Cap = (SocketOptionLevel)0x100;
        /// <summary>
        /// Service Discovery Protocol (bt-sdp)
        /// </summary>
        public const SocketOptionLevel Sdp = (SocketOptionLevel)0x0101;
    }
}
