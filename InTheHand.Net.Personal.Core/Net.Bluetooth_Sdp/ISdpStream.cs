// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.ISdpStream
// 
// Copyright (c) 2003-2008 In The Hand Ltd, All rights reserved.
// This source code is licensed under the In The Hand Community License - see License.txt

using System;
using System.Runtime.InteropServices;

#if NETCF
namespace InTheHand.Net.Bluetooth
{
#pragma warning disable 1591

    [Guid("A6ECD9FB-0C7A-41A3-9FF0-0B617E989357"), ComImport(),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown), CLSCompliant(false)]
    internal interface ISdpStream
    {
        void Validate(
            [MarshalAs(UnmanagedType.LPArray)] byte[] pStream,
            int size,
            out uint pErrorByte);
        
        void Walk(
            [MarshalAs(UnmanagedType.LPArray)]  byte[] pStream,
            int size,
            ISdpWalk pWalk);
        
        void RetrieveRecords( 
            [MarshalAs(UnmanagedType.LPArray)] byte[] pStream,
            int size,
            /* [out][in] */ [MarshalAs(UnmanagedType.LPArray)] IntPtr[] ppSdpRecords,
            /* [out][in] */ ref int pNumRecords);
        
        void RetrieveUuid128(
            [MarshalAs(UnmanagedType.LPArray)]  byte[] pStream,
            out Guid pUuid128);
        
        void RetrieveUint16(
            [MarshalAs(UnmanagedType.LPArray)]  byte[] pStream,
            out ushort pUint16);
        
        void RetrieveUint32(
            [MarshalAs(UnmanagedType.LPArray)]  byte[] pStream,
            out uint pUint32);
        
        void RetrieveUint64(
           [MarshalAs(UnmanagedType.LPArray)]  byte[] pStream,
            out ulong pUint64);
        
        void RetrieveUint128(
            [MarshalAs(UnmanagedType.LPArray)]  byte[] pStream,
            out Guid pUint128);
        
        void RetrieveInt16(
            [MarshalAs(UnmanagedType.LPArray)]  byte[] pStream,
             out short pInt16);
        
        void RetrieveInt32(
            [MarshalAs(UnmanagedType.LPArray)]  byte[] pStream,
            out int pInt32);
        
        void RetrieveInt64(
            [MarshalAs(UnmanagedType.LPArray)]  byte[] pStream,
            out long pInt64);
        
        void RetrieveInt128(
            [MarshalAs(UnmanagedType.LPArray)]  byte[] pStream,
            out Guid pInt128);
        
        void ByteSwapUuid128( 
            ref Guid pInUuid128,
            out Guid pOutUuid128);
        
        void ByteSwapUint128( 
            ref Guid pInUint128,
            out Guid pOutUint128);
        
        void ByteSwapUint64( 
            ref ulong inUint64,
            out ulong pOutUint64);
        
        void ByteSwapUint32( 
            uint uint32,
            out uint pUint32);
        
        void ByteSwapUint16( 
            ushort uint16,
            out ushort pUint16);
        
        void ByteSwapInt128( 
            ref Guid pInInt128,
            out Guid pOutInt128);
        
        void ByteSwapInt64( 
            ref long inInt64,
            out long pOutInt64);
        
        void ByteSwapInt32( 
            int int32,
            out int pInt32);
        
        void ByteSwapInt16( 
            short int16,
            out short pInt16);
        
        void NormalizeUuid( 
            ref NodeData pDataUuid,
            out Guid pNormalizeUuid);
        
        void RetrieveElementInfo(
            [MarshalAs(UnmanagedType.LPArray)] byte[] pStream,
            out SDP_TYPE pElementType,
            out SDP_SPECIFICTYPE pElementSpecificType,
            out uint pElementSize,
            out uint pStorageSize,
            out byte[] ppData);
        
        void VerifySequenceOf( 
            [MarshalAs(UnmanagedType.LPArray)] byte[] pStream,
            int size,
            SDP_TYPE ofType,
            byte[] pSpecificSizes,
            out int pNumFound); 
    }

    [ComImport(), Guid("249797FA-19DB-4dda-94D4-E0BCD30EA65E"), CLSCompliant(false)]
    public class SdpStream : ISdpStream
    {
        #region ISdpStream Members

        public extern void Validate(byte[] pStream, int size, out uint pErrorByte);

        public extern void Walk(byte[] pStream, int size, ISdpWalk pWalk);

        public extern void RetrieveRecords(byte[] pStream, int size, IntPtr[] ppSdpRecords, ref int pNumRecords);

        public extern void RetrieveUuid128(byte[] pStream, out Guid pUuid128);

        public extern void RetrieveUint16(byte[] pStream, out ushort pUint16);

        public extern void RetrieveUint32(byte[] pStream, out uint pUint32);

        public extern void RetrieveUint64(byte[] pStream, out ulong pUint64);

        public extern void RetrieveUint128(byte[] pStream, out Guid pUint128);

        public extern void RetrieveInt16(byte[] pStream, out short pInt16);

        public extern void RetrieveInt32(byte[] pStream, out int pInt32);

        public extern void RetrieveInt64(byte[] pStream, out long pInt64);

        public extern void RetrieveInt128(byte[] pStream, out Guid pInt128);

        public extern void ByteSwapUuid128(ref Guid pInUuid128, out Guid pOutUuid128);

        public extern void ByteSwapUint128(ref Guid pInUint128, out Guid pOutUint128);

        public extern void ByteSwapUint64(ref ulong inUint64, out ulong pOutUint64);

        public extern void ByteSwapUint32(uint uint32, out uint pUint32);

        public extern void ByteSwapUint16(ushort uint16, out ushort pUint16);

        public extern void ByteSwapInt128(ref Guid pInInt128, out Guid pOutInt128);

        public extern void ByteSwapInt64(ref long inInt64, out long pOutInt64);

        public extern void ByteSwapInt32(int int32, out int pInt32);

        public extern void ByteSwapInt16(short int16, out short pInt16);

        public extern void NormalizeUuid(ref NodeData pDataUuid, out Guid pNormalizeUuid);

        public extern void RetrieveElementInfo(byte[] pStream, out SDP_TYPE pElementType, out SDP_SPECIFICTYPE pElementSpecificType, out uint pElementSize, out uint pStorageSize, out byte[] ppData);

        public extern void VerifySequenceOf(byte[] pStream, int size, SDP_TYPE ofType, byte[] pSpecificSizes, out int pNumFound);

        #endregion
    }
}
#endif
