// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Sockets.IrDASocketOptionLevel
// 
// Copyright (c) 2003-2006 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Net.Sockets;

namespace InTheHand.Net.Sockets
{
    /// <summary>
    /// Defines additional IrDA socket option levels for the <see cref="System.Net.Sockets.Socket.SetSocketOption(System.Net.Sockets.SocketOptionLevel,System.Net.Sockets.SocketOptionName,System.Int32)"/> and <see cref="System.Net.Sockets.Socket.GetSocketOption(System.Net.Sockets.SocketOptionLevel,System.Net.Sockets.SocketOptionName)"/> methods.
    /// </summary>
    /// <remarks>
    /// Use along with the socket options defined by 
    /// <see cref="T:InTheHand.Net.Sockets.IrDASocketOptionName"/>.
    /// </remarks>
    /// <seealso cref="T:InTheHand.Net.Sockets.IrDASocketOptionName"/>
    public static class IrDASocketOptionLevel
    {
        /// <summary>
        /// The socket option level for use with IrDA sockets 
        /// along with the options defined in <see 
        /// cref="T:InTheHand.Net.Sockets.IrDASocketOptionName"/>.
        /// </summary>
        /// <remarks>
        /// Use along with the socket options defined by 
        /// <see cref="T:InTheHand.Net.Sockets.IrDASocketOptionName"/>.
        /// </remarks>
        /// <seealso cref="T:InTheHand.Net.Sockets.IrDASocketOptionName"/>
        public const SocketOptionLevel IrLmp = (SocketOptionLevel)0xff;
    }  
}
