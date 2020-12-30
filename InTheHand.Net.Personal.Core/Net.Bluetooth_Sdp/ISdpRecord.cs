// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.ISdpRecord
// 
// Copyright (c) 2003-2008 In The Hand Ltd, All rights reserved.
// This source code is licensed under the In The Hand Community License - see License.txt

using System;
using System.Runtime.InteropServices;

#if NETCF
namespace InTheHand.Net.Bluetooth
{
#pragma warning disable 1591

    [Guid("10276714-1456-46D7-B526-8B1E83D5116E"), ComImport(), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [CLSCompliant(false)]
    internal interface ISdpRecord
    {
        void  CreateFromStream(byte[] pStream, int size);
        
        void WriteToStream(out IntPtr ppStream, out uint pStreamSize, uint preSize, uint postSize);
        
        void SetAttribute(ushort attribute, ref NodeData pNode);
        
        void SetAttributeFromStream(ushort attribute, byte[] pStream, uint size);
        
        void GetAttribute(ushort attribute, ref NodeData pNode);
        
        void GetAttributeAsStream(ushort attribute, out byte[] ppStream, out uint pSize);
        
        void Walk(ISdpWalk pWalk);
        
        void GetAttributeList(out IntPtr ppList, out int pListSize);
        
        void GetString(ushort offset, ref ushort pLangId, out string ppString);
        
        void GetIcon(int cxRes, int cyRes, out IntPtr phIcon);
        
        void GetServiceClass(out Guid pServiceClass);      
    }

    [ComImport(), Guid("ACD02BA7-9667-4085-A100-CC6ACA9621D6")]
    [CLSCompliant(false)]
    public class SdpRecord : ISdpRecord
    {
#region ISdpRecord Members

        public extern void  CreateFromStream(byte[] pStream, int size);

        public extern void  WriteToStream(out IntPtr ppStream, out uint pStreamSize, uint preSize, uint postSize);

        public extern void  SetAttribute(ushort attribute, ref NodeData pNode);

        public extern void  SetAttributeFromStream(ushort attribute, byte[] pStream, uint size);

        public extern void GetAttribute(ushort attribute, ref NodeData pNode);

        public extern void  GetAttributeAsStream(ushort attribute, out byte[] ppStream, out uint pSize);

        public extern void  Walk(ISdpWalk pWalk);

        public extern void  GetAttributeList(out IntPtr ppList, out int pListSize);

        public extern void  GetString(ushort offset, ref ushort pLangId, out string ppString);

        public extern void  GetIcon(int cxRes, int cyRes, out IntPtr phIcon);

        public extern void  GetServiceClass(out Guid pServiceClass);

#endregion
    }
}
#endif