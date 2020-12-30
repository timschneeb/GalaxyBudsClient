// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Widcomm.WidcommSocketExceptions
// 
// Copyright (c) 2009-2010 In The Hand Ltd, All rights reserved.
// Copyright (c) 2009-2010 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using InTheHand.Net.Bluetooth.AttributeIds;

namespace InTheHand.Net.Bluetooth.Widcomm
{
    class WidcommSdpServiceCreator : ServiceRecordCreator
    {
        ServiceRecord m_record;
        ISdpService m_sdpSvc;
        //
        bool aboveBase; // Ignore outer/bottom SEQ
        bool isAttrId, isInSeqOrAlt;
        //
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Used in DEBUG")]
        bool needWrite;
        ServiceAttributeId attrId;
        SdpService.DESC_TYPE dt;
        byte[] value;
        //
        static byte[] s_dummy = new byte[1024];

        //--------------------------
        public void CreateServiceRecord(ServiceRecord record, ISdpService sdpService)
        {
            if (m_record != null)
                throw new InvalidOperationException("One at a time please.");
            System.Threading.Interlocked.Exchange(ref m_record, record);
            try {
                m_sdpSvc = sdpService;
                int count = base.CreateServiceRecord(record, s_dummy);
            } finally {
                System.Threading.Interlocked.Exchange(ref m_record, null);
            }
        }

        //--------------------------
        static readonly ServiceAttributeId[] MustWellKnownWriteAttributes = {
            // AddAttribute():
            // "Note: The value ATTR_ID_SERVICE_CLASS_ID_LIST may not be used with 
            // this function. There is a special DK function, AddServiceClassIdList(), 
            // that must be used for that attribute.
            // New applications may define their own attribute identifiers, as long 
            // as the value is greater than 0x300 and does not conflict with the 
            // existing values in the table."
            UniversalAttributeId.ServiceClassIdList,
        };

        protected override void WriteAttribute(ServiceAttribute attr, byte[] buffer, ref int offset)
        {
            bool doneKnown = WriteWellKnownAttribute(attr, m_sdpSvc);
            if (doneKnown)
                return;
            if (Array.IndexOf(MustWellKnownWriteAttributes, attr.Id) != -1) {
                throw new NotImplementedException("MustWellKnownWriteAttributes: " + attr.Id.ToString("X") + ".");
            }
            //----
            base.WriteAttribute(attr, buffer, ref offset);
            //
            Debug.Assert(this.needWrite);
            this.needWrite = false;
            Debug.Assert(this.value != null, "What length to write with null array value?");
            m_sdpSvc.AddAttribute(
                unchecked((ushort)this.attrId), 
                this.dt,
                this.value != null ? this.value.Length : 0, 
                this.value);
        }

        //--------
        protected override int CreateAttrId(ServiceAttributeId attrId, byte[] buf, int offset)
        {
            Debug.Assert(!this.needWrite, "overwrite");
            this.attrId = attrId;
            this.isAttrId = true;
            int ret = base.CreateAttrId(attrId, buf, offset);
            this.isAttrId = false;
            return ret;
        }

        protected override void WriteFixedLength(ServiceElement element, byte[] valueBytes, byte[] buf, ref int offset, out int totalLength)
        {
            if (this.isAttrId) {
            } else if (this.isInSeqOrAlt) {
                // Child of a SEQ/ALT, will be written from byte buffer.
            } else {
                Debug.Assert(!this.needWrite, "overwrite");
                this.value = (byte[])valueBytes.Clone();
                this.dt = ToDESC_TYPE(element.ElementTypeDescriptor);
                this.needWrite = true;
            }
            //
            base.WriteFixedLength(element, valueBytes, buf, ref offset, out totalLength);
        }

        protected override void WriteVariableLength(ServiceElement element, byte[] valueBytes, byte[] buf, ref int offset, out int totalLength)
        {
            if (this.isAttrId) {
                Debug.Fail("Huh!  AttrId is always fixed length");
            } else if (this.isInSeqOrAlt) {
                // Child of a SEQ/ALT, will be written from byte buffer.
            } else {
                Debug.Assert(!this.needWrite, "overwrite");
                this.value = (byte[])valueBytes.Clone();
                this.dt = ToDESC_TYPE(element.ElementTypeDescriptor);
                this.needWrite = true;
            }
            //
            base.WriteVariableLength(element, valueBytes, buf, ref offset, out totalLength);
        }

        //----
        protected override int MakeVariableLengthHeader(byte[] buf, int offset, 
            ElementTypeDescriptor etd, out HeaderWriteState headerState)
        {
            bool seqOrAltToStore  = false;
            if (!this.aboveBase) {
                Debug.Assert(etd == ElementTypeDescriptor.ElementSequence,
                    "Outer element is a SEQ!  Or are we called elsewhere!?");
                this.aboveBase = true;
            } else if (this.isInSeqOrAlt) {
                // Child of a SEQ/ALT, will be written from byte buffer.
            } else if (etd == ElementTypeDescriptor.ElementSequence
                        || etd == ElementTypeDescriptor.ElementAlternative) {
                seqOrAltToStore = true;
            }
            //
            int ret = base.MakeVariableLengthHeader(buf, offset, etd, out headerState);
            //
            if(seqOrAltToStore){
                this.isInSeqOrAlt = true; // Ignore individual elements
                headerState.widcommNeedsStoring = true;
            }
            return ret;
        }

        protected override void CompleteHeaderWrite(HeaderWriteState headerState, 
            byte[] buf, int offsetAtEndOfWritten, out int totalLength)
        {
            base.CompleteHeaderWrite(headerState, buf, offsetAtEndOfWritten, out totalLength);
            //
            if (headerState.widcommNeedsStoring) {
                Debug.Assert(!this.needWrite, "overwrite");
                Debug.Assert(this.isInSeqOrAlt);
                this.isInSeqOrAlt = false;
                int startOffset = headerState.HeaderOffset + headerState.HeaderLength;
                byte[] val = new byte[offsetAtEndOfWritten - startOffset];
                Array.Copy(buf, startOffset, val, 0, val.Length);
                this.value = val;
                this.dt = ToDESC_TYPE(headerState.Etd);
                this.needWrite = true;
            }
        }

        //--------

        private static SdpService.DESC_TYPE ToDESC_TYPE(ElementTypeDescriptor elementTypeDescriptor)
        {
            // This is actually a one-to-one exact match, so no need for this map
            // but leave, as it allows us to block Nil for instance.
            SdpService.DESC_TYPE dt;
            switch (elementTypeDescriptor) {
                case ElementTypeDescriptor.UnsignedInteger:
                    dt = SdpService.DESC_TYPE.UINT;
                    break;
                case ElementTypeDescriptor.TwosComplementInteger:
                    dt = SdpService.DESC_TYPE.TWO_COMP_INT;
                    break;
                case ElementTypeDescriptor.Uuid:
                    dt = SdpService.DESC_TYPE.UUID;
                    break;
                case ElementTypeDescriptor.TextString:
                    dt = SdpService.DESC_TYPE.TEXT_STR;
                    break;
                case ElementTypeDescriptor.Boolean:
                    dt = SdpService.DESC_TYPE.BOOLEAN;
                    break;
                case ElementTypeDescriptor.ElementSequence:
                    dt = SdpService.DESC_TYPE.DATA_ELE_SEQ;
                    break;
                case ElementTypeDescriptor.ElementAlternative:
                    dt = SdpService.DESC_TYPE.DATA_ELE_ALT;
                    break;
                case ElementTypeDescriptor.Url:
                    dt = SdpService.DESC_TYPE.URL;
                    break;
                //
                case ElementTypeDescriptor.Unknown:
                case ElementTypeDescriptor.Nil:
                default:
                    throw new ArgumentException("ToDESC_TYPE(" + elementTypeDescriptor + ")");
            }
            return dt;
        }

        //--------
        bool WriteWellKnownAttribute(ServiceAttribute attr, ISdpService sdpService) //, ServiceRecord record)
        {
            switch (attr.Id) {
                case UniversalAttributeId.ServiceClassIdList:
                    sdpService.AddServiceClassIdList(ServiceRecordHelper_GetServiceClassIdList(attr));
                    return true;
                case UniversalAttributeId.ProtocolDescriptorList:
                    bool? isSimpleRfcomm;
                    ServiceElement el = ServiceRecordHelper.GetChannelElement(attr,
                        BluetoothProtocolDescriptorType.Rfcomm, out isSimpleRfcomm);
                    if (el == null) {
                        return false; // Non-RFCOMM
                    }
                    int scn = ServiceRecordHelper.GetRfcommChannelNumber(el);
                    const int NotFound = -1;
                    if (scn == NotFound) {
                        Debug.Fail("scn == -1 but non-RFCOMM case should be detected above!!?");
                        return false;
                    } else {
                        Debug.Assert(isSimpleRfcomm.HasValue, "isRfcommPlusMore.HasValue");
                        if (isSimpleRfcomm == true) {
                            sdpService.AddRFCommProtocolDescriptor(checked((byte)scn));
                        } else {
                            Debug.Assert(!isSimpleRfcomm.Value, "!isSimpleRfcomm");
                            // Need more layers (etc)!  Could call AddProtocolList() with 
                            // three (or more) tSDP_PROTOCOL_ELEM.  But just fall 
                            // back to raw dumping for now.
                            return false;
                        }
                    }
                    return true;
                default:
                    return false;
            }
        }

        private static IList<Guid> ServiceRecordHelper_GetServiceClassIdList(ServiceAttribute attr)
        {
            ServiceElement e0 = attr.Value;
            if (e0.ElementType != ElementType.ElementSequence)
                throw new ArgumentException("ServiceClassIdList needs ElementSequence at base.");
            IList<ServiceElement> list = e0.GetValueAsElementList();
            List<Guid> result = new List<Guid>();
            foreach (ServiceElement eCur in list) {
                if (eCur.ElementTypeDescriptor != ElementTypeDescriptor.Uuid)
                    throw new ArgumentException(
                        "ServiceClassIdList contains a " + eCur.ElementType + " element.");
                result.Add(eCur.GetValueAsUuid());
            }
            return result;
        }

    }//class

}
