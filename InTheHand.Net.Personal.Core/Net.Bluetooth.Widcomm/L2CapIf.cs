// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Widcomm.L2CapIf.cs
// 
// Copyright (c) 2008-2010 In The Hand Ltd, All rights reserved.
// Copyright (c) 2008-2010 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Globalization;

namespace InTheHand.Net.Bluetooth.Widcomm
{
    internal sealed class L2CapIf : IRfCommIf
    {
        IntPtr _pIf;

        //----
        internal L2CapIf()
        {
        }

        public IntPtr PObject { get { return _pIf; } }

        //----
        public void Create()
        {
            WidcommL2CapClient.NativeMethods.L2CapIf_Create(out _pIf);
            if (_pIf == IntPtr.Zero)
                throw new InvalidOperationException("Native object creation failed.");
        }

        public void Destroy(bool disposing)
        {
            Utils.MiscUtils.Trace_WriteLine("L2CapIf.Destroy()");
            Debug.Assert(_pIf != IntPtr.Zero, "WidcommRfcommIf Already Destroyed");
            if (_pIf != IntPtr.Zero) {
                WidcommL2CapClient.NativeMethods.L2CapIf_Deregister(_pIf);
                WidcommL2CapClient.NativeMethods.L2CapIf_Destroy(_pIf);
                _pIf = IntPtr.Zero;
            }
        }

        public bool ClientAssignScnValue(Guid serviceGuid, int scn)
        {
            ushort psm = checked((ushort)scn);
            bool success;
            success = WidcommL2CapClient.NativeMethods.L2CapIf_AssignPsmValue(_pIf, ref serviceGuid, psm);
            Utils.MiscUtils.Trace_WriteLine("L2CapIf_AssignPsmValue ret: {0} <- psm: {1}=0x{1:X}",
                success, psm);
            if (!success)
                return false;
            //
            var psmOut = WidcommL2CapClient.NativeMethods.L2CapIf_GetPsm(_pIf);
            Utils.MiscUtils.Trace_WriteLine("L2CapIf_GetPsm PSM: {0}=0x{0:X}", psmOut);
            //
            success = WidcommL2CapClient.NativeMethods.L2CapIf_Register(_pIf);
            Utils.MiscUtils.Trace_WriteLine("L2CapIf_Register ret :{0}", success);
            return success;
        }

        public bool SetSecurityLevel(byte[] serviceName, BTM_SEC securityLevel, bool isServer)
        {
            string serviceNameX = "foo"; // HACK serviceNameX
            return WidcommL2CapClient.NativeMethods.L2CapIf_SetSecurityLevel(_pIf, serviceNameX, securityLevel, isServer);
        }

        public int GetScn()
        {
            return WidcommL2CapClient.NativeMethods.L2CapIf_GetPsm(_pIf);
        }
    }
}
