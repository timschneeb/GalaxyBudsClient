// 32feet.NET - Personal Area Networking for .NET
//
// Net.Bluetooth.StonestreetOne.BluetopiaSdpQuery
// 
// Copyright (c) 2010 Alan J.McFarlane, All rights reserved.
// Copyright (c) 2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the In The Hand Community License - see License.txt

using AsyncResultGsr = InTheHand.Net.AsyncResult<
    System.Collections.Generic.List<InTheHand.Net.Bluetooth.ServiceRecord>, uint>;
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using InTheHand.Net.Bluetooth.AttributeIds;
using System.Globalization;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth.Widcomm;
using Utils;

namespace InTheHand.Net.Bluetooth.StonestreetOne
{
    class BluetopiaSdpQuery
    {
        static int _callbackCount;
        //
        readonly BluetopiaFactory _fcty;
        readonly NativeMethods.SDP_Response_Callback _callback;
        readonly ServiceRecordParser _parser;
        //
        AsyncResultGsr _ar;

        public BluetopiaSdpQuery(BluetopiaFactory factory)
        {
            Debug.Assert(factory != null);
            _fcty = factory;
            _callback = HandleSDP_Response_Callback;
            _parser = new ServiceRecordParser();
        }

        //----
        static bool _brokenInterlockedExchange;

        internal static T MyInterlockedExchange<T>(ref T location1, T value)
            where T : class
        {
            T old;
            if (!_brokenInterlockedExchange) {
                try {
                    old = Interlocked.Exchange<T>(ref location1, value);
                    return old;
                } catch (NotSupportedException) {
                    _brokenInterlockedExchange = true;
                    Debug.Fail("Interlocked.Exchange threw NotSupportedException!");
                }
            }
            // When doing some smoke testing (using BluetopiaFakeFactory 
            // running on a ASUS device with the Widcomm stack) that call
            // fails with NotSupportedException!!!  Version=2.0.7045.0
            // But is the mentioned "ative exception in marshalling code when using Interlocked.Exchange"??
            // WORKAROUND:
            old = location1;
            location1 = value;
            return old;
        }

        internal static T MyInterlockedCompareExchange<T>(ref T location1, T value, T comparand)
            where T : class
        {
            T old;
            if (!_brokenInterlockedExchange) {
                try {
                    old = Interlocked.CompareExchange<T>(ref location1, value, comparand);
                    return old;
                } catch (NotSupportedException) {
                    _brokenInterlockedExchange = true;
                    Debug.Fail("Interlocked.Exchange threw NotSupportedException!");
                }
            }
            // When doing some smoke testing (using BluetopiaFakeFactory 
            // running on a ASUS device with the Widcomm stack) that call
            // fails with NotSupportedException!!!  Version=2.0.7045.0
            // But is the mentioned "ative exception in marshalling code when using Interlocked.Exchange"??
            // WORKAROUND:
            old = location1;
            if(old == comparand) location1 = value;
            return old;
        }

        internal IAsyncResult BeginQuery(BluetoothAddress device, Guid svcClass,
            bool rfcommOnly,
            AsyncCallback asyncCallback, object state)
        {
            int id0 = Interlocked.Increment(ref _callbackCount);
            var id = unchecked((uint)id0);
            var ar = new AsyncResultGsr(asyncCallback, state, id);
            // Attribute range/etc
            Structs.SDP_Attribute_ID_List_Entry[] attrIdListArr;
            var attrIdAll = Structs.SDP_Attribute_ID_List_Entry.CreateRange(0, 0xFFFF);
            var attrIdSvcList = Structs.SDP_Attribute_ID_List_Entry.CreateItem(
                UniversalAttributeId.ServiceClassIdList);
            var attrIdProto = Structs.SDP_Attribute_ID_List_Entry.CreateItem(
                UniversalAttributeId.ProtocolDescriptorList);
            if (rfcommOnly) {
                attrIdListArr = new[] { attrIdSvcList, attrIdProto };
            } else {
                attrIdListArr = new[] { attrIdAll };
            }
            // UUID
            var uuidArg = new Structs.SDP_UUID_Entry(svcClass);
            Structs.SDP_UUID_Entry[] uuidArr = new[] { uuidArg };
            AsyncResultGsr old;
            old = MyInterlockedCompareExchange<AsyncResultGsr>(ref _ar, ar, null);
            if (old != null)
                throw new NotImplementedException("One at a time please.");
            bool trySuccess = false;
            try {
                // Call
                _fcty.CancelAllQueryNames();
                int hRequest = _fcty.Api.SDP_Service_Search_Attribute_Request(_fcty.StackId,
                    BluetopiaUtils.BluetoothAddressAsInteger(device),
                    (uint)uuidArr.Length, uuidArr,
                    (uint)attrIdListArr.Length, attrIdListArr,
                    _callback, id);
                var ret = (BluetopiaError)hRequest;
                int i;
                for (i = 0; i < 5 && ret == BluetopiaError.ATTEMPTING_CONNECTION_TO_DEVICE; ++i) {
                    // See BluetopiaRfcommStream.DoOpenClient... Assuming here a
                    // previous baseband connection was closing right when as we
                    // wanted to connect, so retry.
                    if (i > 0) Thread.Sleep(100); // Try right away, then after 100ms sleeps
                    hRequest = _fcty.Api.SDP_Service_Search_Attribute_Request(_fcty.StackId,
                        BluetopiaUtils.BluetoothAddressAsInteger(device),
                        (uint)uuidArr.Length, uuidArr,
                        (uint)attrIdListArr.Length, attrIdListArr,
                        _callback, id);
                    ret = (BluetopiaError)hRequest;
                }
                if (i > 0) {
                    Debug.WriteLine("Auto-retry " + i + " after ATTEMPTING_CONNECTION_TO_DEVICE for SDP_Service_Search_Attribute_Request");
                }
                BluetopiaUtils.CheckAndThrowZeroIsIllegal(ret, "SDP_Service_Search_Attribute_Request");
                trySuccess = true;
                return ar;
            } finally {
                if (!trySuccess) {
                    // If we got here it must be our value in the variable!
                    var replaced = MyInterlockedExchange(ref _ar, null);
                    Debug.Assert(replaced != null);
                    Debug.Assert(replaced == ar);
                }
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        internal List<ServiceRecord> EndQuery(IAsyncResult ar)
        {
            var ar2 = (AsyncResultGsr)ar;
            return ar2.EndInvoke();
        }

        const int SocketError_NotSocket = 10038;

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Mustn't let exception return to stack.")]
#if DEBUG
        internal
#endif
 void HandleSDP_Response_Callback(uint BluetoothStackID, uint SDPRequestID,
                IntPtr/*"SDP_Response_Data_t *"*/ pSDP_Response_Data, uint CallbackParameter)
        {
            var ar = MyInterlockedExchange(ref _ar, null);
            if (ar == null) {
                Debug.Fail("No outstanding query...");
                return;
            } try {
                HandleSDP_Response_Callback2(BluetoothStackID, SDPRequestID,
                    pSDP_Response_Data, CallbackParameter, ar);
            } catch (Exception ex) {
                ar.SetAsCompleted(ex, AsyncResultCompletion.MakeAsync);
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "BluetoothStackID")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "SDPRequestID")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "CallbackParameter")]
        void HandleSDP_Response_Callback2(uint BluetoothStackID, uint SDPRequestID,
            IntPtr/*"SDP_Response_Data_t *"*/ pSDP_Response_Data, uint CallbackParameter,
            AsyncResultGsr ar)
        {
            Debug.Assert(ar != null);
            var stru = (Structs.SDP_Response_Data)Marshal.PtrToStructure(
                pSDP_Response_Data, typeof(Structs.SDP_Response_Data));
            var type = stru.SDP_Response_Data_Type;
            int? socketError = null;
            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture,
                "HandleSDP_Response_Callback2: {0}=0x{1:X}", type, (int)type));
            switch (type) {
                case StackConsts.SDP_Response_Data_Type.Timeout:
                // Get ConnectionError when connecting to a non-existent device.
                case StackConsts.SDP_Response_Data_Type.ConnectionError:
                    // (There no further info about the error, whether non-connect, no response packet etc).
                    socketError = (int)SocketError.NotSocket;
                    break;
                case StackConsts.SDP_Response_Data_Type.ErrorResponse:
                    //var errorStru = (Structs.SDP_Response_Data_t__SDP_Error_Response_Data_t)Marshal.PtrToStructure(
                    //    pSDP_Response_Data, typeof(Structs.SDP_Response_Data_t__SDP_Error_Response_Data_t));
                    //var ret = (BluetopiaError)errorStru.Error_Code;
                    socketError = (int)SocketError.NoData;
                    break;
                case StackConsts.SDP_Response_Data_Type.ServiceSearchAttributeResponse:
                    List<ServiceRecord> srList = BuildRecordList(pSDP_Response_Data);
                    ar.SetAsCompleted(srList, AsyncResultCompletion.MakeAsync);
                    return;
                case StackConsts.SDP_Response_Data_Type.ServiceSearchResponse:
                case StackConsts.SDP_Response_Data_Type.ServiceAttributeResponse:
                    // Not used by us, so shouldn't occur...
                    break;
                default:
                    Debug.Fail("unexpected response type: " + type);
                    break;
            }//switch
            //
            Exception ex;
            if (socketError != null) {
                Utils.MiscUtils.Trace_WriteLine("SDP error response: " + type);
                ex = new SocketException(SocketError_NotSocket);
                ar.SetAsCompleted(ex, AsyncResultCompletion.MakeAsync);
            } else {
                // NOP -- We handle ServiceSearchAttributeResponse above.
            }
        }

        internal List<ServiceRecord> BuildRecordList(IntPtr pSSSARD)
        {
            var sizeofSSARD = Marshal.SizeOf(typeof(Structs.SDP_Service_Attribute_Response_Data));
            var rList = new List<ServiceRecord>();
            var svcSrchAttrRsp = (Structs.SDP_Response_Data__SDP_Service_Search_Attribute_Response_Data)
                Marshal.PtrToStructure(pSSSARD, typeof(Structs.SDP_Response_Data__SDP_Service_Search_Attribute_Response_Data));
            IntPtr pCur = svcSrchAttrRsp.pSDP_Service_Attribute_Response_Data;
            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture,
                "BuildRecordList Number_Service_Records: {0}",
                svcSrchAttrRsp.Number_Service_Records));
            for (int i = 0; i < svcSrchAttrRsp.Number_Service_Records; ++i) {
                var curSR = BuildRecord(pCur);
                rList.Add(curSR);
                //next SR
                pCur = Pointers.Add(pCur, sizeofSSARD);
            }//for
            return rList;
        }

#if false
        internal ServiceRecord BuildRecord(IntPtr pAttrRsp)
        {
            var attrRsp = (Structs.SDP_Service_Attribute_Response_Data_t_ArrayOfChildren)
                Marshal.PtrToStructure(pAttrRsp, typeof(Structs.SDP_Service_Attribute_Response_Data_t_ArrayOfChildren));
            return BuildRecord(attrRsp);
        }
#else
        internal ServiceRecord BuildRecord(IntPtr pAttrRsp)
        {
            var attrRsp = (Structs.SDP_Service_Attribute_Response_Data)
                Marshal.PtrToStructure(pAttrRsp, typeof(Structs.SDP_Service_Attribute_Response_Data));
            return BuildRecord(attrRsp);
        }
#endif

        internal ServiceRecord BuildRecord(Structs.SDP_Service_Attribute_Response_Data_ArrayOfChildren attrRsp)
        {
            var attrList = new List<ServiceAttribute>();
            for (int curAttr = 0; curAttr < attrRsp.Number_Attribute_Values; ++curAttr) {
                var attr = BuildAttribute(attrRsp.aSDP_Service_Attribute_Value_Data[curAttr]);
                attrList.Add(attr);
            }//for
            var r = new ServiceRecord(attrList);
            return r;
        }

        internal ServiceRecord BuildRecord(Structs.SDP_Service_Attribute_Response_Data attrRsp) //ORIG
        {
            int sizeofAttrData = Marshal.SizeOf(typeof(Structs.SDP_Service_Attribute_Value_Data));
            var attrList = new List<ServiceAttribute>();
            IntPtr pCur = attrRsp.pSDP_Service_Attribute_Value_Data;
            for (int curAttr = 0; curAttr < attrRsp.Number_Attribute_Values; ++curAttr) {
                var attr = BuildAttribute(pCur);
                attrList.Add(attr);
                // Next
                pCur = Pointers.Add(pCur, sizeofAttrData);
            }//for
            var r = new ServiceRecord(attrList);
            return r;
        }

        private ServiceAttribute BuildAttribute(IntPtr pAttrData)
        {
            var attrData = (Structs.SDP_Service_Attribute_Value_Data)
                Marshal.PtrToStructure(pAttrData, typeof(Structs.SDP_Service_Attribute_Value_Data));
            return BuildAttribute(attrData);
        }

        private ServiceAttribute BuildAttribute(Structs.SDP_Service_Attribute_Value_Data attrData)
        {
            ushort attrId0 = attrData.Attribute_ID;
            IntPtr pElemData = attrData.pSDP_Data_Element;
            var elem = BuildElement(pElemData);
            return new ServiceAttribute(attrId0, elem);
        }

        private ServiceElement BuildElement(IntPtr pElemData)
        {
            var elemData = (Structs.SDP_Data_Element)Marshal.PtrToStructure(
                pElemData, typeof(Structs.SDP_Data_Element));
            var sizeofElemData = Marshal.SizeOf(typeof(Structs.SDP_Data_Element));
            //
            ElementTypeDescriptor etd; SizeIndex sizeIndex;
            Map(elemData.SDP_Data_Element_Type, out etd, out sizeIndex);
            //
            const int OffsetOf_FakeAtUnionPosition = 8;
#if !NETCF
            var dbgOffset = Marshal.OffsetOf(typeof(Structs.SDP_Data_Element), "FakeAtUnionPosition");
            Debug.Assert(new IntPtr(OffsetOf_FakeAtUnionPosition) == dbgOffset,
                "OffsetOf_FakeAtUnionPosition but: " + dbgOffset);
#endif
            IntPtr pDataInStruct = Pointers.Add(pElemData, OffsetOf_FakeAtUnionPosition);
            IntPtr pArrElements = Marshal.ReadIntPtr(pDataInStruct);
            if (etd == ElementTypeDescriptor.ElementSequence
                    || etd == ElementTypeDescriptor.ElementAlternative) {
                var list = new List<ServiceElement>();
                var pCur = pArrElements;
                for (int i = 0; i < elemData.SDP_Data_Element_Length; ++i) {
                    var e = BuildElement(pCur);
                    list.Add(e);
                    // Next
                    pCur = Pointers.Add(pCur, sizeofElemData);
                }//for
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
                byte[] buf = new byte[elemData.SDP_Data_Element_Length];
                IntPtr pData;
                if (etd == ElementTypeDescriptor.TextString
                        || etd == ElementTypeDescriptor.Url) {
                    pData = pArrElements;
                } else {
                    pData = pDataInStruct;
                }
                Marshal.Copy(pData, buf, 0, buf.Length);
                int readLen = buf.Length;
                var elem = _parser.ParseContent(false, true,
                    buf, 0, buf.Length, ref readLen,
                    etd, sizeIndex, buf.Length, 0);
                return elem;
            }
        }

        private static void Map(StackConsts.SDP_Data_Element_Type sdpDataElementType,
            out ElementTypeDescriptor etd, out SizeIndex sizeIndex)
        {
            const SizeIndex NotUsed = SizeIndex.LengthOneByteOrNil;
            switch (sdpDataElementType) {
                // Signed/Unsigned Integer
                case StackConsts.SDP_Data_Element_Type.UnsignedInteger1Byte:
                    etd = ElementTypeDescriptor.UnsignedInteger;
                    sizeIndex = SizeIndex.LengthOneByteOrNil;
                    break;
                case StackConsts.SDP_Data_Element_Type.SignedInteger1Byte:
                    etd = ElementTypeDescriptor.TwosComplementInteger;
                    sizeIndex = SizeIndex.LengthOneByteOrNil;
                    break;
                case StackConsts.SDP_Data_Element_Type.UnsignedInteger2Bytes:
                    etd = ElementTypeDescriptor.UnsignedInteger;
                    sizeIndex = SizeIndex.LengthTwoBytes;
                    break;
                case StackConsts.SDP_Data_Element_Type.SignedInteger2Bytes:
                    etd = ElementTypeDescriptor.TwosComplementInteger;
                    sizeIndex = SizeIndex.LengthTwoBytes;
                    break;
                case StackConsts.SDP_Data_Element_Type.UnsignedInteger4Bytes:
                    etd = ElementTypeDescriptor.UnsignedInteger;
                    sizeIndex = SizeIndex.LengthFourBytes;
                    break;
                case StackConsts.SDP_Data_Element_Type.SignedInteger4Bytes:
                    etd = ElementTypeDescriptor.TwosComplementInteger;
                    sizeIndex = SizeIndex.LengthFourBytes;
                    break;
                case StackConsts.SDP_Data_Element_Type.UnsignedInteger8Bytes:
                    etd = ElementTypeDescriptor.UnsignedInteger;
                    sizeIndex = SizeIndex.LengthEightBytes;
                    break;
                case StackConsts.SDP_Data_Element_Type.SignedInteger8Bytes:
                    etd = ElementTypeDescriptor.TwosComplementInteger;
                    sizeIndex = SizeIndex.LengthEightBytes;
                    break;
                case StackConsts.SDP_Data_Element_Type.UnsignedInteger16Bytes:
                    etd = ElementTypeDescriptor.UnsignedInteger;
                    sizeIndex = SizeIndex.LengthSixteenBytes;
                    break;
                case StackConsts.SDP_Data_Element_Type.SignedInteger16Bytes:
                    etd = ElementTypeDescriptor.TwosComplementInteger;
                    sizeIndex = SizeIndex.LengthSixteenBytes;
                    break;
                // Strings
                case StackConsts.SDP_Data_Element_Type.TextString:
                    etd = ElementTypeDescriptor.TextString;
                    sizeIndex = NotUsed;
                    break;
                case StackConsts.SDP_Data_Element_Type.URL:
                    etd = ElementTypeDescriptor.Url;
                    sizeIndex = NotUsed;
                    break;
                // UUID
                case StackConsts.SDP_Data_Element_Type.UUID_16:
                    etd = ElementTypeDescriptor.Uuid;
                    sizeIndex = SizeIndex.LengthTwoBytes;
                    break;
                case StackConsts.SDP_Data_Element_Type.UUID_32:
                    etd = ElementTypeDescriptor.Uuid;
                    sizeIndex = SizeIndex.LengthTwoBytes;
                    break;
                case StackConsts.SDP_Data_Element_Type.UUID_128:
                    etd = ElementTypeDescriptor.Uuid;
                    sizeIndex = SizeIndex.LengthFourBytes;
                    break;
                // Seq/Alt
                case StackConsts.SDP_Data_Element_Type.Sequence:
                    etd = ElementTypeDescriptor.ElementSequence;
                    sizeIndex = NotUsed;
                    break;
                case StackConsts.SDP_Data_Element_Type.Alternative:
                    etd = ElementTypeDescriptor.ElementAlternative;
                    sizeIndex = NotUsed;
                    break;
                // Nil/Boolean/etc.
                case StackConsts.SDP_Data_Element_Type.Boolean:
                    etd = ElementTypeDescriptor.Boolean;
                    sizeIndex = SizeIndex.LengthOneByteOrNil;
                    break;
                case StackConsts.SDP_Data_Element_Type.NIL:
                    etd = ElementTypeDescriptor.Nil;
                    sizeIndex = SizeIndex.LengthOneByteOrNil;
                    break;
                case StackConsts.SDP_Data_Element_Type.NULL:
                    Debug.Fail("SDP_Data_Element_Type.deNULL");
                    etd = ElementTypeDescriptor.Unknown;
                    sizeIndex = SizeIndex.LengthOneByteOrNil;
                    break;
                //
                default:
                    Debug.Fail("Unexpected SDP_Data_Element_Type: 0x" + ((int)sdpDataElementType).ToString("X"));
                    etd = ElementTypeDescriptor.Unknown;
                    sizeIndex = SizeIndex.AdditionalUInt32;
                    break;
            }
        }

    }
}
