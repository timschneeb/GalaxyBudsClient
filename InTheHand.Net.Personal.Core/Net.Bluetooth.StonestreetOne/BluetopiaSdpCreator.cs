// 32feet.NET - Personal Area Networking for .NET
//
// Net.Bluetooth.StonestreetOne.IBluetopiaApi
// 
// Copyright (c) 2010 Alan J.McFarlane, All rights reserved.
// Copyright (c) 2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the In The Hand Community License - see License.txt

using Byte_t = System.Byte;
using Word_t = System.UInt16;   /* Generic 16 bit Container.  */
using DWord_t = System.UInt32;   /* Generic 32 bit Container.  */
//
using System;
using System.Diagnostics;
using System.Collections.Generic;
using InTheHand.Net.Bluetooth.AttributeIds;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Utils;

namespace InTheHand.Net.Bluetooth.StonestreetOne
{
    /// <exclude/>
    //sealed
    class BluetopiaSdpCreator : ServiceRecordCreator, IDisposable
    {
#pragma warning disable 1591 // XmlDocs
        BluetopiaFactory _fcty;
        //
        bool _isAttrId;
        ServiceAttributeId _attrId;
        Structs.SDP_Data_Element__Class _element;
        Stack<SeqAltHolder> _seqAltStack = new Stack<SeqAltHolder>();
        uint? _hSR;
        //
        static byte[] s_dummy = new byte[1024];

        //--------
        public static BluetopiaSdpCreator CreateTestable()
        {
            var f = InTheHand.Net.Bluetooth.Factory.BluetoothFactory
                .GetTheFactoryOfTypeOrDefault<BluetopiaFactory>();
            return new BluetopiaSdpCreator(f);
        }

        protected void GetRecordAttrInfo(out uint? hSR, out ServiceAttributeId attrId)
        {
            hSR = _hSR;
            attrId = _attrId;
        }

        //--------
        internal BluetopiaSdpCreator(BluetopiaFactory fcty)
        {
            Debug.Assert(fcty != null, "fcty NULL!");
            _fcty = fcty;
        }


        //----
        #region Hold Element state
        private void SetCurElement(Structs.SDP_Data_Element__Class e)
        {
            if (_seqAltStack.Count != 0) {
                Debug.Assert(_element == null, "(overwrite???!)");
                _seqAltStack.Peek().SeqAltChildren.Add(e);
            } else {
                Debug.Assert(_element == null, "overwrite!!!");
                this._element = e;
            }
        }

        class SeqAltHolder
        {
            public SeqAltHolder(ElementType seqOrAlt)
            {
                SeqOrAlt = seqOrAlt;
                SeqAltChildren = new List<Structs.SDP_Data_Element__Class>();
            }

            public ElementType? SeqOrAlt { get; private set; }
            public List<Structs.SDP_Data_Element__Class> SeqAltChildren { get; private set; }
        }
        #endregion

        //----
        public new void CreateServiceRecord(ServiceRecord record)
        {
            if (record == null) throw new ArgumentNullException("record");
            if (_hSR.HasValue)
                throw new InvalidOperationException("One at a time please.");
            //
            ServiceElement svcIdListElement;
            var newList = new List<ServiceAttribute>(Math.Max(record.Count - 1, 0));
            foreach (var cur in record) {
                if (cur.Id == UniversalAttributeId.ServiceClassIdList) {
                    svcIdListElement = cur.Value;
                } else {
                    newList.Add(cur);
                }
            }
            var newRecord = new ServiceRecord(newList);
            Debug.Assert(!newRecord.Contains(UniversalAttributeId.ServiceClassIdList), "DOES!! contain SvcIdList");
            // Add the list of UUIDs
            var el = record.GetAttributeById(UniversalAttributeId.ServiceClassIdList).Value;
            var classList = el.GetValueAsElementList();
            var svcIdListList = new Structs.SDP_UUID_Entry_Bytes[classList.Count];
            for (int i = 0; i < svcIdListList.Length; ++i) {
                svcIdListList[i] = Structs.SDP_UUID_Entry_Bytes.Create(classList[i]);
            }
            Debug.Assert(svcIdListList.Length != 0, "Docs: 'the number of UUID Entries'...'CANNOT be zero'.");
            //
            int hSR = _fcty.Api.SDP_Create_Service_Record(_fcty.StackId,
                checked((uint)svcIdListList.Length), svcIdListList);
            var ret = (BluetopiaError)hSR;
            BluetopiaUtils.CheckAndThrowZeroIsIllegal(ret, "SDP_Create_Service_Record");
            _hSR = unchecked((uint)hSR);
            //
            int count = base.CreateServiceRecord(newRecord, s_dummy);
        }

        public void DeleteServiceRecord()
        {
            if (!_hSR.HasValue) {
                //throw new InvalidOperationException("No record was created.");
                Debug.Fail("Delete -- No record was created.");
                return;
            }
            var hSR = _hSR.Value;
            _hSR = null;
            var ret = _fcty.Api.SDP_Delete_Service_Record(_fcty.StackId, hSR);
            BluetopiaUtils.Assert(ret, "SDP_Delete_Service_Record");
        }

        //----
        protected sealed override void WriteAttribute(ServiceAttribute attr, byte[] buffer, ref int offset)
        {
            bool doneKnown = WriteWellKnownAttribute(attr);
            if (doneKnown)
                return;
            //
            base.WriteAttribute(attr, buffer, ref offset);
            //
            var e = _element;
            var ret = AddAttribute(e);
            BluetopiaUtils.CheckAndThrow(ret, "SDP_Add_Attribute");
            _element = null;
        }

        private void ConvertToNativeStructs(Structs.SDP_Data_Element__Class eNotNative,
            out Structs.SDP_Data_Element__Struct e,
            List<IntPtr> toFree)
        {
            var eEa = eNotNative as Structs.SDP_Data_Element__Class_ElementArray;
            if (eEa != null) {
                Debug.Assert(eEa.SDP_Data_Element_Length == eEa.elementArray.Length);
                var newEa = new Structs.SDP_Data_Element__Struct[eEa.SDP_Data_Element_Length];
                for (int i = 0; i < newEa.Length; ++i) {
                    ConvertToNativeStructs(eEa.elementArray[i], out newEa[i], toFree);
                }
                var p = CopyArrayToHGlobal(newEa, toFree);
                byte[] addrArr = PointerAsBytes(p);
                e = new Structs.SDP_Data_Element__Struct(eEa.SDP_Data_Element_Type,
                    eEa.SDP_Data_Element_Length, addrArr);
            } else {
                var eIba = eNotNative as Structs.SDP_Data_Element__Class_InlineByteArray;
                if (eIba != null) {
                    e = new Structs.SDP_Data_Element__Struct(eIba.SDP_Data_Element_Type,
                        eNotNative.SDP_Data_Element_Length, eIba._elementValue);
                    Debug.Assert(eNotNative.SDP_Data_Element_Type == e.SDP_Data_Element_Type);
                    Debug.Assert(eNotNative.SDP_Data_Element_Length == e.SDP_Data_Element_Length);
                    Debug.Assert(eIba._elementValue.Length == e._content.Length);
                    if (eIba._elementValue.Length != 0) {
                        Debug.Assert(eIba._elementValue[0] == e._content[0]);
                        Debug.Assert(eIba._elementValue[eIba._elementValue.Length - 1]
                            == e._content[e._content.Length - 1]);
                    }
                } else {
                    var eNiBA = eNotNative as Structs.SDP_Data_Element__Class_NonInlineByteArray;
                    if (eNiBA != null) {
                        int len = checked((int)eNiBA.SDP_Data_Element_Length);
                        var p = Marshal.AllocHGlobal(len);
                        Debug.Assert(eNiBA._elementValue.Length == eNiBA.SDP_Data_Element_Length);
                        Marshal.Copy(eNiBA._elementValue, 0, p, len);
                        byte[] addrArr = PointerAsBytes(p);
                        e = new Structs.SDP_Data_Element__Struct(eNiBA.SDP_Data_Element_Type,
                            eNiBA.SDP_Data_Element_Length, addrArr);
                        Debug.Assert(eNotNative.SDP_Data_Element_Type == e.SDP_Data_Element_Type);
                        Debug.Assert(eNotNative.SDP_Data_Element_Length == e.SDP_Data_Element_Length);
                        Debug.Assert(eNiBA._elementValue.Length == e.SDP_Data_Element_Length);
                        if (eNiBA._elementValue.Length != 0) {
                            Debug.Assert(eNiBA._elementValue[0] == Marshal.ReadByte(p, 0));
                            Debug.Assert(eNiBA._elementValue[eNiBA._elementValue.Length - 1]
                                == Marshal.ReadByte(p, checked((int)e.SDP_Data_Element_Length) - 1));
                        }
                    } else {
                        throw new InvalidOperationException("Unknown type.");
                    }
                }
            }
        }

        protected virtual BluetopiaError AddAttribute(Structs.SDP_Data_Element__Class eNotNative)
        {
            Structs.SDP_Data_Element__Struct e;
            var toFree = new List<IntPtr>();
            try {
                ConvertToNativeStructs(eNotNative, out e, toFree);
                var ret = _fcty.Api.SDP_Add_Attribute(_fcty.StackId, _hSR.Value,
                    unchecked((ushort)this._attrId), ref e);
                return ret;
            } finally {
                foreach (var p in toFree) {
                    Marshal.FreeHGlobal(p);
                }
            }
        }

        private static IntPtr CopyArrayToHGlobal<T>(T[] arr, List<IntPtr> toFree)
            where T : struct
        {
            // NETCF P/Invoke doesn't support marshalling an array of structs
            // so we have to marshal it ourselves.
            var so = Marshal.SizeOf(typeof(T));
            var p = Marshal.AllocHGlobal(so * arr.Length);
            toFree.Add(p);
            var pCur = p;
            for (int i = 0; i < arr.Length; ++i) {
                Marshal.StructureToPtr(arr[i], pCur, false);
                pCur = Pointers.Add(pCur, so);
            }
#if DEBUG
            var pEnd = Pointers.Add(p, (so * arr.Length));
            Debug.Assert(pCur == pEnd);
#endif
            return p;
        }

        private static byte[] PointerAsBytes(IntPtr p)
        {
            byte[] arr;
            switch (IntPtr.Size) {
                case 4:
                    var i4 = p.ToInt32();
                    arr = BitConverter.GetBytes(i4);
                    break;
                case 8:
                    var i8 = p.ToInt64();
                    arr = BitConverter.GetBytes(i8);
                    break;
                default:
                    throw new NotImplementedException();
            }
            return arr;
        }

        static bool WriteWellKnownAttribute(ServiceAttribute attr) //, ServiceRecord record)
        {
            switch (attr.Id) {
                case UniversalAttributeId.ServiceClassIdList:
                    throw new InvalidOperationException("INTERNAL ERROR: ServiceClassIdList should have been handled earlier.");
                default:
                    return false;
            }
        }

        //--------
        protected sealed override int CreateAttrId(ServiceAttributeId attrId, byte[] buf, int offset)
        {
            this._attrId = attrId;
            this._isAttrId = true;
            int ret = base.CreateAttrId(attrId, buf, offset);
            this._isAttrId = false;
            return ret;
        }

        //---------
        protected sealed override int CreateElement(ServiceElement element, byte[] buf, int offset)
        {
            if (element.ElementType == ElementType.ElementSequence
                   || element.ElementType == ElementType.ElementAlternative) {
                _seqAltStack.Push(new SeqAltHolder(element.ElementType));
            }
            //
            int totalLength = base.CreateElement(element, buf, offset);
            //
            if (element.ElementType == ElementType.ElementSequence
                   || element.ElementType == ElementType.ElementAlternative) {
                var e = ToElement(_seqAltStack.Pop());
                SetCurElement(e);
            }
            return totalLength;
        }

        private static Structs.SDP_Data_Element__Class_ElementArray ToElement(SeqAltHolder seqAlt)
        {
            Debug.Assert(seqAlt.SeqOrAlt.HasValue);
            Debug.Assert(seqAlt.SeqAltChildren != null);
            var det = ToSDP_Data_Element_Type(seqAlt.SeqOrAlt.Value);
            Debug.Assert(det == StackConsts.SDP_Data_Element_Type.Sequence
                || det == StackConsts.SDP_Data_Element_Type.Alternative, "but was: " + det);
            var e = new Structs.SDP_Data_Element__Class_ElementArray(
                det, seqAlt.SeqAltChildren.Count, seqAlt.SeqAltChildren.ToArray());
            return e;
        }

        //--------
        protected sealed override void WriteFixedLength(ServiceElement element, byte[] valueBytes, byte[] buf, ref int offset, out int totalLength)
        {
            if (this._isAttrId) {
            } else {
                byte[] value = (byte[])valueBytes.Clone();
                if (element.ElementTypeDescriptor == ElementTypeDescriptor.UnsignedInteger
                        || element.ElementTypeDescriptor == ElementTypeDescriptor.TwosComplementInteger
                    // The two UUIDs are big-endian...
                    //|| element.ElementType == ElementType.Uuid16
                    //|| element.ElementType == ElementType.Uuid32
                        ) {
                    Array.Reverse(value);
                }
                var det = ToSDP_Data_Element_Type(element.ElementType);
                Debug.Assert(value != null, "What length to write with null array value?");
                SetCurElement(new Structs.SDP_Data_Element__Class_InlineByteArray(det,
                    value.Length, value));
            }
            //
            base.WriteFixedLength(element, valueBytes, buf, ref offset, out totalLength);
        }

        protected sealed override void WriteVariableLength(ServiceElement element, byte[] valueBytes, byte[] buf, ref int offset, out int totalLength)
        {
            var det = ToSDP_Data_Element_Type(element.ElementType);
            SetCurElement(new Structs.SDP_Data_Element__Class_NonInlineByteArray(det,
                valueBytes.Length, valueBytes));
            //
            base.WriteVariableLength(element, valueBytes, buf, ref offset, out totalLength);
        }

        //--------
        private static StackConsts.SDP_Data_Element_Type ToSDP_Data_Element_Type(ElementType elementType)
        {
            StackConsts.SDP_Data_Element_Type val;
            switch (elementType) {
                case ElementType.Nil:
                    val = StackConsts.SDP_Data_Element_Type.NIL;
                    break;
                case ElementType.Boolean:
                    val = StackConsts.SDP_Data_Element_Type.Boolean;
                    break;
                //
                case ElementType.UInt8:
                    val = StackConsts.SDP_Data_Element_Type.UnsignedInteger1Byte;
                    break;
                case ElementType.Int8:
                    val = StackConsts.SDP_Data_Element_Type.SignedInteger1Byte;
                    break;
                case ElementType.UInt16:
                    val = StackConsts.SDP_Data_Element_Type.UnsignedInteger2Bytes;
                    break;
                case ElementType.Int16:
                    val = StackConsts.SDP_Data_Element_Type.SignedInteger2Bytes;
                    break;
                case ElementType.UInt32:
                    val = StackConsts.SDP_Data_Element_Type.UnsignedInteger4Bytes;
                    break;
                case ElementType.Int32:
                    val = StackConsts.SDP_Data_Element_Type.SignedInteger4Bytes;
                    break;
                case ElementType.UInt64:
                    val = StackConsts.SDP_Data_Element_Type.UnsignedInteger8Bytes;
                    break;
                case ElementType.Int64:
                    val = StackConsts.SDP_Data_Element_Type.SignedInteger8Bytes;
                    break;
                case ElementType.UInt128:
                    val = StackConsts.SDP_Data_Element_Type.UnsignedInteger16Bytes;
                    break;
                case ElementType.Int128:
                    val = StackConsts.SDP_Data_Element_Type.SignedInteger16Bytes;
                    break;
                //
                case ElementType.Uuid16:
                    val = StackConsts.SDP_Data_Element_Type.UUID_16;
                    break;
                case ElementType.Uuid32:
                    val = StackConsts.SDP_Data_Element_Type.UUID_32;
                    break;
                case ElementType.Uuid128:
                    val = StackConsts.SDP_Data_Element_Type.UUID_128;
                    break;
                //
                case ElementType.TextString:
                    val = StackConsts.SDP_Data_Element_Type.TextString;
                    break;
                case ElementType.Url:
                    val = StackConsts.SDP_Data_Element_Type.URL;
                    break;
                //
                case ElementType.ElementSequence:
                    val = StackConsts.SDP_Data_Element_Type.Sequence;
                    break;
                case ElementType.ElementAlternative:
                    val = StackConsts.SDP_Data_Element_Type.Alternative;
                    break;
                //
                case ElementType.Unknown:
                default:
                    throw new ArgumentException(
                        "Unknown ElementType: " + elementType.ToString() + ".", "elementType");
            }
            return val;
        }


        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~BluetopiaSdpCreator()
        {
            Dispose(false);
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "disposing")]
        void Dispose(bool disposing)
        {
            if (_hSR.HasValue) {
                DeleteServiceRecord();
            }
        }
        #endregion
    }
}
