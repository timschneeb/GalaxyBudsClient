// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.LmpExtendedFeatures 
// 
// Copyright (c) 2012 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;

namespace InTheHand.Net.Bluetooth
{
    /// <summary>
    /// Entered from v4.0 spec.
    /// Volume 2, Part C 'LMP', Section 3.4.
    /// </summary>
    /// --
    /// <remarks>
    /// <para>The format of the summary for each entry is:
    /// "NUM Text BYTE BIT".
    /// Where NUM is the bit number, so starting from 64, as these value
    /// follow those in <see cref="E:InTheHand.Net.Bluetooth.LmpFeatures"/>,
    /// and where BYTE and BIT is the position of the bit in this 64-bit value.
    /// </para>
    /// </remarks>
    [Flags]
    public enum LmpExtendedFeatures : long
    {
        /// <summary>
        /// 64 Secure Simple Pairing (Host Support) 0 0
        /// </summary>
        SecureSimplePairing_HostSupport = 0x0001,
        /// <summary>
        /// 65 LE Supported (Host) 0 1
        /// </summary>
        LeSupported_Host = 0x0001,
        /// <summary>
        /// 66 Simultaneous LE and BR/EDR to Same Device Capable (Host) 0 2
        /// </summary>
        SimultaneousLeAndBeEdrToSameDeviceCapable_Host = 0x0002,
    }
}
