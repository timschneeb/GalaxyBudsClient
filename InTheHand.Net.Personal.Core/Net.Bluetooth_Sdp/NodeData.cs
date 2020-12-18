// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.NodeData
// 
// Copyright (c) 2003-2008 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Runtime.InteropServices;

#if NETCF
namespace InTheHand.Net.Bluetooth
{
#pragma warning disable 1591

    /*[StructLayout(LayoutKind.Sequential)]
    public struct SdpString
    {
        public IntPtr val;
        public int length;
    }*/

    [StructLayout(LayoutKind.Explicit, Size=24)]
    [CLSCompliant(false)]
    internal struct NodeData
    {
        [FieldOffset(0)]
        public SDP_TYPE type;
        [FieldOffset(2)]
        public SDP_SPECIFICTYPE specificType;

        //[FieldOffset(4)]
        //public Guid int128;
        //[FieldOffset(4)]
        //public Guid uint128;
        [FieldOffset(8)]
        public Guid uuid128;
        //[FieldOffset(8)]
        //public uint uuid32;
        //[FieldOffset(8)]
        //public ushort uuid16;
        [FieldOffset(8)]
        public long int64;
        [FieldOffset(8)]
        public ulong uint64;
        [FieldOffset(8)]
        public int int32;
        [FieldOffset(8)]
        public uint uint32;
        [FieldOffset(8)]
        public short int16;
        [FieldOffset(8)]
        public ushort uint16;
        [FieldOffset(8)]
        public sbyte int8;
        [FieldOffset(8)]
        public byte uint8;

        //[FieldOffset(4)]
        //public byte booleanVal;

        //flatten the SdpString structure
        [FieldOffset(8)]
        public IntPtr str;
        [FieldOffset(12)]
        public int stringLength;

        [FieldOffset(8)]
        public IntPtr container;
    }

}
#endif