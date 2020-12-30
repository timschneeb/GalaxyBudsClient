// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.Msft.WSAQUERYSET
// 
// Copyright (c) 2003-2007 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Runtime.InteropServices;

namespace InTheHand.Net.Bluetooth.Msft
{
    [StructLayout(LayoutKind.Sequential, Size=60)]
    internal struct WSAQUERYSET
    {
        public int dwSize;
#if NETCF
        IntPtr lpszServiceInstanceName;
#else
        [MarshalAs(UnmanagedType.LPStr)]
        public string lpszServiceInstanceName;
#endif
        public IntPtr lpServiceClassId;
        IntPtr lpVersion;
        IntPtr lpszComment;
        public int dwNameSpace;
        IntPtr lpNSProviderId;
#if NETCF
        IntPtr lpszContext;
#else
        [MarshalAs(UnmanagedType.LPStr)]
        public string lpszContext;
#endif
        int dwNumberOfProtocols;
        IntPtr lpafpProtocols;
        IntPtr lpszQueryString;
        public int dwNumberOfCsAddrs;
        public IntPtr lpcsaBuffer;
        int dwOutputFlags;
        public IntPtr lpBlob;
    }


    // WSAQUERYSET
#if !V1
    static
#endif
    class WqsOffset
    {
        public static readonly int dwSize_0 = 0;
        public static readonly int dwNameSpace_20 = 5 * IntPtr.Size;
        public static readonly int lpcsaBuffer_48 = 12 * IntPtr.Size;
        public static readonly int dwOutputFlags_52 = 13 * IntPtr.Size;
        public static readonly int lpBlob_56 = 14 * IntPtr.Size;
        //
        public static readonly int StructLength_60 = 15 * IntPtr.Size;
        //
        public const int NsBth_16 = 16;

        static bool s_doneAssert;

        [System.Diagnostics.Conditional("DEBUG")]
        public static void AssertCheckLayout()
        {
            if (s_doneAssert)
                return;
            s_doneAssert = true;
#if !NETCF // No OffsetOf on NETCF
            System.Diagnostics.Debug.Assert(WqsOffset.dwNameSpace_20
                == Marshal.OffsetOf(typeof(WSAQUERYSET), "dwNameSpace").ToInt64(), "offset dwNameSpace");
            System.Diagnostics.Debug.Assert(WqsOffset.lpcsaBuffer_48
                == Marshal.OffsetOf(typeof(WSAQUERYSET), "lpcsaBuffer").ToInt64(), "offset lpcsaBuffer");
            System.Diagnostics.Debug.Assert(WqsOffset.dwOutputFlags_52
                == Marshal.OffsetOf(typeof(WSAQUERYSET), "dwOutputFlags").ToInt64(), "offset dwOutputFlags");
            System.Diagnostics.Debug.Assert(WqsOffset.lpBlob_56
                == Marshal.OffsetOf(typeof(WSAQUERYSET), "lpBlob").ToInt64(), "offset lpBlob");
            //
            System.Diagnostics.Debug.Assert(WqsOffset.StructLength_60
                == Marshal.SizeOf(typeof(WSAQUERYSET)), "StructLength");
#endif
        }

    }//class

}
