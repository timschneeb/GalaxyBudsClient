// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Widcomm.WidcommNativeBits
// 
// Copyright (c) 2008-2012 In The Hand Ltd, All rights reserved.
// Copyright (c) 2008-2012 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

using System;

namespace InTheHand.Net.Bluetooth.Widcomm
{
    internal static class WidcommNativeBits
    {
#if NETCF
        internal const string WidcommDll = "32feetWidcomm"; //"wm2";
#else
        internal const string WidcommDll = "32feetWidcomm"; //"32feetWidcommWin32";
#endif

        internal delegate void RfcommPort_DataReceivedCallbackDelegate(IntPtr data, UInt16 len);
        internal delegate void RfcommPort_EventReceivedCallbackDelegate(UInt32 data);

    }
}