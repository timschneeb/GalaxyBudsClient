// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.Msft.BTHNS_INQUIRYBLOB
// 
// Copyright (c) 2003-2011 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Runtime.InteropServices;
using InTheHand.Net.Sockets;

namespace InTheHand.Net.Bluetooth.Msft
{
    [StructLayout(LayoutKind.Sequential, Size=6)]
    internal struct BTHNS_INQUIRYBLOB
    {
        internal int LAP;
        internal byte length;
        internal byte num_responses;
    }
}
