// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.Msft.BLUETOOTH_COD_PAIRS
// 
// Copyright (c) 2003-2011 In The Hand Ltd, All rights reserved.
// This source code is licensed under the Microsoft Public License (Ms-PL) - see License.txt

using System;
using System.Runtime.InteropServices;

namespace InTheHand.Net.Bluetooth.Msft
{
    [StructLayout(LayoutKind.Sequential, Size=8)]
    internal class BLUETOOTH_COD_PAIRS
    {
        internal uint ulCODMask;
        [MarshalAs(UnmanagedType.LPWStr)]
        internal string pcszDescription;
    }
}
