// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.ServiceRecordCreator
// 
// Copyright (c) 2007 Andy Hume
// Copyright (c) 2007 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Text;
using System.Net;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace InTheHand.Net.Bluetooth
{

    /// <summary>
    /// Creates a Service Record byte array from the given 
    /// <see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/> object.
    /// </summary>
    public class ServiceRecordCreator
    {

        /// <overloads>
        /// Creates a Service Record byte array from the given 
        /// <see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/> object.
        /// </overloads>
        /// -
        /// <summary>
        /// Creates a Service Record byte array from the given 
        /// <see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/> object,
        /// into the specified byte array.
        /// </summary>
        /// -
        /// <remarks>
        /// See the other overload <see cref="M:InTheHand.Net.Bluetooth.ServiceRecordCreator.CreateServiceRecord(InTheHand.Net.Bluetooth.ServiceRecord)"/>
        /// </remarks>
        /// -
        /// <param name="record">An instance of <see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/>
        /// containing the record to be created.
        /// </param>
        /// <param name="buffer">An array of <see cref="T:System.Byte"/> for the record
        /// to be written to.
        /// </param>
        /// -
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The record bytes are longer that the supplied byte array buffer.
        /// </exception>
        /// -
        /// <returns>The length of the record in the array of <see cref="T:System.Byte"/>.
        /// </returns>
        public int CreateServiceRecord(ServiceRecord record, byte[] buffer)
        {
            if (record == null) { throw new ArgumentNullException("record"); }
            if (buffer == null) { throw new ArgumentNullException("buffer"); }
            //
            int len;
            HeaderWriteState headerState;
            int offset = 0;
            len = MakeVariableLengthHeader(buffer, offset, ElementTypeDescriptor.ElementSequence, out headerState);
            offset += len;
            foreach (ServiceAttribute attr in record) {
                WriteAttribute(attr, buffer, ref offset);
            }//for
            int tmp;
            CompleteHeaderWrite(headerState, buffer, offset, out tmp);
            System.Diagnostics.Debug.Assert(offset != 0);
            return offset;
        }

        /// <exclude/>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "2#")]
        protected virtual void WriteAttribute(ServiceAttribute attr, byte[] buffer, ref int offset)
        {
            int len;
            len = CreateAttrId(attr.Id, buffer, offset);
            offset += len;
            len = CreateElement(attr.Value, buffer, offset);
            offset += len;
        }

        /// <summary>
        /// Creates a Service Record byte array from the given 
        /// <see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/> object.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>The only oddity (as with parsing) is with the <c>TextString</c>
        /// type.  The <see cref="T:InTheHand.Net.Bluetooth.ServiceElement"/> can
        /// either hold the string already encoded to its array of bytes or an 
        /// <see cref="T:System.String"/>.  In the latter case we will always simply 
        /// encode the string to an array of bytes using encoding 
        /// <see cref="P:System.Text.Encoding.UTF8"/>.
        /// </para>
        /// <para>Currently any UUIDs in the record are written out in the form supplied,
        /// we should probably write a &#x2018;short-form&#x2019; equivalent if its
        /// a &#x2018;Bluetooth-based&#x2019; UUID e.g. <c>Uuid128</c> as <c>Uuid16</c>.
        /// </para>
        /// </remarks>
        /// -
        /// <param name="record">An instance of <see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/>
        /// containing the record to be created.
        /// </param>
        /// -
        /// <returns>An array of <see cref="T:System.Byte"/> containing the resultant
        /// record bytes.  The length of the array is the length of the record bytes.
        /// </returns>
        public byte[] CreateServiceRecord(ServiceRecord record)
        {
            const int MaxiOutputSize = 256;
            byte[] tmpResult = new byte[MaxiOutputSize];
            int length = CreateServiceRecord(record, tmpResult);
            byte[] result = new byte[length];
            Array.Copy(tmpResult, 0, result, 0, length);
            return result;
        }

        //--------

        private static void VerifyWriteSpaceRemaining(int requiredLength, byte[] buffer, int offset)
        {
            int spaceRemaining = buffer.Length - offset;
            if (requiredLength > spaceRemaining) {
                // I never know what exception to throw in a 'overrun' case.
                // The 'paramName' argument ("buffer") below is of the top-level method 'CreateServiceRecord'.
                throw ExceptionFactory.ArgumentOutOfRangeException(
                    "buffer",
                    "The record bytes are longer that the supplied byte array buffer.");
            }
        }

        /// <exclude/>
        protected virtual int CreateAttrId(ServiceAttributeId attrId, byte[] buf, int offset)
        {
            ServiceElement dummyElement
                = new ServiceElement(
                    ElementType.UInt16, unchecked((UInt16)attrId));
            return CreateElement(dummyElement, buf, offset);
        }

        /// <summary>
        /// Create the element in the buffer starting at offset, and return its totalLength.
        /// </summary>
        /// <param name="element">The element to create.
        /// </param>
        /// <param name="buf">The byte array to write the encoded element to.
        /// </param>
        /// <param name="offset">The place to start writing in <paramref name="buf"/>.
        /// </param>
        /// 
        /// <returns>The total length of the encoded element written to the buffer
        /// </returns>
        protected virtual int CreateElement(ServiceElement element, byte[] buf, int offset)
        {
            int totalLength;
            //
            if (element.ElementTypeDescriptor == ElementTypeDescriptor.ElementSequence
                    || element.ElementTypeDescriptor == ElementTypeDescriptor.ElementAlternative) {
                HeaderWriteState headerState;
                int curLen;
                curLen = MakeVariableLengthHeader(buf, offset, element.ElementTypeDescriptor, out headerState);
                offset += curLen;
                foreach (ServiceElement childElement in element.GetValueAsElementList()) {
                    curLen = CreateElement(childElement, buf, offset);
                    offset += curLen;
                }//for
                CompleteHeaderWrite(headerState, buf, offset, out totalLength);
                //----------------
            } else if (element.ElementTypeDescriptor == ElementTypeDescriptor.UnsignedInteger
                    || element.ElementTypeDescriptor == ElementTypeDescriptor.TwosComplementInteger) {
                switch (element.ElementType) {
                    case ElementType.UInt8:
                        WriteByte(element, (Byte)element.Value, buf, ref offset, out totalLength);
                        break;
                    case ElementType.Int8:
                        WriteSByte(element, (SByte)element.Value, buf, ref offset, out totalLength);
                        break;
                    case ElementType.UInt16:
                        WriteUInt16(element, (UInt16)element.Value, buf, ref offset, out totalLength);
                        break;
                    case ElementType.Int16:
                        WriteInt16(element, (Int16)element.Value, buf, ref offset, out totalLength);
                        break;
                    case ElementType.UInt32:
                        WriteUInt32(element, (UInt32)element.Value, buf, ref offset, out totalLength);
                        break;
                    case ElementType.Int32:
                        WriteInt32(element, (Int32)element.Value, buf, ref offset, out totalLength);
                        break;
                    case ElementType.UInt64:
                        WriteUInt64(element, (UInt64)element.Value, buf, ref offset, out totalLength);
                        break;
                    case ElementType.Int64:
                        WriteInt64(element, (Int64)element.Value, buf, ref offset, out totalLength);
                        break;
                    default:
                        System.Diagnostics.Debug.Fail(String.Format(System.Globalization.CultureInfo.InvariantCulture,
                                "Unexpected integral type '{0}'.", element.ElementType));
                        totalLength = 0;
                        break;
                }//switch
                //----------------
            } else if (element.ElementTypeDescriptor == ElementTypeDescriptor.Uuid) {
                if (element.ElementType == ElementType.Uuid16) {
                    WriteUInt16(element, (UInt16)element.Value, buf, ref offset, out totalLength);
                } else if (element.ElementType == ElementType.Uuid32) {
                    WriteUInt32(element, (UInt32)element.Value, buf, ref offset, out totalLength);
                } else {
                    //TODO If the 'Guid' holds a 'Bluetooth-based' UUID, then should we write the short form?
                    byte[] bytes;
                    System.Diagnostics.Debug.Assert(element.ElementType == ElementType.Uuid128);
                    Guid hostGuid = (Guid)element.Value;
                    Guid netGuid = Sockets.BluetoothListener.HostToNetworkOrder(hostGuid);
                    bytes = netGuid.ToByteArray();
                    WriteFixedLength(element, bytes, buf, ref offset, out totalLength);
                }
                //----------------
            } else if (element.ElementTypeDescriptor == ElementTypeDescriptor.Url) {
                Uri uri = element.GetValueAsUri();
                String uriString = uri.ToString();
                byte[] valueBytes = System.Text.Encoding.ASCII.GetBytes(uriString);
                WriteVariableLength(element, valueBytes, buf, ref offset, out totalLength);
                //----------------
            } else if (element.ElementTypeDescriptor == ElementTypeDescriptor.TextString) {
                byte[] valueBytes;
                String valueString = element.Value as String;
                if (valueString != null) {
                    valueBytes = System.Text.Encoding.UTF8.GetBytes(valueString);
                } else {
                    System.Diagnostics.Debug.Assert(element.Value is byte[]);
                    valueBytes = (byte[])element.Value;
                }
                WriteVariableLength(element, valueBytes, buf, ref offset, out totalLength);
                //----------------
            } else if (element.ElementTypeDescriptor == ElementTypeDescriptor.Nil) {
                WriteFixedLength(element, new byte[0], buf, ref offset, out totalLength);
                //----------------
            } else if (element.ElementTypeDescriptor == ElementTypeDescriptor.Boolean) {
                bool value = (bool)element.Value;
                byte[] valueBytes = new byte[1];
                valueBytes[0] = value ? (byte)1 : (byte)0;
                WriteFixedLength(element, valueBytes, buf, ref offset, out totalLength);
                //----------------
            } else {
                totalLength = 0;
            }
            //
            if (totalLength == 0) {
                throw new NotSupportedException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "Creation of element type '{0}' not implemented.", element.ElementType));
            }
            return totalLength;
        }

        /// <exclude/>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "3#")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "4#")]
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "bytes")]
        protected virtual void WriteVariableLength(ServiceElement element, byte[] valueBytes, byte[] buf, ref int offset, out int totalLength)
        {
            HeaderWriteState headerState;
            int curLen;
            curLen = MakeVariableLengthHeader(buf, offset, element.ElementTypeDescriptor, out headerState);
            offset += curLen;
            VerifyWriteSpaceRemaining(valueBytes.Length, buf, offset);
            valueBytes.CopyTo(buf, offset);//write
            offset += valueBytes.Length;
            CompleteHeaderWrite(headerState, buf, offset, out totalLength);
        }

        /// <exclude/>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "3#")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "4#")]
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "bytes")]
        protected virtual void WriteFixedLength(ServiceElement element, byte[] valueBytes, byte[] buf, ref int offset, out int totalLength)
        {
            int headerLen = WriteHeaderFixedLength(element.ElementTypeDescriptor, valueBytes.Length, buf, offset, out totalLength);
            offset += headerLen;
            VerifyWriteSpaceRemaining(valueBytes.Length, buf, offset);
            valueBytes.CopyTo(buf, offset);
            System.Diagnostics.Debug.Assert(totalLength == headerLen + valueBytes.Length);
        }

        //--------------------------------------------
        private void WriteUInt16(ServiceElement element, UInt16 value, byte[] buf, ref int offset, out int totalLength)
        {
            Int16 valueS = unchecked((Int16)value);
            WriteInt16(element, valueS, buf, ref  offset, out  totalLength);
        }
        private void WriteInt16(ServiceElement element, Int16 value, byte[] buf, ref int offset, out int totalLength)
        {
            Int16 host16 = value;
            Int16 net16 = IPAddress.HostToNetworkOrder(host16);
            byte[] valueBytes = BitConverter.GetBytes(net16);
            WriteFixedLength(element, valueBytes, buf, ref offset, out totalLength);
        }

        private void WriteByte(ServiceElement element, Byte value, byte[] buf, ref int offset, out int totalLength)
        {
            byte[] valueBytes = new byte[1];
            valueBytes[0] = value;
            WriteFixedLength(element, valueBytes, buf, ref offset, out totalLength);
        }
        private void WriteSByte(ServiceElement element, SByte value, byte[] buf, ref int offset, out int totalLength)
        {
            Byte valueU = unchecked((Byte)value);
            WriteByte(element, valueU, buf, ref  offset, out  totalLength);
        }


        private void WriteUInt32(ServiceElement element, UInt32 value, byte[] buf, ref int offset, out int totalLength)
        {
            Int32 valueS = unchecked((Int32)value);
            WriteInt32(element, valueS, buf, ref  offset, out  totalLength);
        }
        private void WriteInt32(ServiceElement element, Int32 value, byte[] buf, ref int offset, out int totalLength)
        {
            Int32 host32 = value;
            Int32 net32 = IPAddress.HostToNetworkOrder(host32);
            byte[] valueBytes = BitConverter.GetBytes(net32);
            WriteFixedLength(element, valueBytes, buf, ref offset, out totalLength);
        }

        private void WriteUInt64(ServiceElement element, UInt64 value, byte[] buf, ref int offset, out int totalLength)
        {
            Int64 valueS = unchecked((Int64)value);
            WriteInt64(element, valueS, buf, ref  offset, out  totalLength);
        }
        private void WriteInt64(ServiceElement element, Int64 value, byte[] buf, ref int offset, out int totalLength)
        {
            Int64 host64 = value;
            Int64 net64 = IPAddress.HostToNetworkOrder(host64);
            byte[] valueBytes = BitConverter.GetBytes(net64);
            WriteFixedLength(element, valueBytes, buf, ref offset, out totalLength);
        }

        //--------------------------------------------
        private static SizeIndex FixedLengthToSizeIndex(int contentLength)
        {
            if (contentLength == 0) {
                return SizeIndex.LengthOneByteOrNil;
            } else if (contentLength == 1) {
                return SizeIndex.LengthOneByteOrNil;
            } else if (contentLength == 2) {
                return SizeIndex.LengthTwoBytes;
            } else if (contentLength == 4) {
                return SizeIndex.LengthFourBytes;
            } else if (contentLength == 8) {
                return SizeIndex.LengthEightBytes;
            } else {
                System.Diagnostics.Debug.Assert(contentLength == 16,
                    "FixedLengthToSizeIndex--contentLength not a supported size.");
                return SizeIndex.LengthSixteenBytes;
            }
        }

        private int WriteHeaderFixedLength(ElementTypeDescriptor elementTypeDescriptor, int contentLength,
            byte[] buf, int offset,
            out int totalLength)
        {
            SizeIndex sizeIndex = FixedLengthToSizeIndex(contentLength);
            int len = WriteHeaderFixedLength_(elementTypeDescriptor, contentLength, sizeIndex, buf, offset, out totalLength);
            return len;
        }

        private int WriteHeaderFixedLength_(ElementTypeDescriptor elementTypeDescriptor, int contentLength, SizeIndex sizeIndex,
            byte[] buf, int offset,
            out int totalLength)
        {
            System.Diagnostics.Debug.Assert(
                sizeIndex == SizeIndex.LengthOneByteOrNil
                || sizeIndex == SizeIndex.LengthTwoBytes
                || sizeIndex == SizeIndex.LengthFourBytes
                || sizeIndex == SizeIndex.LengthEightBytes
                || sizeIndex == SizeIndex.LengthSixteenBytes);
            ServiceRecordParser.VerifyAllowedSizeIndex(elementTypeDescriptor, sizeIndex, false);
            HeaderWriteState headerState = new HeaderWriteState(elementTypeDescriptor, buf, offset, sizeIndex, 1);
            CompleteHeaderWrite(headerState, buf, offset + contentLength + headerState.HeaderLength, out totalLength);
            return headerState.HeaderLength;
        }

        /// <exclude/>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "3#")]
        protected virtual int MakeVariableLengthHeader(byte[] buf, int offset, ElementTypeDescriptor elementTypeDescriptor, out HeaderWriteState headerState)
        {
            HackFxCopHintNonStaticMethod();
            // We only support one-byte length fields (currently?).
            headerState = new HeaderWriteState(elementTypeDescriptor, buf, offset, SizeIndex.AdditionalUInt8, 2);
            return headerState.HeaderLength;
        }

        /// <exclude/>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "3#")]
        protected virtual void CompleteHeaderWrite(HeaderWriteState headerState, byte[] buf, int offsetAtEndOfWritten, out int totalLength)
        {
            HackFxCopHintNonStaticMethod();
            byte headerByte = CreateHeaderByte(headerState.Etd, headerState.SizeIndex);
            buf[headerState.HeaderOffset] = headerByte;
            //
            if (headerState.SizeIndex == SizeIndex.LengthOneByteOrNil
                    || headerState.SizeIndex == SizeIndex.LengthTwoBytes
                    || headerState.SizeIndex == SizeIndex.LengthFourBytes
                    || headerState.SizeIndex == SizeIndex.LengthEightBytes
                    || headerState.SizeIndex == SizeIndex.LengthSixteenBytes) {
                // Nothing to write-out here.
                totalLength = offsetAtEndOfWritten - headerState.HeaderOffset;
            } else {
                System.Diagnostics.Debug.Assert(
                    headerState.SizeIndex == SizeIndex.AdditionalUInt8
                    || headerState.SizeIndex == SizeIndex.AdditionalUInt16
                    || headerState.SizeIndex == SizeIndex.AdditionalUInt32);
                int contentLength = offsetAtEndOfWritten - headerState.HeaderOffset - headerState.HeaderLength;
                System.Diagnostics.Debug.Assert(headerState.SizeIndex == SizeIndex.AdditionalUInt8,
                    "WriteHeader not AdditionalUInt8 but that's all that MakeHeaderSpace supports...");
                if (headerState.SizeIndex == SizeIndex.AdditionalUInt8) {
                    if (contentLength > Byte.MaxValue) {
                        throw new NotSupportedException(ErrorMsgSupportOnlyLength255);
                    }
                    buf[headerState.HeaderOffset + 1] = checked((byte)contentLength);
                }
                totalLength = offsetAtEndOfWritten - headerState.HeaderOffset;
            }
        }

        private static byte CreateHeaderByte(ElementTypeDescriptor etd, SizeIndex sizeIndex)
        {
            System.Diagnostics.Debug.Assert((int)etd < 32);
            byte headerByte = (byte)((int)etd << ServiceRecordParser.ElementTypeDescriptorOffset);
            System.Diagnostics.Debug.Assert((int)sizeIndex < 8);
            headerByte |= (byte)sizeIndex;
            return headerByte;
        }

        /// <exclude/>
        protected sealed class HeaderWriteState
        {
            /// <exclude/>
            public readonly int HeaderOffset;
            /// <exclude/>
            public readonly ElementTypeDescriptor Etd;
            /// <exclude/>
            public readonly SizeIndex SizeIndex;
            /// <exclude/>
            public readonly int HeaderLength;
            //
            /// <exclude/>
            internal bool widcommNeedsStoring;

            internal HeaderWriteState(ElementTypeDescriptor elementTypeDescriptor, byte[] buf, int offset, SizeIndex sizeIndex, int headerLength)
            {
                this.Etd = elementTypeDescriptor;
                this.HeaderOffset = offset;
                this.SizeIndex = sizeIndex;
                this.HeaderLength = headerLength;
                //
                VerifyWriteSpaceRemaining(HeaderLength, buf, offset);
                ServiceRecordParser.VerifyAllowedSizeIndex(Etd, SizeIndex, false);
            }
        }//class


#if CODE_ANALYSIS
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
#endif
        private void HackFxCopHintNonStaticMethod()
        {
        }

        //--------
        /// <exclude/>
        public const string ErrorMsgSupportOnlyLength255
            = "Only ServiceRecords shorter that 256 bytes are supported currently.";

    }//class


    //public class ServiceRecordCreator : ServiceRecordCreatorBase
    //{
    //}

}
