// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.ISdpNodeContainer
// 
// Copyright (c) 2003-2008 In The Hand Ltd, All rights reserved.
// This source code is licensed under the In The Hand Community License - see License.txt

using System;
using System.Runtime.InteropServices;

#if NETCF
namespace InTheHand.Net.Bluetooth
{
#pragma warning disable 1591

    [Guid("43F6ED49-6E22-4F81-A8EB-DCED40811A77"), ComImport(), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [CLSCompliant(false)]
    internal interface ISdpNodeContainer
    {
        void CreateStream(out byte[] ppStream, out int pSize);
        
        void WriteStream(byte[] pStream, out int pNumBytesWritten);
        
        void AppendNode(ref NodeData pData);
        
        void GetType(out NodeContainerType pType);
        
        void SetType(NodeContainerType type);
        
        void Walk(ISdpWalk pWalk);
        
        void SetNode(int nodeIndex, ref NodeData pData);
        
        void GetNode(int nodeIndex, ref NodeData pData);
        
        void LockContainer(byte lockc);
        
        void GetNodeCount(out int pNodeCount);
        
        void CreateFromStream(byte[] pStream, int size);
        
        void GetNodeStringData(int nodeIndex, ref NodeData pData);
        
        void GetStreamSize(out int pSize);
        
    };
}
#endif