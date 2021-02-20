// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.BlueZ.BluezSdpQuery
// 
// Copyright (c) 2008-2010 In The Hand Ltd, All rights reserved.
// Copyright (c) 2010 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

#if BlueZ

using AsyncResultGsr = InTheHand.Net.AsyncResult<
    System.Collections.Generic.List<InTheHand.Net.Bluetooth.ServiceRecord>, uint>;
using SdpSession = System.IntPtr;  // TODO SdpSessionSafeHandle
//
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using InTheHand.Net.Bluetooth.AttributeIds;
using System.Net.Sockets;
using System.Diagnostics.CodeAnalysis;

namespace InTheHand.Net.Bluetooth.BlueZ
{
    sealed class BluezSdpQuery
    {
        const int RestrictRunawayCount = 500;
        //readonly BluezFactory _fcty;
        readonly ServiceRecordParser _parser;

        //----
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "factory", Justification = "Every class takes this.")]
        public BluezSdpQuery(BluezFactory factory)
        {
            Debug.Assert(factory != null);
            //_fcty = factory;
            _parser = new ServiceRecordParser();
        }


        //----
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Is rethrown")]
        internal IAsyncResult BeginQuery(BluetoothAddress device, Guid svcClass,
            bool rfcommOnly,
            AsyncCallback callback, object state)
        {
            var ar = new AsyncResultGsr(callback, state, 0);
            WaitCallback dlgt = delegate {
                try {
                    var list = DoSdpQueryWithConnect(device, svcClass, rfcommOnly);
                    ar.SetAsCompleted(list, AsyncResultCompletion.IsAsync);
                } catch (Exception ex) {
                    ar.SetAsCompleted(ex, AsyncResultCompletion.IsAsync);
                }
            };
            ThreadPool.QueueUserWorkItem(dlgt, null);
            return ar;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        internal List<ServiceRecord> EndQuery(IAsyncResult ar)
        {
            var ar2 = (AsyncResultGsr)ar;
            return ar2.EndInvoke();
        }

        //----
        internal List<ServiceRecord> DoSdpQueryWithConnect(
            BluetoothAddress addr, Guid svcClass, bool rfcommOnly)
        {
            var svcUuid = new Structs.uuid_t(svcClass);
            //:TestUuids();

            // Connect
            byte[] target = BluezUtils.FromBluetoothAddress(addr);
            Console.WriteLine("Gonna sdp_connect (SafeHandle)...");
            NativeMethods.SdpSessionSafeHandle session = NativeMethods.sdp_connect(StackConsts.BDADDR_ANY,
                target, StackConsts.SdpConnectFlags.SDP_RETRY_IF_BUSY);
            if (session.IsInvalid) {
                //BluezUtils.Throw((BluezError)(-1), "sdp_connect");
                const int WSASERVICE_NOT_FOUND = 10108;
                throw new SocketException(WSASERVICE_NOT_FOUND);
            }
            try {
                // Query
                return DoSdpQuery(session, svcUuid, rfcommOnly);
            } finally {
                session.Close();
            }
        }

        List<ServiceRecord> DoSdpQuery(NativeMethods.SdpSessionSafeHandle session,
            Structs.uuid_t svcUuid, bool rfcommOnly)
        {
            var listAllocs = new List<IntPtr>();
            IntPtr searchList = BluezUtils.sdp_list_append(IntPtr.Zero, svcUuid, listAllocs);

            // Attribute pattern
            IntPtr attridList;
            StackConsts.sdp_attrreq_type_t reqType;
            Console.WriteLine("rfcommOnly: " + rfcommOnly);
            if (rfcommOnly) {
                const UInt16 ClassListId = (ushort)UniversalAttributeId.ServiceClassIdList; //=1
                const UInt16 ProtoDListId = (ushort)UniversalAttributeId.ProtocolDescriptorList; //=4
                reqType = StackConsts.sdp_attrreq_type_t.SDP_ATTR_REQ_INDIVIDUAL;
                attridList = BluezUtils.sdp_list_append(IntPtr.Zero, ClassListId, listAllocs);
                attridList = BluezUtils.sdp_list_append(attridList, ProtoDListId, listAllocs);
            } else {
                const UInt32 allAttributes = 0x0000ffff;
                reqType = StackConsts.sdp_attrreq_type_t.SDP_ATTR_REQ_RANGE;
                attridList = BluezUtils.sdp_list_append(IntPtr.Zero, allAttributes, listAllocs);
            }

            // Query
            Console.WriteLine("sdp_service_search_attr_req in:"
                + " {0}, attrid_list: {1}",
                searchList, attridList);
            IntPtr pResponseList;
            BluezError ret = NativeMethods.sdp_service_search_attr_req(session,
                searchList,
                reqType, attridList,
                out pResponseList);
            Console.WriteLine("sdp_service_search_attr_req ret: {0}, result: {1}",
                ret, pResponseList);
            BluezUtils.CheckAndThrow(ret, "sdp_service_search_attr_req");
            //
            var rList = BuildRecordList(pResponseList);
            return rList;
        }

        //----
        internal List<ServiceRecord> BuildRecordList(IntPtr pResponseList)
        {
            var list = new List<ServiceRecord>();
            if (pResponseList == IntPtr.Zero) {
                Console.WriteLine("Empty responseList.");
                return list;
            }
            int count = 0;
            IntPtr pCurR = pResponseList;
            for (int HACK = 0; HACK < RestrictRunawayCount; ++HACK) {
                ++count;
                var item = (Structs.sdp_list_t)Marshal.PtrToStructure(
                    pCurR, typeof(Structs.sdp_list_t));
                //Console.WriteLine("                       item next: 0x{0:X8}, data 0x{1:X8}", item.next, item.data);
                var r = (Structs.sdp_record_t)Marshal.PtrToStructure(
                    item.data, typeof(Structs.sdp_record_t));
                //Console.WriteLine("rcd.Handle: 0x{0:X}", r.handle);
                var x = BuildRecord(r);
                list.Add(x);
                pCurR = item.next;
                if (pCurR == IntPtr.Zero) {
                    break;
                }
            }//while
            Console.WriteLine("record count: {0}", count);
            return list;
        }

        internal ServiceRecord BuildRecord(Structs.sdp_record_t rcdData)
        {
            var attrList = new List<ServiceAttribute>();
            int count = 0;
            IntPtr pCurAttr = rcdData.attrlist;
            for (int HACK = 0; HACK < RestrictRunawayCount; ++HACK) {
                ++count;
                var item = (Structs.sdp_list_t)Marshal.PtrToStructure(
                    pCurAttr, typeof(Structs.sdp_list_t));
                //Console.WriteLine("                       item next: 0x{0:X8}, data 0x{1:X8}", item.next, item.data);
                var attrData = (Structs.sdp_data_struct__Bytes)Marshal.PtrToStructure(
                    item.data, typeof(Structs.sdp_data_struct__Bytes));
                //Console.WriteLine("      attrId: 0x{0:X}", attrData.attrId);
                var attr = BuildAttribute(item.data);
                attrList.Add(attr);
                // Next
                pCurAttr = item.next;
                if (pCurAttr == IntPtr.Zero) {
                    break;
                }
            }//while
            Console.WriteLine("attr count: {0}", count);
            var r = new ServiceRecord(attrList);
            return r;
        }

        private ServiceAttribute BuildAttribute(IntPtr pAttrData)
        {
            var attrData = (Structs.sdp_data_struct__Bytes)Marshal.PtrToStructure(
                pAttrData, typeof(Structs.sdp_data_struct__Bytes));
            ushort attrId0 = attrData.attrId;
            var attrId = unchecked((ServiceAttributeId)attrId0);
            //Console.WriteLine("attrId: {0} = 0x{1:X}", attrId, attrId0);
            var elem = BuildElement(pAttrData);
            return new ServiceAttribute(attrId0, elem);
        }

        private ServiceElement BuildElement(IntPtr pElemData)
        {
            var elemData = (Structs.sdp_data_struct__Bytes)Marshal.PtrToStructure(
                pElemData, typeof(Structs.sdp_data_struct__Bytes));
            var debug = (Structs.sdp_data_struct__Debug)Marshal.PtrToStructure(
                pElemData, typeof(Structs.sdp_data_struct__Debug));
            //
            ElementTypeDescriptor etd; SizeIndex sizeIndex;
            Map(elemData.dtd, out etd, out sizeIndex);
            //Console.WriteLine("BE: dtd: {0}, unitSize: {1}",
            //    elemData.dtd, elemData.unitSize);
            //
            if (etd == ElementTypeDescriptor.ElementSequence
                    || etd == ElementTypeDescriptor.ElementAlternative) {
                IntPtr pCur = elemData.ReadIntPtr();
                var list = DoSequence(pCur);
#if DEBUG
                ElementTypeDescriptor cover;
                if (etd == ElementTypeDescriptor.ElementAlternative) {
                    cover = etd; //COVERAGE
                } else {
                    cover = etd; //COVERAGE
                }
#endif
                return new ServiceElement(
                    etd == ElementTypeDescriptor.ElementSequence
                        ? ElementType.ElementSequence
                        : ElementType.ElementAlternative,
                    list);
            } else {
                byte[] buf;
                byte[] data;
                if (etd == ElementTypeDescriptor.TextString
                        || etd == ElementTypeDescriptor.Url) {
                    buf = new byte[elemData.unitSize];
                    IntPtr pStr = elemData.ReadIntPtr();
                    Marshal.Copy(pStr, buf, 0, buf.Length - 1);
                    //----
                } else if (etd == ElementTypeDescriptor.Uuid) {
                    var elemDataUuid = (Structs.sdp_data_struct__uuid_t)Marshal.PtrToStructure(
                        pElemData, typeof(Structs.sdp_data_struct__uuid_t));
                    Debug.Assert(elemDataUuid.dtd == elemDataUuid.val.type, "uuid type");
                    int len = FromSizeIndex(sizeIndex);
                    buf = new byte[len];
                    data = elemDataUuid.val.value;
                    Array.Copy(data, buf, buf.Length);
                    //----
                } else {
                    int len = FromSizeIndex(sizeIndex);
                    buf = new byte[len];
                    data = elemData.val;
                    Array.Copy(data, buf, buf.Length);
                }
                int readLen = buf.Length;
                bool networkOrderFalse = false;
                var elem = _parser.ParseContent(networkOrderFalse, networkOrderFalse,
                    buf, 0, buf.Length, ref readLen,
                    etd, sizeIndex, buf.Length, 0);
                return elem;
            }
        }

        private List<ServiceElement> DoSequence(IntPtr pCur)
        {
            List<ServiceElement> list = new List<ServiceElement>();
            if (pCur != IntPtr.Zero) {
                for (int HACK = 0; HACK < Math.Min(4, RestrictRunawayCount); ++HACK) {
                    //Console.WriteLine("pCur: 0x{0:X}", pCur);
                    var e = BuildElement(pCur);
                    list.Add(e);
                    // Next
                    var curElemData = (Structs.sdp_data_struct__uuid_t)Marshal.PtrToStructure(
                        pCur, typeof(Structs.sdp_data_struct__uuid_t));
                    pCur = curElemData.next;
                    if (pCur == IntPtr.Zero) {
                        break;
                    }
                }//for
            }
            return list;
        }

        //----
        static int FromSizeIndex(SizeIndex sizeIndex)
        {
            int len;
            switch (sizeIndex) {
                case SizeIndex.LengthOneByteOrNil:
                    len = 1;
                    break;
                case SizeIndex.LengthTwoBytes:
                    len = 2;
                    break;
                case SizeIndex.LengthFourBytes:
                    len = 4;
                    break;
                case SizeIndex.LengthEightBytes:
                    len = 8;
                    break;
                case SizeIndex.LengthSixteenBytes:
                    len = 16;
                    break;
                case SizeIndex.AdditionalUInt8:
                    len = 1;
                    break;
                case SizeIndex.AdditionalUInt16:
                    len = 1;
                    break;
                case SizeIndex.AdditionalUInt32:
                    len = 1;
                    break;
                default:
                    len = 1;
                    break;
            }
            return len;
        }

        private static void Map(StackConsts.SdpType_uint8_t sdpDataElementType,
            out ElementTypeDescriptor etd, out SizeIndex sizeIndex)
        {
            const SizeIndex NotUsed = SizeIndex.LengthOneByteOrNil;
            switch (sdpDataElementType) {
                // Signed/Unsigned Integer
                case StackConsts.SdpType_uint8_t.UINT8:
                    etd = ElementTypeDescriptor.UnsignedInteger;
                    sizeIndex = SizeIndex.LengthOneByteOrNil;
                    break;
                case StackConsts.SdpType_uint8_t.INT8:
                    etd = ElementTypeDescriptor.TwosComplementInteger;
                    sizeIndex = SizeIndex.LengthOneByteOrNil;
                    break;
                case StackConsts.SdpType_uint8_t.UINT16:
                    etd = ElementTypeDescriptor.UnsignedInteger;
                    sizeIndex = SizeIndex.LengthTwoBytes;
                    break;
                case StackConsts.SdpType_uint8_t.INT16:
                    etd = ElementTypeDescriptor.TwosComplementInteger;
                    sizeIndex = SizeIndex.LengthTwoBytes;
                    break;
                case StackConsts.SdpType_uint8_t.UINT32:
                    etd = ElementTypeDescriptor.UnsignedInteger;
                    sizeIndex = SizeIndex.LengthFourBytes;
                    break;
                case StackConsts.SdpType_uint8_t.INT32:
                    etd = ElementTypeDescriptor.TwosComplementInteger;
                    sizeIndex = SizeIndex.LengthFourBytes;
                    break;
                case StackConsts.SdpType_uint8_t.UINT64:
                    etd = ElementTypeDescriptor.UnsignedInteger;
                    sizeIndex = SizeIndex.LengthEightBytes;
                    break;
                case StackConsts.SdpType_uint8_t.INT64:
                    etd = ElementTypeDescriptor.TwosComplementInteger;
                    sizeIndex = SizeIndex.LengthEightBytes;
                    break;
                case StackConsts.SdpType_uint8_t.UINT128:
                    etd = ElementTypeDescriptor.UnsignedInteger;
                    sizeIndex = SizeIndex.LengthSixteenBytes;
                    break;
                case StackConsts.SdpType_uint8_t.INT128:
                    etd = ElementTypeDescriptor.TwosComplementInteger;
                    sizeIndex = SizeIndex.LengthSixteenBytes;
                    break;
                // Strings
                case StackConsts.SdpType_uint8_t.TEXT_STR_UNSPEC:
                case StackConsts.SdpType_uint8_t.TEXT_STR8:
                case StackConsts.SdpType_uint8_t.TEXT_STR16:
                case StackConsts.SdpType_uint8_t.TEXT_STR32:
                    etd = ElementTypeDescriptor.TextString;
                    sizeIndex = NotUsed;
                    break;
                case StackConsts.SdpType_uint8_t.URL_STR_UNSPEC:
                case StackConsts.SdpType_uint8_t.URL_STR8:
                case StackConsts.SdpType_uint8_t.URL_STR16:
                case StackConsts.SdpType_uint8_t.URL_STR32:
                    etd = ElementTypeDescriptor.Url;
                    sizeIndex = NotUsed;
                    break;
                // UUID
                case StackConsts.SdpType_uint8_t.UUID16:
                    etd = ElementTypeDescriptor.Uuid;
                    sizeIndex = SizeIndex.LengthTwoBytes;
                    break;
                case StackConsts.SdpType_uint8_t.UUID32:
                    etd = ElementTypeDescriptor.Uuid;
                    sizeIndex = SizeIndex.LengthFourBytes;
                    break;
                case StackConsts.SdpType_uint8_t.UUID128:
                    etd = ElementTypeDescriptor.Uuid;
                    sizeIndex = SizeIndex.LengthSixteenBytes;
                    break;
                case StackConsts.SdpType_uint8_t.UUID_UNSPEC:
                    throw new NotSupportedException("UUID_UNSPEC");
                // Seq/Alt
                case StackConsts.SdpType_uint8_t.SEQ_UNSPEC:
                case StackConsts.SdpType_uint8_t.SEQ8:
                case StackConsts.SdpType_uint8_t.SEQ16:
                case StackConsts.SdpType_uint8_t.SEQ32:
                    etd = ElementTypeDescriptor.ElementSequence;
                    sizeIndex = NotUsed;
                    break;
                case StackConsts.SdpType_uint8_t.ALT_UNSPEC:
                case StackConsts.SdpType_uint8_t.ALT8:
                case StackConsts.SdpType_uint8_t.ALT16:
                case StackConsts.SdpType_uint8_t.ALT32:
                    etd = ElementTypeDescriptor.ElementAlternative;
                    sizeIndex = NotUsed;
                    break;
                // Nil/Boolean/etc.
                case StackConsts.SdpType_uint8_t.BOOL:
                    etd = ElementTypeDescriptor.Boolean;
                    sizeIndex = SizeIndex.LengthOneByteOrNil;
                    break;
                case StackConsts.SdpType_uint8_t.DATA_NIL:
                    etd = ElementTypeDescriptor.Nil;
                    sizeIndex = SizeIndex.LengthOneByteOrNil;
                    break;
                //
                default:
                    Debug.Fail("Unexpected SdpType_uint8_t: 0x" + ((int)sdpDataElementType).ToString("X"));
                    etd = ElementTypeDescriptor.Unknown;
                    sizeIndex = SizeIndex.AdditionalUInt32;
                    break;
            }
        }

    }
}
#endif