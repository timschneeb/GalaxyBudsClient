using System;
#if V1
using System.Collections;
#else
using System.Collections.Generic;
#endif
using System.Diagnostics;
using System.Net;

namespace InTheHand.Net.Bluetooth
{

    /// <summary>
    /// Parses an array of bytes into the contained SDP 
    /// <see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/>.
    /// </summary>
    /// -
    /// <remarks>
    /// See the
    /// <see cref="M:InTheHand.Net.Bluetooth.ServiceRecordParser.Parse(System.Byte[],System.Int32,System.Int32)"/>
    /// methods for more information.
    /// </remarks>
    /// -
    /// <seealso cref="M:InTheHand.Net.Bluetooth.ServiceRecordParser.Parse(System.Byte[],System.Int32,System.Int32)"/>
    public class ServiceRecordParser
    {
        //--------------------------------------------------------------
        delegate
#if V1
            object SequenceItemParser
#else
 T SequenceItemParser<T>
#endif
(byte[] buffer, int offset, int length, out int readLength);


        //--------------------------------------------------------------
        private bool m_allowAnySizeForUnknownTypeDescriptorElements = true;

        //--------------------------------------------------------------
        // Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or set whether the parser will attempt to skip any unknown element
        /// type rather than producing an error.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>
        /// An element type is added instead with 
        /// <c>ElementType.</c><see cref="F:InTheHand.Net.Bluetooth.ElementType.Unknown"/> 
        /// and <c>ElementTypeDescriptor.</c><see cref="F:InTheHand.Net.Bluetooth.ElementTypeDescriptor.Unknown"/>.
        /// </para>
        /// </remarks>
        public bool SkipUnhandledElementTypes { get; set; }

        /// <summary>
        /// Gets or sets whether any URL elements will be converted to 
        /// <see cref="T:System.Uri"/> instances at parse time, or left as raw byte arrays.
        /// </summary>
        /// -
        /// <remarks><para>
        /// This is useful when the URL element is badly formatted and thus the
        /// parser will reject the record, setting this property to <c>true</c> will
        /// allow the parse to complete without attempting to decode the URL value.
        /// </para>
        /// <para>When <c>true</c> the value is stored as a array of bytes, when
        /// <c>false</c> it is stored as a <see cref="T:System.String"/>;
        /// however in earlier versions it was stored as <see cref="T:System.Uri"/>,
        /// and since there was often invalid content on devices (e.g. iPhone)
        /// this often failed.
        /// </para>
        /// </remarks>
        public bool LazyUrlCreation
        {
            get; set;
        }

        //TODO ((public AllowAnySizeForUnknownTypeDescriptorElements))      
        //    bool AllowAnySizeForUnknownTypeDescriptorElements
        //{
        //    [System.Diagnostics.DebuggerStepThrough]
        //    get { return m_allowAnySizeForUnknownTypeDescriptorElements; }
        //    [System.Diagnostics.DebuggerStepThrough]
        //    set { m_allowAnySizeForUnknownTypeDescriptorElements = value; }
        //}
	
        //--------------------------------------------------------------
        // Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Parses an array of bytes into its contained 
        /// <see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/>.
        /// </summary>
        /// -
        /// <remarks>
        /// See <see cref="M:InTheHand.Net.Bluetooth.ServiceRecordParser.Parse(System.Byte[],System.Int32,System.Int32)"/>
        /// for more information.
        /// </remarks>
        /// -
        /// <param name="buffer">A byte array containing the encoded Service Record.
        /// </param>
        /// <returns>The new <see cref="ServiceRecord"/> parsed from the byte array.
        /// </returns>
        /// -
        /// <seealso cref="M:InTheHand.Net.Bluetooth.ServiceRecordParser.Parse(System.Byte[],System.Int32,System.Int32)"/>
        /// <seealso cref="M:InTheHand.Net.Bluetooth.ServiceRecord.CreateServiceRecordFromBytes(System.Byte[])"/>
        public ServiceRecord Parse(byte[] buffer)
        {
            if (buffer == null) { throw new ArgumentNullException("buffer"); }
            return Parse(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Parses an array of bytes into its contained 
        /// <see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/>.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>If the record contains any element type not supported by the parser
        /// it will throw <see cref="T:System.NotImplementedException"/>. The
        /// only element types defined by SDP in v2.0 that are not currently implemented 
        /// are 64- and 128-bit integers.  Of course any types defined in a later 
        /// version will also throw this.  This behaviour can be changed with the
        /// <see cref="P:InTheHand.Net.Bluetooth.ServiceRecordParser.SkipUnhandledElementTypes"/> 
        /// property.
        /// </para>
        /// </remarks>
        /// -
        /// <param name="buffer">A byte array containing a Service Record.
        /// </param>
        /// <param name="offset">The position in the data buffer at which to
        /// begin parsing the Service Record.
        /// </param>
        /// <param name="length">The length of the Service Record in the byte array.
        /// </param>
        /// <returns>The Service Record parse from the byte array.
        /// </returns>
        /// -
        /// <exception cref="T:System.Net.ProtocolViolationException">
        /// The record contains invalid content.
        /// </exception>
        /// <exception cref="T:System.NotImplementedException">
        /// The record contains an element type not supported by the parser.
        /// </exception>
        /// -
        /// <seealso cref="M:InTheHand.Net.Bluetooth.ServiceRecordParser.Parse(System.Byte[])"/>
        /// <seealso cref="M:InTheHand.Net.Bluetooth.ServiceRecord.CreateServiceRecordFromBytes(System.Byte[])"/>
        public ServiceRecord Parse(byte[] buffer, int offset, int length)
        {
            if (buffer == null) { throw new ArgumentNullException("buffer"); }
            if (length <= 0) { throw new_ArgumentOutOfRangeException("buffer", ErrorMsgServiceRecordBytesZeroLength); }
            if (offset < 0) { throw new ArgumentOutOfRangeException("offset"); }
            if ((buffer.Length - offset) < length) {
                throw new ArgumentException("offset is past the end of the data.");
            }
            //
            ElementTypeDescriptor etd;
            System.Diagnostics.Debug.Assert(length >= 1); //is checked above!
            etd = GetElementTypeDescriptor(buffer[offset]);
            if (etd != ElementTypeDescriptor.ElementSequence) {
                throw CreateInvalidException(ErrorMsgTopElementNotSequence, offset);
            }
            //
            int contentLength;
            int contentOffset;
            int elementLength = GetElementLength(buffer, offset, length, out contentOffset, out contentLength);
            if (elementLength > length) {
                throw CreateInvalidExceptionOverruns(offset, elementLength, length);
            }
#if V1
            ArrayList children = new ArrayList();
            SequenceItemParser itemParser = new SequenceItemParser(ParseAttributeElementPairNETCFv1);
#else
            List<ServiceAttribute> children = new List<ServiceAttribute>();
            SequenceItemParser<ServiceAttribute> itemParser = ParseAttributeElementPair;
#endif
            ParseSeqOrAlt(buffer, offset, elementLength, children, itemParser);
            //if (elementLength < length) {
            //    Console.WriteLine("Data left in buffer: buf-length: {0}, elementLength: {1}", length, elementLength);
            //}
            //
            ServiceRecord result = new ServiceRecord(children);
            if (offset == 0) {
                result.SetSourceBytes(buffer);
                //TODO Set the source bytes on the record in the case where they're offset in the buffer.
            }
            return result;
        }

#if V1
        private object ParseAttributeElementPairNETCFv1(byte[] buffer, int offset, int length, out int readLength)
        {
            return ParseAttributeElementPair(buffer, offset, length, out readLength);
        }
#endif

        private ServiceAttribute ParseAttributeElementPair(byte[] buffer, int offset, int length, out int readLength)
        {
            // Verify that the first element of the pair is a UInt16 to hold the attr id.
            int contentOffset, contentLength;
            GetElementLength(buffer, offset, length, out contentOffset, out contentLength);
            ElementTypeDescriptor etd = GetElementTypeDescriptor(buffer[offset]);
            bool goodIdType = (etd == ElementTypeDescriptor.UnsignedInteger /*|| etd == ElementTypeDescriptor.TwosComplementInteger*/);
            goodIdType = goodIdType && (contentLength == 2);
            if (!goodIdType) {
                throw CreateInvalidException(ErrorMsgAttributePairFirstMustUint16, offset);

            }
            // Now we can read the pair -- the id and the following element.
            readLength = 0;
            Int32 elementLength;
            UInt16 attrId = ReadElementUInt16(buffer, offset, length, out elementLength);
            readLength += elementLength;
            offset += elementLength; length -= elementLength;
            ServiceElement value = ParseInternal(buffer, offset, length, out elementLength);
            readLength += elementLength;
            ServiceAttribute attr = new ServiceAttribute(attrId, value);
            return attr;
        }//fn

        //--------------------------------------------------------------

#if V1
        private object ParseInternalNETCFv1(byte[] buffer, int offset, int length, out int readLength)
        {
            return ParseInternal(buffer, offset, length, out readLength);
        }
#endif

        private ServiceElement ParseInternal(byte[] buffer, int offset, int length, out int readLength)
        {
            ElementTypeDescriptor etd;
            SizeIndex sizeIndex;
            SplitHeaderByte(buffer[offset], out etd, out sizeIndex);
            VerifyAllowedSizeIndex(etd, sizeIndex, m_allowAnySizeForUnknownTypeDescriptorElements);
            int contentLength;
            int contentOffset;
            readLength = GetElementLength(buffer, offset, length, out contentOffset, out contentLength);
            if (readLength > length) {
                throw CreateInvalidExceptionOverruns(offset, readLength, length);
            }
            object rawValue = null;
            // The code at the bottom of the method uses the value of 'type' to know
            // whether the element was handled and parsed.  So if we don't handle a 
            // particular type we'll drop through, and throw an error or create a 
            // 'unknown' element depending on the settings.
            ElementType type = ElementType.Unknown;
            // -- Parse --
            if (etd == ElementTypeDescriptor.Nil) { //----------------
                type = ElementType.Nil;
                rawValue = null;
            } else if (etd == ElementTypeDescriptor.UnsignedInteger
                        || etd == ElementTypeDescriptor.TwosComplementInteger) { //----------------
                if (contentLength == 1) {
                    Byte valueU8 = ReadElementUInt8(buffer, offset, length, out readLength);
                    if (etd == ElementTypeDescriptor.UnsignedInteger) {
                        type = ElementType.UInt8;
                        rawValue = valueU8;
                    } else {
                        type = ElementType.Int8;
                        rawValue = unchecked((SByte)valueU8);
                    }
                } else if (contentLength == 2) {
                    UInt16 valueU16 = ReadElementUInt16(buffer, offset, length, out readLength);
                    if (etd == ElementTypeDescriptor.UnsignedInteger) {
                        type = ElementType.UInt16;
                        rawValue = valueU16;
                    } else {
                        type = ElementType.Int16;
                        rawValue = unchecked((Int16)valueU16);
                    }
                } else if (contentLength == 4) {
                    UInt32 valueU32 = ReadElementUInt32(buffer, offset, length, out readLength);
                    if (etd == ElementTypeDescriptor.UnsignedInteger) {
                        type = ElementType.UInt32;
                        rawValue = valueU32;
                    } else {
                        type = ElementType.Int32;
                        rawValue = unchecked((Int32)valueU32);
                    }
                } else if (contentLength == 8) {
                    UInt64 valueU64 = ReadElementUInt64(buffer, offset, length, out readLength);
                    if (etd == ElementTypeDescriptor.UnsignedInteger) {
                        type = ElementType.UInt64;
                        rawValue = valueU64;
                    } else {
                        type = ElementType.Int64;
                        rawValue = unchecked((Int64)valueU64);
                    }
                } else if (contentLength == 16) {
                    byte[] valueArr128 = ReadArrayContent(buffer, offset, length, out readLength);
                    Debug.Assert(readLength == 17, "but readLength is : " + readLength);
                    Debug.Assert(valueArr128.Length == 16, "but valueArr128.Length is : " + valueArr128.Length);
                    if (etd == ElementTypeDescriptor.UnsignedInteger) {
                        type = ElementType.UInt128;
                        rawValue = valueArr128;
                    } else {
                        type = ElementType.Int128;
                        rawValue = valueArr128;
                    }
                }
            } else if (etd == ElementTypeDescriptor.Uuid) { //----------------
                if (contentLength == 2) {
                    UInt16 valueU16 = ReadFieldUInt16(buffer, offset + contentOffset, length - contentOffset);
                    type = ElementType.Uuid16;
                    rawValue = valueU16;
                } else if (contentLength == 4) {
                    UInt32 valueU32 = ReadFieldUInt32(buffer, offset + contentOffset, length - contentOffset);
                    type = ElementType.Uuid32;
                    rawValue = valueU32;
                } else if (contentLength == 16) {
                    offset += contentOffset;
                    Guid guidValue = new Guid(
                        // NETCF doesn't have #ctor(uint, ushort, ...) overload.
                        unchecked((Int32)ReadFieldUInt32(buffer, offset, contentLength)),//--
                        unchecked((Int16)ReadFieldUInt16(buffer, offset + 4, contentLength - 4)),//--
                        unchecked((Int16)ReadFieldUInt16(buffer, offset + 6, contentLength - 6)),//--
                        ReadFieldUInt8(buffer, offset + 8, contentLength - 8),
                        ReadFieldUInt8(buffer, offset + 9, contentLength - 9),//--
                        ReadFieldUInt8(buffer, offset + 10, contentLength - 10),
                        ReadFieldUInt8(buffer, offset + 11, contentLength - 11),
                        ReadFieldUInt8(buffer, offset + 12, contentLength - 12),
                        ReadFieldUInt8(buffer, offset + 13, contentLength - 13),
                        ReadFieldUInt8(buffer, offset + 14, contentLength - 14),
                        ReadFieldUInt8(buffer, offset + 15, contentLength - 15));
                    type = ElementType.Uuid128;
                    rawValue = guidValue;
                }
            } else if (etd == ElementTypeDescriptor.Boolean) { //----------------
                System.Diagnostics.Debug.Assert(contentLength == 1);
                byte valueU8 = ReadFieldUInt8(buffer, offset + contentOffset, length - contentOffset);
                bool valueBoolean = valueU8 != 0;
                rawValue = valueBoolean;
                type = ElementType.Boolean;
            } else if (etd == ElementTypeDescriptor.ElementSequence
                    || etd == ElementTypeDescriptor.ElementAlternative) {
#if V1
                ArrayList children = new ArrayList();
                SequenceItemParser itemParser = new SequenceItemParser(ParseInternalNETCFv1);
#else
                List<ServiceElement> children = new List<ServiceElement>();
                SequenceItemParser<ServiceElement> itemParser = ParseInternal;
#endif
                ParseSeqOrAlt(buffer, offset, readLength, children, itemParser);
                type = (etd == ElementTypeDescriptor.ElementSequence)
                    ? ElementType.ElementSequence : ElementType.ElementAlternative;
                rawValue = children;
            } else if (etd == ElementTypeDescriptor.Url) { //----------------
                int myReadLength;
                byte[] valueArray = ReadArrayContent(buffer, offset, length, out myReadLength);
                System.Diagnostics.Debug.Assert(myReadLength == readLength);
                if (LazyUrlCreation) {
                    rawValue = valueArray;
                } else {
                    string valueUri = CreateUriStringFromBytes(valueArray);
                    rawValue = valueUri;
                }
                type = ElementType.Url;
            } else if (etd == ElementTypeDescriptor.TextString) { //----------------
                int myReadLength;
                byte[] valueArray = ReadArrayContent(buffer, offset, length, out myReadLength);
                System.Diagnostics.Debug.Assert(myReadLength == readLength);
                type = ElementType.TextString;
                rawValue = valueArray;
            }
            // --------
            // Use parse result (or lack of!).
            if (type == ElementType.Unknown) {
                // If we haven't handled the type, we'll end-up here.
                string msg = "Element type: " + etd + ", SizeIndex: " + sizeIndex + ", at offset: " + offset + ".";
                if (SkipUnhandledElementTypes) {
                    type = ElementType.Unknown;
                    etd = ElementTypeDescriptor.Unknown;
                    rawValue = null;
                    Utils.MiscUtils.Trace_WriteLine(
                        "Unhandled SDP Parse " + msg);
                } else {
                    throw new_NotImplementedException(msg);
                }
            }
#if V1
    /*
#endif
#pragma warning disable 618
#if V1
    */
#endif
            ServiceElement value = new ServiceElement(etd, type, rawValue);
#if V1
    /*
#endif
#pragma warning restore 618
#if V1
    */
#endif
            return value;
        }

        private static void ParseSeqOrAlt
#if ! V1
            <T>
#endif
            (byte[] buffer, int offset, int length,
#if V1
                IList children, SequenceItemParser itemParser
#else
                IList<T> children, SequenceItemParser<T> itemParser
#endif
            )
        {
            Debug.Assert(!(itemParser == null), "NULL itemParser");
            Debug.Assert(!(children == null), "NULL children");
            System.Diagnostics.Debug.Assert(buffer.Length >= offset + length);
            //
            ElementTypeDescriptor etd = GetElementTypeDescriptor(buffer[offset]);
            Debug.Assert(!(etd != ElementTypeDescriptor.ElementSequence
                && etd != ElementTypeDescriptor.ElementAlternative), "Not Element Sequence or Alternative.");
            //
            Int32 elementLength;
            Int32 contentOffset;
            Int32 curContentLength;
            elementLength = GetElementLength(buffer, offset, length, out contentOffset, out curContentLength);
            Debug.Assert(!(elementLength != length), "Given length is not equal to the Seq/Alt length field.");
            //
            int curOffset = offset + contentOffset;
            while (curOffset < offset + length && curContentLength > 0) {
                //
                int readLength;
#if V1
                object
#else
                T
#endif
                    child = itemParser(buffer, curOffset, curContentLength, out readLength);
                children.Add(child);
                curOffset += readLength; curContentLength -= readLength;
            }//while
            // These can be wrong if the record is wrong but check them so we can 
            // see this during development.
            // Now, we throw if the record is invalid, so these aren't executed in
            // that case, and thus they *are* full invariants.
            System.Diagnostics.Debug.Assert(curOffset == offset + length);
            System.Diagnostics.Debug.Assert(curContentLength == 0);
        }

        //--------------------------------------------------------------

        /// <summary>
        /// Split a sequence of records into the component records.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>The Bluetooth SDP operation ServiceSearchAttribute returns its 
        /// result as a &#x201C;data element sequence where each element in turn is 
        /// a data element sequence representing an attribute list.&#x201D;  This
        /// method split that sequence into the individual attribute lists.
        /// </para>
        /// <para>On CE/Windows Mobile the result of a record lookup is in this form
        /// so <see cref="M:InTheHand.Net.Sockets.BluetoothDeviceInfo.GetServiceRecords(System.Guid)"/>
        /// etc use this method to split the result into is constituent records.
        /// </para>
        /// </remarks>
        /// -
        /// <param name="multiRecord">A byte array holding the 
        /// &#x201C;data element sequence where each element in turn is 
        /// a data element sequence representing an attribute list.&#x201D;
        /// </param>
        /// -
        /// <returns>An array of byte arrays where each holds a SDP record
        /// (a &#x201C;data element sequence representing an attribute list.&#x201D;).
        /// If the input was zero length or empty then a zero length array is returned.
        /// </returns>
        /// -
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="multiRecord"/> is <see langword="null"/>.
        /// </exception>
        public static byte[][] SplitSearchAttributeResult(byte[] multiRecord)
        {
            if (multiRecord == null) {
                throw new ArgumentNullException("multiRecord");
            }
            if (multiRecord.Length == 0) {
                // Should we error here?  We handle the zero element case below 
                // (where the input array is 0x35 0x00 etc).  So the caller likely 
                // made an error in passing a totally empty array...
                return new byte[0][];
            }
            //
            byte[] buffer = multiRecord;
            int offset = 0;
            int length = multiRecord.Length;
            //
            ElementTypeDescriptor etd;
            System.Diagnostics.Debug.Assert(length >= 1); //is checked above!
            etd = GetElementTypeDescriptor(buffer[offset]);
            if (etd != ElementTypeDescriptor.ElementSequence) {
                throw CreateInvalidException(ErrorMsgTopElementNotSequence, offset);
            }
            //
            int contentLength;
            int contentOffset;
            int elementLength = GetElementLength(buffer, offset, length, out contentOffset, out contentLength);
            if (elementLength > length) {
                throw CreateInvalidExceptionOverruns(offset, elementLength, length);
            }
            //
#if V1
            ArrayList list = new ArrayList();
#else
            List<byte[]> list = new List<byte[]>();
#endif
            //
            int curOffset = offset + contentOffset;
            int curLength = contentLength;
            while (curLength > 0) {
                ElementTypeDescriptor etdInner;
                System.Diagnostics.Debug.Assert(length >= 1); //is checked above!
                etdInner = GetElementTypeDescriptor(buffer[curOffset]);
                if (etdInner != ElementTypeDescriptor.ElementSequence) {
                    throw CreateInvalidException(ErrorMsgMultiSeqChildElementNotSequence, curOffset);
                }
                //
                int contentLengthInner;
                int contentOffsetInner;
                int elementLengthInner = GetElementLength(buffer, curOffset, curLength, out contentOffsetInner, out contentLengthInner);
                if (elementLengthInner > curLength) {
                    throw CreateInvalidExceptionOverruns(curOffset, elementLengthInner, curLength);
                }
                //
                int recordLength = contentOffsetInner + contentLengthInner;
                byte[] curRecord = new byte[recordLength];
                Array.Copy(buffer, curOffset, curRecord, 0, recordLength);
                list.Add(curRecord);
                //
                curOffset += recordLength;
                curLength -= recordLength;
            }//while
            System.Diagnostics.Debug.Assert(curLength == 0, "curLength == 0");
#if V1
            return (byte[][])list.ToArray(typeof(byte[]));
#else
            return list.ToArray();
#endif
        }

        //--------------------------------------------------------------

        private static byte ReadElementUInt8(byte[] buffer, int offset, int length, out int readLength)
        {
            int contentOffset;
            int contentLength;
            readLength = GetElementLength(buffer, offset, length, out contentOffset, out contentLength);
            ElementTypeDescriptor etd = GetElementTypeDescriptor(buffer[offset]);
            System.Diagnostics.Debug.Assert(etd == ElementTypeDescriptor.UnsignedInteger || etd == ElementTypeDescriptor.TwosComplementInteger);
            System.Diagnostics.Debug.Assert(contentLength == 1);
            Byte value = ReadFieldUInt8(buffer, offset + contentOffset, length - contentOffset);
            return value;
        }

        private static UInt16 ReadElementUInt16(byte[] buffer, int offset, int length, out int readLength)
        {
            int contentOffset;
            int contentLength;
            readLength = GetElementLength(buffer, offset, length, out contentOffset, out contentLength);
            ElementTypeDescriptor etd = GetElementTypeDescriptor(buffer[offset]);
            System.Diagnostics.Debug.Assert(etd == ElementTypeDescriptor.UnsignedInteger || etd == ElementTypeDescriptor.TwosComplementInteger);
            System.Diagnostics.Debug.Assert(contentLength == 2);
            UInt16 value = ReadFieldUInt16(buffer, offset + contentOffset, length - contentOffset);
            return value;
        }

        private static UInt32 ReadElementUInt32(byte[] buffer, int offset, int length, out int readLength)
        {
            int contentOffset;
            int contentLength;
            readLength = GetElementLength(buffer, offset, length, out contentOffset, out contentLength);
            ElementTypeDescriptor etd = GetElementTypeDescriptor(buffer[offset]);
            System.Diagnostics.Debug.Assert(etd == ElementTypeDescriptor.UnsignedInteger || etd == ElementTypeDescriptor.TwosComplementInteger);
            System.Diagnostics.Debug.Assert(contentLength == 4);
            UInt32 value = ReadFieldUInt32(buffer, offset + contentOffset, length - contentOffset);
            return value;
        }

        private static UInt64 ReadElementUInt64(byte[] buffer, int offset, int length, out int readLength)
        {
            int contentOffset;
            int contentLength;
            readLength = GetElementLength(buffer, offset, length, out contentOffset, out contentLength);
            ElementTypeDescriptor etd = GetElementTypeDescriptor(buffer[offset]);
            System.Diagnostics.Debug.Assert(etd == ElementTypeDescriptor.UnsignedInteger || etd == ElementTypeDescriptor.TwosComplementInteger);
            System.Diagnostics.Debug.Assert(contentLength == 8, "NOT contentLength == 8, is: " + contentLength);
            UInt64 value = ReadFieldUInt64(buffer, offset + contentOffset, length - contentOffset);
            return value;
        }

        private static byte[] ReadArrayContent(byte[] buffer, int offset, int length, out int readLength)
        {
            int contentOffset;
            int contentLength;
            readLength = GetElementLength(buffer, offset, length, out contentOffset, out contentLength);
            //
            byte[] result = new byte[contentLength];
            Array.Copy(buffer, offset + contentOffset, result, 0, contentLength);
            return result;
        }

        //--------
        /// <summary>
        /// For use when the content of the element is in an array
        /// i.e. the stack parses the element structure and returns the values in byte arrays.
        /// </summary>
        /// -
        /// <param name="networkOrderInteger">Whether the stack uses network order
        /// for UnsignedInteger and TwosComplementInteger elements (as used in the SDP format)
        /// or instead that the numerical values are in host order
        /// in the byte array.
        /// </param>
        /// <param name="networkOrderUuid">Whether the stack uses network order
        /// for Uuid elements (as used in the SDP format) 
        /// or instead that the numerical values are in host order
        /// in the byte array.
        /// </param>
        /// <param name="buffer">The byte array containing the SDP value.
        /// </param>
        /// <param name="offset">(?Always zero).
        /// </param>
        /// <param name="length">The length of the byte array.
        /// (Always equals <paramref name="contentLength"/>).
        /// </param>
        /// <param name="_readLength">
        /// </param>
        /// <param name="etd">The Element Type.
        /// </param>
        /// <param name="dbgSizeIndex">(Not used).
        /// </param>
        /// <param name="contentLength">The size of the value.
        /// </param>
        /// <param name="contentOffset">(?Always zero).
        /// </param>
        /// -
        /// <returns>
        /// </returns>
        internal ServiceElement ParseContent(bool networkOrderInteger, bool networkOrderUuid,
            byte[] buffer, int offset, int length, ref int _readLength,
            ElementTypeDescriptor etd, SizeIndex dbgSizeIndex,
            int contentLength, int contentOffset)
        {
            //TODO ParseContent ENDIAN!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            Debug.Assert(offset == 0, "NOT offset == 0, is: " + contentOffset);
            Debug.Assert(length == buffer.Length, "NOT length == buffer.Length...");
            Debug.Assert(contentOffset == 0, "NOT contentOffset == 0, is: " + contentOffset);
            Debug.Assert(contentLength == length, "DIFFERENT contentLength: " + contentLength + ", length: " + length);
            //--
            object rawValue = null;
            // The code at the bottom of the method uses the value of 'type' to know
            // whether the element was handled and parsed.  So if we don't handle a 
            // particular type we'll drop through, and throw an error or create a 
            // 'unknown' element depending on the settings.
            ElementType type = ElementType.Unknown;
            // -- Parse --
            if (etd == ElementTypeDescriptor.Nil) { //----------------
                type = ElementType.Nil;
                rawValue = null;
            } else if (etd == ElementTypeDescriptor.UnsignedInteger
                        || etd == ElementTypeDescriptor.TwosComplementInteger) { //----------------
                if (contentLength == 1) {
                    Byte valueU8 = ReadFieldUInt8_Content(networkOrderInteger, buffer, offset, length);
                    if (etd == ElementTypeDescriptor.UnsignedInteger) {
                        type = ElementType.UInt8;
                        rawValue = valueU8;
                    } else {
                        type = ElementType.Int8;
                        rawValue = unchecked((SByte)valueU8);
                    }
                } else if (contentLength == 2) {
                    UInt16 valueU16 = ReadFieldUInt16_Content(networkOrderInteger, buffer, offset, length);
                    if (etd == ElementTypeDescriptor.UnsignedInteger) {
                        type = ElementType.UInt16;
                        rawValue = valueU16;
                    } else {
                        type = ElementType.Int16;
                        rawValue = unchecked((Int16)valueU16);
                    }
                } else if (contentLength == 4) {
                    UInt32 valueU32 = ReadFieldUInt32_Content(networkOrderInteger, buffer, offset, length);
                    if (etd == ElementTypeDescriptor.UnsignedInteger) {
                        type = ElementType.UInt32;
                        rawValue = valueU32;
                    } else {
                        type = ElementType.Int32;
                        rawValue = unchecked((Int32)valueU32);
                    }
                } else if (contentLength == 8) {
                    UInt64 valueU64 = ReadFieldUInt64_Content(networkOrderInteger, buffer, offset, length);
                    if (etd == ElementTypeDescriptor.UnsignedInteger) {
                        type = ElementType.UInt64;
                        rawValue = valueU64;
                    } else {
                        type = ElementType.Int64;
                        rawValue = unchecked((Int64)valueU64);
                    }
                } else if (contentLength == 16) {
                    //int my_readLength;
                    byte[] valueArr128 = ReadArray_Content(buffer, offset, length);
                    if (!networkOrderInteger) {
                        Array.Reverse(valueArr128);
                    }
                    //Debug.Assert(my_readLength == 16, "but readLength is : " + my_readLength);
                    //Debug.Assert(valueArr128.Length == 16, "but valueArr128.Length is : " + valueArr128.Length);
                    if (etd == ElementTypeDescriptor.UnsignedInteger) {
                        type = ElementType.UInt128;
                        rawValue = valueArr128;
                    } else {
                        type = ElementType.Int128;
                        rawValue = valueArr128;
                    }
                }
            } else if (etd == ElementTypeDescriptor.Uuid) { //----------------
                if (contentLength == 2) {
                    UInt16 valueU16 = ReadFieldUInt16_Content(networkOrderUuid, buffer, offset + contentOffset, length - contentOffset);
                    type = ElementType.Uuid16;
                    rawValue = valueU16;
                } else if (contentLength == 4) {
                    UInt32 valueU32 = ReadFieldUInt32_Content(networkOrderUuid, buffer, offset + contentOffset, length - contentOffset);
                    type = ElementType.Uuid32;
                    rawValue = valueU32;
                } else if (contentLength == 16) {
                    offset += contentOffset;
                    Guid guidValue = new Guid(
                        // NETCF doesn't have #ctor(uint, ushort, ...) overload.
                        unchecked((Int32)ReadFieldUInt32(buffer, offset, contentLength)),//--
                        unchecked((Int16)ReadFieldUInt16(buffer, offset + 4, contentLength - 4)),//--
                        unchecked((Int16)ReadFieldUInt16(buffer, offset + 6, contentLength - 6)),//--
                        ReadFieldUInt8(buffer, offset + 8, contentLength - 8),
                        ReadFieldUInt8(buffer, offset + 9, contentLength - 9),//--
                        ReadFieldUInt8(buffer, offset + 10, contentLength - 10),
                        ReadFieldUInt8(buffer, offset + 11, contentLength - 11),
                        ReadFieldUInt8(buffer, offset + 12, contentLength - 12),
                        ReadFieldUInt8(buffer, offset + 13, contentLength - 13),
                        ReadFieldUInt8(buffer, offset + 14, contentLength - 14),
                        ReadFieldUInt8(buffer, offset + 15, contentLength - 15));
                    type = ElementType.Uuid128;
                    rawValue = guidValue;
                }
            } else if (etd == ElementTypeDescriptor.Boolean) { //----------------
                System.Diagnostics.Debug.Assert(contentLength == 1);
                byte valueU8 = ReadFieldUInt8(buffer, offset + contentOffset, length - contentOffset);
                bool valueBoolean = valueU8 != 0;
                rawValue = valueBoolean;
                type = ElementType.Boolean;
            } else if (etd == ElementTypeDescriptor.ElementSequence
                    || etd == ElementTypeDescriptor.ElementAlternative) {
                throw new InvalidOperationException("INTERNAL ERROR: Not supported here!!");
            } else if (etd == ElementTypeDescriptor.Url) { //----------------
                byte[] valueArray = ReadArray_Content(buffer, offset, length);
                if (LazyUrlCreation) {
                    rawValue = valueArray;
                } else {
                    string valueUri = CreateUriStringFromBytes(valueArray);
                    rawValue = valueUri;
                }
                type = ElementType.Url;
            } else if (etd == ElementTypeDescriptor.TextString) { //----------------
                byte[] valueArray = ReadArray_Content(buffer, offset, length);
                type = ElementType.TextString;
                rawValue = valueArray;
            }
            // --------
            // Use parse result (or lack of!).
            if (type == ElementType.Unknown) {
                // If we haven't handled the type, we'll end-up here.
                string msg = "Element type: " + etd + ", SizeIndex: " + dbgSizeIndex
                    + ", at contentLength: " + contentLength
                    + ", at offset: " + offset + ".";
                if (SkipUnhandledElementTypes) {
                    type = ElementType.Unknown;
                    etd = ElementTypeDescriptor.Unknown;
                    rawValue = null;
                    Utils.MiscUtils.Trace_WriteLine(
                        "Unhandled SDP Parse " + msg);
                } else {
                    throw new_NotImplementedException(msg);
                }
            }
#if V1
    /*
#endif
#pragma warning disable 618
#if V1
    */
#endif
            ServiceElement value = new ServiceElement(etd, type, rawValue);
#if V1
    /*
#endif
#pragma warning restore 618
#if V1
    */
#endif
            return value;
        }

        private static byte ReadFieldUInt8_Content(bool networkOrder, byte[] buffer, int offset, int length)
        {
            return ReadFieldUInt8(buffer, offset, length);
        }

        private static ushort ReadFieldUInt16_Content(bool networkOrder, byte[] buffer, int offset, int length)
        {
            return ReadFieldUInt16(networkOrder, buffer, offset, length);
        }

        private static uint ReadFieldUInt32_Content(bool networkOrder, byte[] buffer, int offset, int length)
        {
            return ReadFieldUInt32(networkOrder, buffer, offset, length);
        }

        private static ulong ReadFieldUInt64_Content(bool networkOrder, byte[] buffer, int offset, int length)
        {
            return ReadFieldUInt64(networkOrder, buffer, offset, length);
        }

        //TODO Make these swap to Host Order....???
        private static byte[] ReadArray_Content(byte[] buffer, int offset, int length)
        {
            int contentOffset = 0;
            int contentLength = length;
            byte[] result = new byte[contentLength];
            Array.Copy(buffer, offset + contentOffset, result, 0, contentLength);
            return result;
        }


        //--------------------------------------------------------------


        // The result length values always also covers the header bytes themselves.
        // This is different from how the protocol itself writes length values.
        /// <exclude/>
#if CODE_ANALYSIS
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "3#")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "4#")]
#endif
        protected static Int32 GetElementLength(byte[] buffer, int index, int length,
            out int contentOffset, out int contentLength)
        {
            // As we can be called by sub-types we need to do 'public' arg checking...
            if (buffer == null) { throw new ArgumentNullException("buffer"); }
            if (length <= 0) { throw new ArgumentOutOfRangeException("length"); }
            if (index < 0 || index == Int32.MaxValue) { // As suggested by FxCop
                throw new ArgumentOutOfRangeException("index");
            }
            if (length > buffer.Length - index) {
                throw new ArgumentException("length and index overruns buffer.");
            }
            //
            ElementTypeDescriptor etd;
            SizeIndex si;
            SplitHeaderByte(buffer[index], out etd, out si);
            if (etd == ElementTypeDescriptor.Nil) {
                if (si == SizeIndex.LengthOneByteOrNil) {
                    contentOffset = 1; //not valid for Nil, but return a length anyway
                    contentLength = 0;
                    return 1; //one byte length element
                }
                throw CreateInvalidException(ErrorMsgSizeIndexNotSuitTypeD, index);
            } else {
                switch (si) {
                    case SizeIndex.LengthOneByteOrNil:
                        contentOffset = 1;
                        return FixupLength(1, contentOffset, out contentLength, index);
                    case SizeIndex.LengthTwoBytes:
                        contentOffset = 1;
                        return FixupLength(2, contentOffset, out contentLength, index);
                    case SizeIndex.LengthFourBytes:
                        contentOffset = 1;
                        return FixupLength(4, contentOffset, out contentLength, index);
                    case SizeIndex.LengthEightBytes:
                        contentOffset = 1;
                        return FixupLength(8, contentOffset, out contentLength, index);
                    case SizeIndex.LengthSixteenBytes:
                        contentOffset = 1;
                        return FixupLength(16, contentOffset, out contentLength, index);
                    case SizeIndex.AdditionalUInt8:
                        CheckParseLength(index, length, 1 + 1);
                        contentOffset = 2;
                        Byte length8 = ReadFieldUInt8(buffer, index + 1, length - 1);
                        return FixupLength(length8, contentOffset, out contentLength, index);
                    case SizeIndex.AdditionalUInt16:
                        CheckParseLength(index, length, 1 + 2);
                        contentOffset = 3;
                        UInt16 length16 = ReadFieldUInt16(buffer, index + 1, length - 1);
                        return FixupLength(length16, contentOffset, out contentLength, index);
                    default:
                        System.Diagnostics.Debug.Assert(si == SizeIndex.AdditionalUInt32);
                        CheckParseLength(index, length, 1 + 4);
                        contentOffset = 5;
                        UInt32 length32 = ReadFieldUInt32(buffer, index + 1, length - 1);
                        return FixupLength(length32, contentOffset, out contentLength, index);
                }
            }
        }
        private static int FixupLength(UInt32 contentLength, int contentOffsetAlsoHeaderBytesLength, out int outContentLength, int index)
        {
            Int64 fullLength = contentLength + contentOffsetAlsoHeaderBytesLength;
            if (fullLength > Int32.MaxValue) {
                throw new System.Net.ProtocolViolationException(
                    String.Format(System.Globalization.CultureInfo.InvariantCulture,
                        ErrorMsgFormatNotSupportFull32bitSized, index));
            }
            int resultLength = checked((int)fullLength);
            outContentLength = resultLength - contentOffsetAlsoHeaderBytesLength;
            System.Diagnostics.Debug.Assert(contentLength == outContentLength);
            return resultLength;
        }

        //--------------------------------------------------------------
        private static void CheckParseLength(int index, int length, int requiredLength)
        {
            if (requiredLength > length) {
                throw CreateInvalidException(ErrorMsgFormatTruncated, index);
            }
        }

        private static Byte ReadFieldUInt8(byte[] bytes, int index, int length)
        {
            CheckParseLength(index, length, 1);
            Byte value8 = bytes[index];
            return value8;
        }
        private static UInt16 ReadFieldUInt16(byte[] bytes, int index, int length)
        {
            return ReadFieldUInt16(true, bytes, index, length);
        }
        private static UInt16 ReadFieldUInt16(bool networkOrder, byte[] bytes, int index, int length)
        {
            CheckParseLength(index, length, 2);
            short value16TypeToSuitHostToNetworkOrder = BitConverter.ToInt16(bytes, index);
            short value16Signed;
            if (networkOrder)
                value16Signed = System.Net.IPAddress.NetworkToHostOrder(value16TypeToSuitHostToNetworkOrder);
            else
                value16Signed = value16TypeToSuitHostToNetworkOrder;
            UInt16 value16 = unchecked((UInt16)value16Signed);
            return value16;
        }
        private static UInt32 ReadFieldUInt32(byte[] bytes, int index, int length)
        {
            return ReadFieldUInt32(true, bytes, index, length);
        }
        private static UInt32 ReadFieldUInt32(bool networkOrder, byte[] bytes, int index, int length)
        {
            CheckParseLength(index, length, 4);
            int value32TypeToSuitHostToNetworkOrder = BitConverter.ToInt32(bytes, index);
            int value32Signed;
            if (networkOrder)
                value32Signed = System.Net.IPAddress.NetworkToHostOrder(value32TypeToSuitHostToNetworkOrder);
            else
                value32Signed = value32TypeToSuitHostToNetworkOrder;
            UInt32 value32 = unchecked((UInt32)value32Signed);
            return value32;
        }
        private static UInt64 ReadFieldUInt64(byte[] bytes, int index, int length)
        {
            return ReadFieldUInt64(true, bytes, index, length);
        }
        private static UInt64 ReadFieldUInt64(bool networkOrder, byte[] bytes, int index, int length)
        {
            CheckParseLength(index, length, 8);
            Int64 value64TypeToSuitHostToNetworkOrder = BitConverter.ToInt64(bytes, index);
            Int64 value64Signed;
            if (networkOrder)
                value64Signed = System.Net.IPAddress.NetworkToHostOrder(value64TypeToSuitHostToNetworkOrder);
            else
                value64Signed = value64TypeToSuitHostToNetworkOrder;
            UInt64 value64 = unchecked((UInt64)value64Signed);
            return value64;
        }

        //--------------------------------------------------------------
        internal static string CreateUriStringFromBytes(byte[] valueArray)
        {
            System.Text.Encoding enc = System.Text.Encoding.ASCII;
            string str = enc.GetString(valueArray, 0, valueArray.Length);
            return str;
        }

        //--------------------------------------------------------------
        private static Exception CreateInvalidException(string formatMessage, int index)
        {
            return new System.Net.ProtocolViolationException(
                String.Format(System.Globalization.CultureInfo.InvariantCulture,
                    formatMessage, index));
        }

        private static Exception CreateInvalidExceptionOverruns(int index,
            int elementLength, int length)
        {
            Debug.Assert(elementLength > length);
#if !NETCF
            Trace.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture,
                    ErrorMsgElementOverrunsBuffer_WithLengths,
                    index, elementLength, length));
#endif
#if SDP_OVERRUN_HAS_LENGTHS
            // The lengths passed in are not always as we would expect, the checks
            // and calculations together detect all overruns but the calculations
            // at certain points are not as expected... :-,(
            string format = ErrorMsgElementOverrunsBuffer_WithLengths;
#else
            string format = ErrorMsgElementOverrunsBuffer;
#endif
            return new System.Net.ProtocolViolationException(
                String.Format(System.Globalization.CultureInfo.InvariantCulture,
                    format,
                    index, elementLength, length));
        }

        //--------------------------------------------------------------

        /// <summary>
        /// Split a header byte into its <see cref="T:InTheHand.Net.Bluetooth.ElementTypeDescriptor"/> and 
        /// <see cref="T:InTheHand.Net.Bluetooth.SizeIndex"/> parts.
        /// </summary>
        /// <remarks>
        /// The <see cref="T:InTheHand.Net.Bluetooth.ElementTypeDescriptor"/> returned is not checked to be a 
        /// known value.
        /// </remarks>
        /// -
        /// <param name="headerByte">The byte from the header.
        /// </param>
        /// <param name="etd">The <see cref="T:InTheHand.Net.Bluetooth.ElementTypeDescriptor"/>
        /// value from the header byte.
        /// </param>
        /// <param name="sizeIndex">The <see cref="T:InTheHand.Net.Bluetooth.SizeIndex"/>
        /// value from a header byte.
        /// </param>
        /// -
        /// <seealso cref="M:InTheHand.Net.Bluetooth.ServiceRecordParser.GetElementTypeDescriptor(System.Byte)"/>
        /// <seealso cref="M:InTheHand.Net.Bluetooth.ServiceRecordParser.GetSizeIndex(System.Byte)"/>
#if CODE_ANALYSIS
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:AvoidTypeNamesInParameters", MessageId = "0#")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]
#endif
        public static void SplitHeaderByte(byte headerByte, out ElementTypeDescriptor etd, out SizeIndex sizeIndex)
        {
            etd = GetElementTypeDescriptor(headerByte);
            sizeIndex = GetSizeIndex(headerByte);
        }

        /// <summary>
        /// Extract the <see cref="T:InTheHand.Net.Bluetooth.ElementTypeDescriptor"/> value from a header byte.
        /// </summary>
        /// <remarks>
        /// The <see cref="T:InTheHand.Net.Bluetooth.ElementTypeDescriptor"/> returned is not checked to be a 
        /// known value.
        /// </remarks>
        /// -
        /// <param name="headerByte">The byte from the header.
        /// </param>
        /// -
        /// <returns>The value as a <see cref="T:InTheHand.Net.Bluetooth.ElementTypeDescriptor"/>.</returns>
        /// -
        /// <seealso cref="M:InTheHand.Net.Bluetooth.ServiceRecordParser.SplitHeaderByte(System.Byte,InTheHand.Net.Bluetooth.ElementTypeDescriptor@,InTheHand.Net.Bluetooth.SizeIndex@)"/>
        /// <seealso cref="M:InTheHand.Net.Bluetooth.ServiceRecordParser.GetSizeIndex(System.Byte)"/>
        [System.Diagnostics.DebuggerStepThroughAttribute]
#if CODE_ANALYSIS
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:AvoidTypeNamesInParameters", MessageId = "0#")]
#endif
        public static ElementTypeDescriptor GetElementTypeDescriptor(byte headerByte)
        {
            UInt32 etdRaw = (UInt32)headerByte >> ElementTypeDescriptorOffset;
            ElementTypeDescriptor etd = (ElementTypeDescriptor)etdRaw;
            return etd;
        }

        /// <summary>
        /// Extract the <see cref="T:InTheHand.Net.Bluetooth.SizeIndex"/> field from a header byte.
        /// </summary>
        /// -
        /// <param name="headerByte">The byte from the header.
        /// </param>
        /// -
        /// <returns>The value as a <see cref="T:InTheHand.Net.Bluetooth.SizeIndex"/>.</returns>
        /// -
        /// <seealso cref="M:InTheHand.Net.Bluetooth.ServiceRecordParser.SplitHeaderByte(System.Byte,InTheHand.Net.Bluetooth.ElementTypeDescriptor@,InTheHand.Net.Bluetooth.SizeIndex@)"/>
        /// <seealso cref="M:InTheHand.Net.Bluetooth.ServiceRecordParser.GetElementTypeDescriptor(System.Byte)"/>
        [System.Diagnostics.DebuggerStepThroughAttribute]
#if CODE_ANALYSIS
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:AvoidTypeNamesInParameters", MessageId = "0#")]
#endif
        public static SizeIndex GetSizeIndex(byte headerByte)
        {
            int siRaw = headerByte & SizeIndexMask;
            SizeIndex sizeIndex = (SizeIndex)siRaw;
            return sizeIndex;
        }

        /// <summary>
        /// Bit offset of the ElementTypeDescriptor field in a header byte.
        /// </summary>
        /// <remarks>
        /// The header byte has two parts: five bits of ElementTypeDescriptor and
        /// three bits of Size Index.
        /// </remarks>
        /// <seealso cref="F:InTheHand.Net.Bluetooth.ServiceRecordParser.SizeIndexMask"/>
        internal const int ElementTypeDescriptorOffset = 3;

        /// <summary>
        /// Mask for the SizeIndex field in a header byte.
        /// </summary>
        /// <remarks>
        /// The header byte has two parts: five bits of ElementTypeDescriptor and
        /// three bits of Size Index, upper and lower respectively.
        /// </remarks>
        /// <seealso cref="F:InTheHand.Net.Bluetooth.ServiceRecordParser.ElementTypeDescriptorOffset"/>
        internal const int SizeIndexMask = 0x07;

        //--------------------------------------------------------------
        internal static void VerifyTypeMatchesEtd(ElementTypeDescriptor etd, ElementType type)
        {
            if (!TypeMatchesEtd(etd, type)) {
                throw new ProtocolViolationException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
                    ErrorMsgFormatTypeNotTypeDSubtype, etd, type));
            }
        }

        // is used by tests...
        /// <exclude/>
        public static bool TypeMatchesEtd(ElementTypeDescriptor etd, ElementType type)
        {
            bool matches = false;
            if (etd == ElementTypeDescriptor.Unknown) {
                matches = (type == ElementType.Unknown);
            } else
            if (etd == ElementTypeDescriptor.Nil) {
                matches = (type == ElementType.Nil);
            } else if (etd == ElementTypeDescriptor.UnsignedInteger) {
                matches = (type == ElementType.UInt8)
                    || (type == ElementType.UInt16)
                    || (type == ElementType.UInt32)
                    || (type == ElementType.UInt64)
                    || (type == ElementType.UInt128);
            } else if (etd == ElementTypeDescriptor.TwosComplementInteger) {
                matches = (type == ElementType.Int8)
                    || (type == ElementType.Int16)
                    || (type == ElementType.Int32)
                    || (type == ElementType.Int64)
                    || (type == ElementType.Int128);
            } else if (etd == ElementTypeDescriptor.Uuid) {
                matches = (type == ElementType.Uuid16)
                    || (type == ElementType.Uuid32)
                    || (type == ElementType.Uuid128);
            } else if (etd == ElementTypeDescriptor.TextString) {
                matches = (type == ElementType.TextString);
            } else if (etd == ElementTypeDescriptor.Boolean) {
                matches = (type == ElementType.Boolean);
            } else if (etd == ElementTypeDescriptor.ElementSequence) {
                matches = (type == ElementType.ElementSequence);
            } else if (etd == ElementTypeDescriptor.ElementAlternative) {
                matches = (type == ElementType.ElementAlternative);
            } else if (etd == ElementTypeDescriptor.Url) {
                matches = (type == ElementType.Url);
            }
            return matches;
        }

        /// <exclude/>
        public static ElementTypeDescriptor GetEtdForType(ElementType type)
        {
            ElementTypeDescriptor etd;
            //if (type == ElementType.Unknown) {
            //    etd = ElementTypeDescriptor.Unknown;}else
            if (type == ElementType.Nil) {
                etd = ElementTypeDescriptor.Nil;
            } else if ((type == ElementType.UInt8)
                        || (type == ElementType.UInt16)
                        || (type == ElementType.UInt64)
                        || (type == ElementType.UInt32)) {
                etd = ElementTypeDescriptor.UnsignedInteger;
            } else if ((type == ElementType.Int8)
                        || (type == ElementType.Int16)
                        || (type == ElementType.Int64)
                        || (type == ElementType.Int32)) {
                etd = ElementTypeDescriptor.TwosComplementInteger;
            } else if ((type == ElementType.Uuid16)
                         || (type == ElementType.Uuid32)
                         || (type == ElementType.Uuid128)) {
                etd = ElementTypeDescriptor.Uuid;
            } else if ((type == ElementType.TextString)) {
                etd = ElementTypeDescriptor.TextString;
            } else if ((type == ElementType.Boolean)) {
                etd = ElementTypeDescriptor.Boolean;
            } else if ((type == ElementType.ElementSequence)) {
                etd = ElementTypeDescriptor.ElementSequence;
            } else if ((type == ElementType.ElementAlternative)) {
                etd = ElementTypeDescriptor.ElementAlternative;
            } else if ((type == ElementType.Url)) {
                etd = ElementTypeDescriptor.Url;
            } else {
                throw new_ArgumentOutOfRangeException("type", 
                    String.Format(System.Globalization.CultureInfo.InvariantCulture,
                        ErrorMsgFormatUnknownType, type));
            }
#if DEBUG
            VerifyTypeMatchesEtd(etd, type);
#endif
            return etd;
        }
        
        //--------------------------------------------------------------

        internal static void VerifyAllowedSizeIndex(ElementTypeDescriptor etd, SizeIndex sizeIndex, bool allowAnySizeIndexForUnknownTypeDescriptorElements)
        {
            if (!IsAllowedSizeIndex(etd, sizeIndex, allowAnySizeIndexForUnknownTypeDescriptorElements)) {
                throw new ProtocolViolationException(ErrorMsgSizeIndexNotSuitTypeD);
            }
        }

        private static bool IsAllowedSizeIndex(ElementTypeDescriptor etd, SizeIndex sizeIndex, bool allowAnySizeIndexForUnknownTypeDescriptorElements)
        {
            bool isValidSi = false;
            if (etd == ElementTypeDescriptor.Nil
                || etd == ElementTypeDescriptor.Boolean) {
                isValidSi = (sizeIndex == SizeIndex.LengthOneByteOrNil);
            } else if (etd == ElementTypeDescriptor.UnsignedInteger
                    || etd == ElementTypeDescriptor.TwosComplementInteger) {
                isValidSi = (sizeIndex == SizeIndex.LengthOneByteOrNil)
                            || (sizeIndex == SizeIndex.LengthTwoBytes)
                            || (sizeIndex == SizeIndex.LengthFourBytes)
                            || (sizeIndex == SizeIndex.LengthEightBytes)
                            || (sizeIndex == SizeIndex.LengthSixteenBytes);
            } else if (etd == ElementTypeDescriptor.Uuid) {
                isValidSi = (sizeIndex == SizeIndex.LengthTwoBytes)
                        || (sizeIndex == SizeIndex.LengthFourBytes)
                        || (sizeIndex == SizeIndex.LengthSixteenBytes);
            } else if (etd == ElementTypeDescriptor.TextString
                    || etd == ElementTypeDescriptor.ElementSequence
                    || etd == ElementTypeDescriptor.ElementAlternative
                    || etd == ElementTypeDescriptor.Url) {
                isValidSi = (sizeIndex == SizeIndex.AdditionalUInt8)
                        || (sizeIndex == SizeIndex.AdditionalUInt16)
                        || (sizeIndex == SizeIndex.AdditionalUInt32);
            } else {
                isValidSi = allowAnySizeIndexForUnknownTypeDescriptorElements;
            }
            return isValidSi;
        }//fn

        //--------------------------------------------------------------
        internal static Exception new_NotImplementedException(string message)
        {
#if (PocketPC && V1)
            return new NotSupportedException(message);
#else
            return new NotImplementedException(message);
#endif
        }

        internal static Exception new_ArgumentOutOfRangeException(string paramName, string message)
        {
#if (PocketPC && V1)
            return new ArgumentOutOfRangeException(message);
#else
            return new ArgumentOutOfRangeException(paramName, message);
#endif
        }

        internal static Exception new_ArgumentOutOfRangeException(string message, Exception innerException)
        {
#if NETCF
            return new ArgumentOutOfRangeException(message);
#else
            return new ArgumentOutOfRangeException(message, innerException);
#endif
        }

        //--------------------------------------------------------------
        /// <exclude/>
        public const string ErrorMsgFormatUnknownType
            = "Unknown ElementType '{0}'.";
        /// <exclude/>
        public const string ErrorMsgFormatTypeNotTypeDSubtype
            = "ElementType '{1}' is not of given TypeDescriptor '{0}'.";
        /// <exclude/>
        public const String ErrorMsgSizeIndexNotSuitTypeD
            = "SizeIndex is not value for TypeDescriptor.";
        /// <exclude/>
        public const string ErrorMsgServiceRecordBytesZeroLength
            = "ServiceRecord byte array must be at least one byte long.";
        //
        /// <exclude/>
        public const string ErrorMsgFormatInvalidHeaderBytes
            = "Invalid header bytes at index {0}.";
        /// <exclude/>
        public const string ErrorMsgFormatTruncated
            = "Header truncated from index {0}.";
        /// <exclude/>
        public const string ErrorMsgFormatNotSupportFull32bitSized
            = "No support for full sized 32bit length values (index {0}).";
        /// <exclude/>
        public const string ErrorMsgTypeNotAsExpected
            = "Element Type not as expected.";
        //
        /// <exclude/>
        public const string ErrorMsgTopElementNotSequence
            = "The top element must be a Element Sequence type.";
        /// <exclude/>
        public const string ErrorMsgMultiSeqChildElementNotSequence
            = "In a multi-record sequence each element must be an Element Sequence.";
        //
        /// <exclude/>
        public const string ErrorMsgSequenceOverruns
            = "Element Sequence overruns the data, from index {0}.";
        /// <exclude/>
        public const string ErrorMsgElementOverrunsBuffer_WithLengths
            = "Element overruns buffer section, from index {0}"
            + ", item length is {1} but remaining length is only {2}"
            + ".";
        /// <exclude/>
        public const string ErrorMsgElementOverrunsBuffer
            = "Element overruns buffer section, from index {0}.";
        /// <exclude/>
        public const string ErrorMsgElementOverrunsBufferPrefix
            = "Element overruns buffer section, from index ";
        /// <exclude/>
        public const string ErrorMsgAttributePairFirstMustUint16
            = "The Attribute Id at index {0} is not of type Uint16.";

    }//class


}
