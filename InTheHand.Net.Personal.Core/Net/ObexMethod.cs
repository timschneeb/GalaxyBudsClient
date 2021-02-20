// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.ObexMethod
// 
// Copyright (c) 2003-2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System.Diagnostics.CodeAnalysis;

namespace InTheHand.Net
{
    /// <summary>
    /// Methods which can be carried out in an Object Exchange transaction.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue"),
     SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32")]
    public enum ObexMethod : byte
    {
        /// <summary>
        /// Sends an object to a receiving device.
        /// </summary>
        Put = 0x02,
        /// <summary>
        /// Requests a file from the remote device.
        /// </summary>
        Get = 0x03,
        /// <summary>
        /// Negotiate an Object Exchange connection with a remote device.
        /// </summary>
        Connect = 0x80,
        /// <summary>
        /// Disconnect an existing Object Exchange session.
        /// </summary>
        Disconnect = 0x81,
        /// <summary>
        /// Sends the last packet of an object to a receiving device.
        /// </summary>
        PutFinal = 0x82,
        /// <summary>
        /// Change remote path on an Object Exchange server.
        /// </summary>
        SetPath = 0x85,
    }
}
