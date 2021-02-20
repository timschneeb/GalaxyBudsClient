// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.ISdpWalk
// 
// Copyright (c) 2003-2008 In The Hand Ltd, All rights reserved.
// This source code is licensed under the In The Hand Community License - see License.txt

using System;
using System.Runtime.InteropServices;

#if NETCF
namespace InTheHand.Net.Bluetooth
{
#pragma warning disable 1591

    [Guid("57134AE6-5D3C-462D-BF2F-810361FBD7E7"), ComImport(), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [CLSCompliant(false)]
    internal interface ISdpWalk
    {
        void WalkNode(IntPtr pData, uint state);

        void WalkStream(byte elementType, uint elementSize, IntPtr pStream);
        
    };

    [CLSCompliant(false)]
    public class SdpWalker : ISdpWalk
    {

#region ISdpWalk Members

        public void WalkNode(IntPtr pData, uint state)
        {
            SDP_TYPE t = (SDP_TYPE)Marshal.ReadInt16(pData, 0);
            SDP_SPECIFICTYPE st = (SDP_SPECIFICTYPE)Marshal.ReadInt16(pData, 2);

            System.Diagnostics.Debug.WriteLine(t.ToString() + " " + st.ToString());
        }

        public void WalkStream(byte elementType, uint elementSize, IntPtr pStream)
        {
            System.Windows.Forms.MessageBox.Show(elementType.ToString() + " " + elementSize.ToString());
        }

#endregion
    }
}
#endif