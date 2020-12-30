// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Ports.PORTEMUPortParams
// 
// Copyright (c) 2003-2008 In The Hand Ltd, All rights reserved.
// This source code is licensed under the In The Hand Community License - see License.txt

using System;
using InTheHand.Net;
using System.Runtime.InteropServices;

#if NETCF

namespace InTheHand.Net.Ports
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct PORTEMUPortParams
    {
        internal int channel;
        [MarshalAs(UnmanagedType.Bool)]
        internal bool flocal;
        internal long device;//10
        internal int imtu;
        internal int iminmtu;
        internal int imaxmtu;
        internal int isendquota;
        internal int irecvquota;
        internal Guid uuidService;//16
        internal RFCOMM_PORT_FLAGS uiportflags;
    }

	[Flags()]
    internal enum RFCOMM_PORT_FLAGS : int
	{
        REMOTE_DCB = 0x00000001,
        KEEP_DCD = 0x00000002,
        AUTHENTICATE = 0x00000004,
        ENCRYPT = 0x00000008, 
	}
}

#endif