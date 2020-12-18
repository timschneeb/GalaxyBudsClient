// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.ISdpSearch
// 
// Copyright (c) 2003-2008 In The Hand Ltd, All rights reserved.
// This source code is licensed under the In The Hand Community License - see License.txt

using System;
using System.Runtime.InteropServices;

#if NETCF
namespace InTheHand.Net.Bluetooth
{
#pragma warning disable 1591

    [Guid("D93B6B2A-5EEF-4E1E-BECF-F5A4340C65F5"), ComImport(), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [CLSCompliant(false)]
    internal interface ISdpSearch
    {
        void Begin(ref ulong pAddrss, uint fConnect);
        
        void End();
        
        void ServiceSearch(ref SdpQueryUuid pUuidList, uint listSize,
            out uint pHandles, out ushort pNumHandles);
        
        void AttributeSearch(uint handle, ref SdpAttributeRange pRangeList,
            uint numRanges, out ISdpRecord ppSdpRecord);
        
        void ServiceAndAttributeSearch(ref SdpQueryUuid pUuidList, uint listSize,
            ref SdpAttributeRange pRangeList,
            uint numRanges,
            out ISdpRecord pppSdpRecord,
            out uint pNumRecords);
        
    }

    
}
#endif