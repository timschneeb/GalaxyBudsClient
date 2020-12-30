// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.SdpSpecificType
// 
// Copyright (c) 2003-2008 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Runtime.InteropServices;


namespace InTheHand.Net.Bluetooth
{
#pragma warning disable 1591

#if NETCF && ComSdp
    [StructLayout(LayoutKind.Sequential)]
    [CLSCompliant(false)]
    internal struct SdpAttributeRange
    {
        public ushort minAttribute;
        public ushort maxAttribute;
    }

    [StructLayout(LayoutKind.Explicit)]
    [CLSCompliant(false)]
    internal struct SdpQueryUuid
    {
        [FieldOffset(0)]
        public Guid uuid128;
        [FieldOffset(0)]
        public uint uuid32;
        [FieldOffset(0)]
        public ushort uuid16;
        [FieldOffset(4)]
        public SDP_SPECIFICTYPE uuidType;
    }

    internal enum NodeContainerType
    {
        NodeContainerTypeSequence = 0,
        NodeContainerTypeAlternative = NodeContainerTypeSequence + 1
    }

    internal enum SDP_TYPE : short
    {	
        NIL	    = 0,
        UINT    = 0x1,
        INT	    = 0x2,
        UUID	= 0x3,
        STRING	= 0x4,
        BOOLEAN	= 0x5,
        SEQUENCE	= 0x6,
        ALTERNATIVE	= 0x7,
        URL	        = 0x8,
        CONTAINER	= 0x20
    }


    internal enum SDP_SPECIFICTYPE : short
    {	
        NONE	= 0,
        UINT8	= 0x10,
        UINT16	= 0x110,
        UINT32	= 0x210,
        UINT64	= 0x310,
        UINT128	= 0x410,
        INT8	= 0x20,
        INT16	= 0x120,
        INT32	= 0x220,
        INT64	= 0x320,
        INT128	= 0x420,
        UUID16	= 0x130,
        UUID32	= 0x230,
        UUID128	= 0x430
    }

#endif

    
	// <summary>
	// Identifies specific data types used by SDP.
	// </summary>
	internal enum SdpSpecificType : short
	{
        // allow for a little easier type checking / sizing for integers and UUIDs
		// ((SDP_ST_XXX & 0xF0) >> 4) == SDP_TYPE_XXX
		// size of the data (in bytes) is encoded as ((SDP_ST_XXX & 0xF0) >> 8)

		None = 0x0000,

        //UInt8 = 0x0010,
        //UInt16 = 0x0110,
        //UInt32 = 0x0210,
        //UInt64 = 0x0310,
        //UInt128 = 0x0410,
    
        //Int8 = 0x0020,
        //Int16 = 0x0120,
        //Int32 = 0x0220,
        //Int64 = 0x0320,
        //Int128 = 0x0420,
    
        //Uuid16 = 0x0130,
        //Uuid32 = 0x0230,
		Uuid128 = 0x0430,
	}
}
