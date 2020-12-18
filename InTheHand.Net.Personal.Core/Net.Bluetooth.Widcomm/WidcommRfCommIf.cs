// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Widcomm.WidcommSocketExceptions
// 
// Copyright (c) 2008-2009 In The Hand Ltd, All rights reserved.
// Copyright (c) 2008-2009 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace InTheHand.Net.Bluetooth.Widcomm
{
    internal sealed class WidcommRfCommIf : IRfCommIf
    {
        public IntPtr PObject { get { throw new NotSupportedException(); } }

        private static class NativeMethods
        {
            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern IntPtr RfCommIf_Create(out IntPtr ppRfCommIf);

            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern void RfCommIf_Destroy(IntPtr pRfCommPort);

            [DllImport(WidcommNativeBits.WidcommDll)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool RfCommIf_Client_AssignScnValue(IntPtr pRfCommPort,
                ref Guid serviceGuid, byte scn);

            [DllImport(WidcommNativeBits.WidcommDll)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool RfCommIf_SetSecurityLevel(IntPtr pRfCommPort,
                byte[] serviceName, BTM_SEC securityLevel,
                [MarshalAs(UnmanagedType.Bool)] bool isServer);

            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern int RfCommIf_GetScn(IntPtr pRfCommPort);
        }

        IntPtr m_pRfCommIf;
        public void Create()
        {
            NativeMethods.RfCommIf_Create(out m_pRfCommIf);
            if (m_pRfCommIf == IntPtr.Zero)
                throw new InvalidOperationException("Native object creation failed.");
        }

        public void Destroy(bool disposing)
        {
            Debug.Assert(m_pRfCommIf != IntPtr.Zero, "WidcommRfcommIf Already Destroyed");
            if (m_pRfCommIf != IntPtr.Zero) {
                NativeMethods.RfCommIf_Destroy(m_pRfCommIf);
                m_pRfCommIf = IntPtr.Zero;
            }
        }

        public bool ClientAssignScnValue(Guid serviceGuid, int scn)
        {
            var scnB = checked((byte)scn);
            return NativeMethods.RfCommIf_Client_AssignScnValue(m_pRfCommIf, ref serviceGuid, scnB);
        }

        public bool SetSecurityLevel(byte[] serviceName, BTM_SEC securityLevel, bool isServer)
        {
            return NativeMethods.RfCommIf_SetSecurityLevel(m_pRfCommIf, serviceName, securityLevel, isServer);
        }

        public int GetScn()
        {
            return NativeMethods.RfCommIf_GetScn(m_pRfCommIf);
        }
    }
}
