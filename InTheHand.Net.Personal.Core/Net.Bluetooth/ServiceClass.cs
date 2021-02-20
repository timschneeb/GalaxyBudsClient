// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.ServiceClass
// 
// Copyright (c) 2003-2006 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;

namespace InTheHand.Net.Bluetooth
{
    /// <summary>
    /// Class of Service flags as assigned in the Bluetooth specifications.
    /// </summary>
    /// -
    /// <remarks>
    /// <para>Is returned by the property <see
    /// cref="P:InTheHand.Net.Bluetooth.ClassOfDevice.Service">ClassOfDevice.Service</see>.
    /// </para>
    /// <para>Defined in Bluetooth Specifications <see href="http://www.bluetooth.org/Technical/AssignedNumbers/baseband.htm"/>.
    /// </para>
    /// </remarks>
    [Flags]
    public enum ServiceClass
    {
        /// <summary>
        /// No service class bits set.
        /// </summary>
        None = 0,

        /// <summary>
        /// 
        /// </summary>
        Information = 0x0400,//0x800000,
        /// <summary>
        /// 
        /// </summary>
        Telephony = 0x0200,//0x400000,
        /// <summary>
        /// 
        /// </summary>
        Audio = 0x0100,//0x200000,
        /// <summary>
        /// 
        /// </summary>
        ObjectTransfer = 0x0080,//0x100000,
        /// <summary>
        /// 
        /// </summary>
        Capturing = 0x0040,//0x080000,
        /// <summary>
        /// 
        /// </summary>
        Rendering = 0x0020,//0x040000,
        /// <summary>
        /// 
        /// </summary>
        Network = 0x0010,//0x020000,
        /// <summary>
        /// 
        /// </summary>
        Positioning = 0x0008,//0x010000,
        /// <summary>
        /// 
        /// </summary>
        LimitedDiscoverableMode = 0x0001, //0x002000,
    }
}
