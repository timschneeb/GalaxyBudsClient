// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.Msft.CSADDR_INFO
// 
// Copyright (c) 2003-2007,2011 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Runtime.InteropServices;

namespace InTheHand.Net.Bluetooth.Msft
{
    [StructLayout(LayoutKind.Sequential, Size=24)]
    internal struct CSADDR_INFO
    {
        internal IntPtr localAddr;
        internal int localSize;
        internal IntPtr remoteAddr;
        internal int remoteSize;
        internal System.Net.Sockets.SocketType iSocketType;
        internal System.Net.Sockets.ProtocolType iProtocol;

        public CSADDR_INFO(BluetoothAddress local, BluetoothAddress remote, System.Net.Sockets.SocketType type, System.Net.Sockets.ProtocolType protocol)
        {
            //ensure zeros
            localAddr = IntPtr.Zero;
            localSize = 0;
            remoteAddr = IntPtr.Zero;
            remoteSize = 0;

            iSocketType = type;
            iProtocol = protocol;
            

            if (local != null)
            {
#if V1
                //have to use AllocHGlobal substitute
                localAddr = Marshal32.AllocHGlobal(40);
                Marshal.Copy(local.ToByteArray(), 0, new IntPtr(localAddr.ToInt32() + 8), 6);
#else
                localAddr = Marshal.AllocHGlobal(40);
                Marshal.WriteInt64(localAddr, 8, local.ToInt64());
#endif
                Marshal.WriteInt16(localAddr, 0, 32);                
                localSize = 40;
            }
            if (remote != null)
            {
#if V1
                remoteAddr = Marshal32.AllocHGlobal(40);
                Marshal.Copy(remote.ToByteArray(), 0, new IntPtr(remoteAddr.ToInt32() + 8), 6);
#else
                remoteAddr = Marshal.AllocHGlobal(40);
                Marshal.WriteInt64(remoteAddr, 8, remote.ToInt64());
#endif
                remoteSize = 40;
                Marshal.WriteInt16(remoteAddr, 0, 32);   
            }
        }

        public void Dispose()
        {
            if (localAddr!=IntPtr.Zero)
            {
#if V1
                Marshal32.FreeHGlobal(localAddr);
#else
                Marshal.FreeHGlobal(localAddr);
#endif
                localAddr = IntPtr.Zero;
            }
            if (remoteAddr!=IntPtr.Zero)
            {
#if V1
                Marshal32.FreeHGlobal(remoteAddr);
#else
                Marshal.FreeHGlobal(remoteAddr);
#endif
                remoteAddr = IntPtr.Zero;
            }
        }
    }


#if ! V1
    static
#endif
    class CsaddrInfoOffsets
    {
        //struct CSADDR_INFO {
        //    SOCKET_ADDRESS LocalAddr;
        //    SOCKET_ADDRESS RemoteAddr;
        //    INT iSocketType;
        //    INT iProtocol;
        //}
        //struct SOCKET_ADDRESS {
        //    LPSOCKADDR lpSockaddr;
        //    INT iSockaddrLength;
        //}

        // Presumably the pointer field is 8-aligned.
        public static readonly int OffsetRemoteAddr_lpSockaddr_8 = 2 * IntPtr.Size;
        public static readonly int OffsetRemoteAddr_iSockaddrLength_12 = 3 * IntPtr.Size;

        static bool s_doneAssert;

        [System.Diagnostics.Conditional("DEBUG")]
        public static void AssertCheckLayout()
        {
            if (s_doneAssert)
                return;
            s_doneAssert = true;
#if !NETCF // No OffsetOf on NETCF
            System.Diagnostics.Debug.Assert(CsaddrInfoOffsets.OffsetRemoteAddr_lpSockaddr_8
                == Marshal.OffsetOf(typeof(CSADDR_INFO), "remoteAddr").ToInt64(), "offset remoteAddr");
            System.Diagnostics.Debug.Assert(CsaddrInfoOffsets.OffsetRemoteAddr_iSockaddrLength_12
                == Marshal.OffsetOf(typeof(CSADDR_INFO), "remoteSize").ToInt64(), "offset remoteSize");
#endif
        }

    }//class

}