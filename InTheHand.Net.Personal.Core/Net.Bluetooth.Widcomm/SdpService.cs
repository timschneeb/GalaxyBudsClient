// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Widcomm.WidcommSocketExceptions
// 
// Copyright (c) 2008-2010 In The Hand Ltd, All rights reserved.
// Copyright (c) 2008-2010 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using InTheHand.Net.Bluetooth.AttributeIds;
using System.Net;

namespace InTheHand.Net.Bluetooth.Widcomm
{
    sealed class SdpService : ISdpService
    {
        private static class NativeMethods
        {
            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern void SdpService_Create(out IntPtr ppObj);

            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern void SdpService_Destroy(IntPtr pObj);

            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern SDP_RETURN_CODE SdpService_AddServiceClassIdList(
                IntPtr pObj, int recordCount, IntPtr guidArray);

            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern SDP_RETURN_CODE SdpService_AddRFCommProtocolDescriptor(
                IntPtr pObj, byte scn);

            [DllImport(WidcommNativeBits.WidcommDll, CharSet = CharSet.Unicode)]
            internal static extern SDP_RETURN_CODE SdpService_AddServiceName(
                IntPtr pObj,
                string p_service_nameWchar, IntPtr p_service_nameChar);

            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern SDP_RETURN_CODE SdpService_AddAttribute(
                IntPtr pObj, UInt16 attrId, DESC_TYPE attrType, UInt32 attrLen, 
                byte[] val);

            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern SDP_RETURN_CODE SdpService_CommitRecord(IntPtr pObj);
        }

        IntPtr m_pSdpService;
        //WidcommBluetoothFactoryBase _factory;

        internal SdpService()//WidcommBluetoothFactoryBase factory)
        {
            //_factory = factory;
            try {
                NativeMethods.SdpService_Create(out m_pSdpService);
                if (m_pSdpService == IntPtr.Zero) {
                    throw new InvalidOperationException("Native object creation failed.");
                }
                //_factory.GetWidcommBtInterface().AddNeedsDisposing(this);
            } finally {
                if (m_pSdpService == IntPtr.Zero) {
                    GC.SuppressFinalize(this);
                }
            }
        }


        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~SdpService()
        {
            Dispose(false);
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "disposing")]
        public void Dispose(bool disposing)
        {
            Debug.Assert(m_pSdpService != IntPtr.Zero, "WidcommSdpService Already Destroyed");
            if (m_pSdpService != IntPtr.Zero) {
                NativeMethods.SdpService_Destroy(m_pSdpService);
                m_pSdpService = IntPtr.Zero;
                //_factory.GetWidcommBtInterface().RemoveFromNeedsDisposing(this);
            }
        }
        #endregion


        void ISdpService.AddServiceClassIdList(IList<Guid> serviceClasses)
        {
            Guid[] serviceClassArray = new Guid[serviceClasses.Count];
            serviceClasses.CopyTo(serviceClassArray, 0);
            GCHandle h = GCHandle.Alloc(serviceClassArray, GCHandleType.Pinned);
            try {
                IntPtr pArray = h.AddrOfPinnedObject();
                SDP_RETURN_CODE ret = NativeMethods.SdpService_AddServiceClassIdList(
                    m_pSdpService, serviceClassArray.Length, pArray);
                if (ret != SDP_RETURN_CODE.OK)
                    throw WidcommSocketExceptions.Create_SDP_RETURN_CODE(ret, "AddServiceClassIdList");
            } finally {
                h.Free();
            }
        }

        void ISdpService.AddServiceClassIdList(Guid serviceClass)
        {
            GCHandle h = GCHandle.Alloc(serviceClass, GCHandleType.Pinned);
            try {
                IntPtr pArray = h.AddrOfPinnedObject();
                SDP_RETURN_CODE ret = NativeMethods.SdpService_AddServiceClassIdList(
                    m_pSdpService, 1, pArray);
                if (ret != SDP_RETURN_CODE.OK)
                    throw WidcommSocketExceptions.Create_SDP_RETURN_CODE(ret, "AddServiceClassIdList");
            } finally {
                h.Free();
            }
        }

        void ISdpService.AddRFCommProtocolDescriptor(byte scn)
        {
            Debug.Assert(scn >= BluetoothEndPoint.MinScn, ">=1");
            Debug.Assert(scn <= BluetoothEndPoint.MaxScn, "<=30");
            SDP_RETURN_CODE ret = NativeMethods.SdpService_AddRFCommProtocolDescriptor(
                m_pSdpService, scn);
            if (ret != SDP_RETURN_CODE.OK)
                throw WidcommSocketExceptions.Create_SDP_RETURN_CODE(ret, "AddRFCommProtocolDescriptor");
        }

        void ISdpService.AddServiceName(string serviceName)
        {
            IntPtr pServiceNameAscii = IntPtr.Zero;
#if !NETCF // Don't need ANSI there (and CF doesn't support marshalling to ascii/ansi)
            pServiceNameAscii = Marshal.StringToHGlobalAnsi(serviceName);
#endif
            try {
                SDP_RETURN_CODE ret = NativeMethods.SdpService_AddServiceName(
                    m_pSdpService, serviceName, pServiceNameAscii);
                if (ret != SDP_RETURN_CODE.OK)
                    throw WidcommSocketExceptions.Create_SDP_RETURN_CODE(ret, "AddServiceName");
            } finally {
                if (pServiceNameAscii != IntPtr.Zero)
                    Marshal.FreeHGlobal(pServiceNameAscii);
            }
        }

        void ISdpService.AddAttribute(ushort id, DESC_TYPE dt, int valLen, byte[] val)
        {
            Debug.Assert(id != 0, "Can't add ServiceRecordHandle...");
            SDP_RETURN_CODE ret = NativeMethods.SdpService_AddAttribute(m_pSdpService,
                id, dt, checked((uint)valLen), val);
            if (ret != SDP_RETURN_CODE.OK)
                throw WidcommSocketExceptions.Create_SDP_RETURN_CODE(ret, "AddAttribute");
        }

        //--------
        void ISdpService.CommitRecord()
        {
            //SDP_RETURN_CODE ret = NativeMethods.SdpService_CommitRecord(m_pSdpService);
            //if (ret != SDP_RETURN_CODE.OK)
            //    throw WidcommSocketExceptions_Create(ret);
        }


        //--------
        //
        // Define return codes from the SDP service functions
        //
        internal enum SDP_RETURN_CODE
        {
            OK,
            COULD_NOT_ADD_RECORD,
            INVALID_RECORD,
            INVALID_PARAMETERS

        };

        /// <summary>
        /// Define for service attribute, all the 'Descriptor Type' values.
        /// These are also referred to as 'attribute type' values
        /// </summary>
        internal enum DESC_TYPE : byte
        {
            NULL = 0,
            UINT = 1,
            TWO_COMP_INT = 2,
            UUID = 3,
            TEXT_STR = 4,
            BOOLEAN = 5,
            DATA_ELE_SEQ = 6,
            DATA_ELE_ALT = 7,
            URL = 8
        };


        //--------
        public static ISdpService CreateRfcomm(
            Guid serviceClass, string serviceName, byte scn, WidcommBluetoothFactoryBase factory)
        {
            if (scn < BluetoothEndPoint.MinScn || scn > BluetoothEndPoint.MaxScn)
                throw new ArgumentOutOfRangeException("scn"
#if !NETCF
                    , scn, null
#endif
                    );
            ISdpService sdpService = factory.GetWidcommSdpService();
            sdpService.AddServiceClassIdList(serviceClass);
            sdpService.AddRFCommProtocolDescriptor(scn);
            if (serviceName != null)
                sdpService.AddServiceName(serviceName);
            sdpService.CommitRecord();
            return sdpService;
        }

        public static ISdpService CreateCustom(ServiceRecord record, WidcommBluetoothFactoryBase factory)
        {
            ISdpService sdpService = factory.GetWidcommSdpService();
            WidcommSdpServiceCreator creator = new WidcommSdpServiceCreator();
            creator.CreateServiceRecord(record, sdpService);
            return sdpService;
        }

    }
}
