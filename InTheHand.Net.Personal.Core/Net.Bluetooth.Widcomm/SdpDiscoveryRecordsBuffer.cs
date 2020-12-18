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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;

namespace InTheHand.Net.Bluetooth.Widcomm
{
    sealed class SdpDiscoveryRecordsBuffer : SdpDiscoveryRecordsBufferBase
    {
        private static class NativeMethods
        {
            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern void SdpDiscoveryRec_GetRfcommPorts(IntPtr p_list,
                int recordCount, [Out] int[] ports);
            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern void SdpDiscoveryRec_GetL2CapPsms(IntPtr p_list,
                int recordCount, [Out] int[] ports);

            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern void SdpDiscoveryRec_GetSimpleInfo(IntPtr p_list,
                int recordCount, /*[Out]*/ IntPtr/*SimpleInfo[]*/ info, int cb);

            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern void SdpDiscoveryRec_DeleteArray(IntPtr p_list);
        }

#if NETCF
        const int SizeOfOneRecord = 224;
#else
        const int SizeOfOneRecord = 127;
#endif
        //
        IntPtr m_pBuffer;
        int m_filledCount;
        readonly WidcommBluetoothFactoryBase _fcty;

        //----
        internal SdpDiscoveryRecordsBuffer(WidcommBluetoothFactoryBase fcty,
                IntPtr pList, int recordCount, ServiceDiscoveryParams requestArgs)
            : base(requestArgs)
        {
            if (pList == IntPtr.Zero) {
                GC.SuppressFinalize(this);
                throw new ArgumentException("The native pointer pList is NULL.");
            }
            m_filledCount = recordCount;
            m_pBuffer = pList;
            Debug.Assert(fcty != null, "fcty");
            _fcty = fcty;
        }

        //----
        internal IntPtr Buffer
        {
            get
            {
                EnsureNotDisposed();
                return m_pBuffer;
            }
        }

        /// <summary>
        /// Get the number of records that the buffer contains.
        /// </summary>
        /// -
        /// <value>An integer containing the number of records that the buffer contains,
        /// may be zero.
        /// </value>
        /// -
        /// <exception cref="T:System.InvalidOperationException">The buffer has 
        /// not yet been filled with a CSdpDiscoveryRec list.
        /// </exception>
        /// -
        /// <remarks>
        /// <para>In <see cref="F:InTheHand.Net.Bluetooth.Widcomm.SdpSearchScope.ServiceClassOnly">SdpSearchScope.ServiceClassOnly</see>
        /// this returns the actual number of records as the filtering is done by
        /// the stack.  In <see cref="F:InTheHand.Net.Bluetooth.Widcomm.SdpSearchScope.Anywhere">SdpSearchScope.Anywhere</see>
        /// this returns the pre-filtered number of records.  We do the filtering
        /// so this will likely be greater that the matching number of records.
        /// </para>
        /// </remarks>
        public override int RecordCount
        {
            get
            {
                EnsureNotDisposed();
                if (m_filledCount == -1)
                    throw new InvalidOperationException("Buffer not yet filled.");
                return m_filledCount;
            }
        }

        //----
        public override int[] Hack_GetPorts()
        {
            return GetPorts(NativeMethods.SdpDiscoveryRec_GetRfcommPorts);
        }
        public override int[] Hack_GetPsms()
        {
            return GetPorts(NativeMethods.SdpDiscoveryRec_GetL2CapPsms);
        }

        delegate void GetPortsMethod(IntPtr p_list,
            int recordCount, [Out] int[] ports);

        int[] GetPorts(GetPortsMethod doGet)
        {
            Debug.Assert(m_request.searchScope == SdpSearchScope.ServiceClassOnly, "INFO unexpected searchScope: " + m_request.searchScope);
            EnsureNotDisposed();
            int[] ports = new int[RecordCount];
            for (int i = 0; i < ports.Length; ++i)
                ports[i] = -5;
            doGet(Buffer, ports.Length, ports);
            return ports;
        }

        protected override SimpleInfo[] GetSimpleInfo()
        {
            SimpleInfo[] infoArr = new SimpleInfo[RecordCount]; // We will filter the record in the caller
            GCHandle h = GCHandle.Alloc(infoArr, GCHandleType.Pinned);
            try {
                NativeMethods.SdpDiscoveryRec_GetSimpleInfo(Buffer, infoArr.Length,
                    h.AddrOfPinnedObject(), Marshal.SizeOf(typeof(SimpleInfo)));

            } finally {
                h.Free();
            }
            return infoArr;
        }

        //----
        #region Dispose
        ~SdpDiscoveryRecordsBuffer()
        {
            Dispose(false);
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "disposing")]
        protected override void Dispose(bool disposing)
        {
            Debug.Assert(m_pBuffer != IntPtr.Zero, "SdpDiscoveryRecordsBuffer is already disposed.");
            if (m_pBuffer != IntPtr.Zero) {
                NativeMethods.SdpDiscoveryRec_DeleteArray(m_pBuffer);
                m_pBuffer = IntPtr.Zero;
                m_filledCount = -1;
            }
        }

        protected override void EnsureNotDisposed()
        {
            if (m_pBuffer == IntPtr.Zero)
                throw new ObjectDisposedException("SdpDiscoveryRecordsBuffer");
        }
        #endregion

    }//class


    abstract class SdpDiscoveryRecordsBufferBase : ISdpDiscoveryRecordsBuffer
    {

        internal struct SimpleInfo
        {
            //internal int fake;
            internal readonly Guid serviceUuid; // Set by UnitTest
            internal readonly IntPtr serviceNameWchar;
            internal readonly IntPtr serviceNameChar;
            internal readonly int scn; // -1 for not present, byte otherwise

            [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "TheseFieldsAreSetByPInvoke")]
            public SimpleInfo(int TheseFieldsAreSetByPInvoke)
            {
                this.serviceUuid = Guid.Empty;
                this.serviceNameWchar = IntPtr.Zero;
                this.serviceNameChar = IntPtr.Zero;
                this.scn = -1;
            }

            // For UnitTest
            internal SimpleInfo(Guid serviceUuid, int scn)
            {
                this.serviceUuid = serviceUuid;
                this.scn = scn;
                this.serviceNameWchar = IntPtr.Zero;
                this.serviceNameChar = IntPtr.Zero;
            }
        }

        //--------------------------------------------------------------
        protected readonly ServiceDiscoveryParams m_request;

        protected SdpDiscoveryRecordsBufferBase(ServiceDiscoveryParams query)
        {
            m_request = query;
        }


        //----
        public abstract int RecordCount { get;}

        //----
        public abstract int[] Hack_GetPorts();
        public abstract int[] Hack_GetPsms();

        //----
        List<ServiceRecord> m_records;
        public ServiceRecord[] GetServiceRecords()
        {
            Debug.Assert(m_request.searchScope == SdpSearchScope.Anywhere, "unexpected searchScope: " + m_request.searchScope);
            int classInSCL, classAnywhere;
            if (m_records == null) {
                m_records = new List<ServiceRecord>();
                SdpDiscoveryRecordsBufferBase.SimpleInfo[] infoArr = GetSimpleInfo();
                foreach (SdpDiscoveryRecordsBufferBase.SimpleInfo info in infoArr) {
                    classInSCL = classAnywhere = 0;
                    //Utils.MiscUtils.Trace_WriteLine("fake int: {0}", info.fake);
                    ServiceRecordBuilder bldr = new ServiceRecordBuilder();
                    const ServiceAttributeId FakeDescr = (ServiceAttributeId)(-1);
                    bldr.AddCustomAttribute(new ServiceAttribute(FakeDescr,
                        new ServiceElement(ElementType.TextString,
                            "<partial Widcomm decode>")));
                    //--
                    bldr.AddServiceClass(info.serviceUuid);
                    if (m_request.serviceGuid == info.serviceUuid)
                        ++classInSCL;
                    //--
                    if (info.serviceNameWchar != IntPtr.Zero) {
                        string name = Marshal.PtrToStringUni(info.serviceNameWchar);
                        if (name.Length != 0)
                            bldr.ServiceName = name;
                    } else if (info.serviceNameChar != IntPtr.Zero) {
                        // PtrToStringAnsi is not supported on NETCF.  The field 
                        // is not used by the native code there so that's ok.
#if NETCF
                        Debug.Fail("Don't expect 'serviceNameChar' on PPC.");
#else
                        string name = Marshal.PtrToStringAnsi(info.serviceNameChar);
                        if (name.Length != 0)
                            bldr.ServiceName = name;
#endif
                    }
                    //--
                    if (info.scn == -1) {
                        bldr.ProtocolType = BluetoothProtocolDescriptorType.None;
                    }
                    //--
                    switch (bldr.ProtocolType) {
                        case BluetoothProtocolDescriptorType.GeneralObex:
                            Debug.Fail("GEOP untested");
                            if (m_request.serviceGuid == BluetoothService.ObexProtocol)
                                ++classAnywhere;
                            goto case BluetoothProtocolDescriptorType.Rfcomm;
                        case BluetoothProtocolDescriptorType.Rfcomm:
                            if (m_request.serviceGuid == BluetoothService.RFCommProtocol)
                                ++classAnywhere;
                            if (m_request.serviceGuid == BluetoothService.L2CapProtocol)
                                ++classAnywhere;
                            break;
                        case BluetoothProtocolDescriptorType.None:
                            // We'd better assume L2CAP in the PDL or we'd skip too many
                            // e.g. the SDP server record!
                            if (m_request.serviceGuid == BluetoothService.L2CapProtocol)
                                ++classAnywhere;
                            break;
                    }
                    //--
                    ServiceRecord sr = bldr.ServiceRecord;
                    if (info.scn != -1) {
                        Debug.Assert(bldr.ProtocolType == BluetoothProtocolDescriptorType.Rfcomm,
                            "type=" + bldr.ProtocolType);
                        ServiceRecordHelper.SetRfcommChannelNumber(sr, checked((byte)info.scn));
                    }
                    if (classInSCL > 0 || classAnywhere > 0) {
                        Utils.MiscUtils.Trace_WriteLine("Adding record");
                        m_records.Add(sr);
                    } else { // COVERAGE
                        Utils.MiscUtils.Trace_WriteLine("Skipping record");
                    }
                }
            }
            return m_records.ToArray();
        }

        protected abstract SdpDiscoveryRecordsBufferBase.SimpleInfo[] GetSimpleInfo();


        #region IDisposable Members
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);

        protected abstract void EnsureNotDisposed();
        #endregion
    }
}
