using System;
using System.Diagnostics;

namespace InTheHand.Net.Bluetooth
{

    /// <summary>
    /// Utilities method working on SDP <see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/>s, for instance to
    /// produce a 'dump' of the record's contents.
    /// </summary>
    /// -
    /// <remarks>
    /// This class produces output like the following:
    /// <code lang="none">
    /// AttrId: 0x0000 -- ServiceRecordHandle
    /// UInt32: 0x0
    /// 
    /// AttrId: 0x0001 -- ServiceClassIdList
    /// ElementSequence
    ///     Uuid16: 0x1000 -- ServiceDiscoveryServer
    /// 
    /// AttrId: 0x0004 -- ProtocolDescriptorList
    /// ElementSequence
    ///     ElementSequence
    ///         Uuid16: 0x100 -- L2CapProtocol
    ///         UInt16: 0x1
    ///     ElementSequence
    ///         Uuid16: 0x1 -- SdpProtocol
    /// ( ( L2Cap, PSM=Sdp ), ( Sdp ) )
    /// 
    /// AttrId: 0x0005 -- BrowseGroupList
    /// ElementSequence
    ///     Uuid16: 0x1002 -- PublicBrowseGroup
    /// 
    /// AttrId: 0x0006 -- LanguageBaseAttributeIdList
    /// ElementSequence
    ///     UInt16: 0x656E
    ///     UInt16: 0x6A
    ///     UInt16: 0x100
    /// 
    /// AttrId: 0x0100 -- ServiceName
    /// TextString: [en] 'Service Discovery'
    /// 
    /// AttrId: 0x0101 -- ServiceDescription
    /// TextString: [en] 'Publishes services to remote devices'
    /// 
    /// AttrId: 0x0102 -- ProviderName
    /// TextString: [en] 'Microsoft'
    /// 
    /// AttrId: 0x0200 -- VersionNumberList
    /// ElementSequence
    ///     UInt16: 0x100
    /// 
    /// AttrId: 0x0201 -- ServiceDatabaseState
    /// UInt32: 0x1
    /// </code>
    /// The Service Class Id names and Attribute Id names are looked up using 
    /// <see cref="M:InTheHand.Net.Bluetooth.BluetoothService.GetName(System.Guid)"/>/etc and
    /// <see cref="T:InTheHand.Net.Bluetooth.MapServiceClassToAttributeIdList"/>
    /// respectively.
    /// </remarks>
    public
#if ! V1
 static
#endif
 class ServiceRecordUtilities
    {
#if V1
        private ServiceRecordUtilities() { }
#endif

        //these are defined in BluetoothProtocolType

        //HACK HackProtocolServiceMultiplexer
        internal enum HackProtocolServiceMultiplexer : short
        {
            Sdp = 1,
            Rfcomm = 3,
            // ...
            Bnep = 0x0F,
            HidControl = 0x11,
            HidInterrupt = 0x13,
        }

        //HACK HackProtocolId
        internal enum HackProtocolId : short
        {
            Sdp = 1,
            Rfcomm = 3,
            Obex = 8,
            Bnep = 0x0F,
            Hidp = 0x11,
            L2Cap = 0x0100
        }

        /*
            AttrId: 0x0000
            UInt32: 0x0

            AttrId: 0x0001
            ElementSequence
                Uuid16: 0x1000

            AttrId: 0x0004
            ElementSequence
                ElementSequence
                    Uuid16: 0x100
                    UInt16: 0x1
                ElementSequence
                    Uuid16: 0x1

            AttrId: 0x0005
            ElementSequence
                Uuid16: 0x1002

            AttrId: 0x0006
            ElementSequence
                UInt16: 0x656E
                UInt16: 0x6A
                UInt16: 0x100

            AttrId: 0x0100
            _____ToBeDefined: 0x

            AttrId: 0x0101
            _____ToBeDefined: 0x

            AttrId: 0x0102
            _____ToBeDefined: 0x

            AttrId: 0x0200
            ElementSequence
                UInt16: 0x100

            AttrId: 0x0201
            UInt32: 0x1
        */

        /// <overloads>
        /// Produces a raw 'dump' of the given record, not including attribute names etc.
        /// </overloads>
        /// -
        /// <summary>
        /// Gets a string containing a raw 'dump' of the given record, not including attribute names etc.
        /// </summary>
        /// -
        /// <param name="record">A <see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/> to be dumped.</param>
        /// <returns>A <see cref="T:System.String"/> containing the 'dump' text.</returns>
        /// <seealso cref="M:InTheHand.Net.Bluetooth.ServiceRecordUtilities.DumpRaw(System.IO.TextWriter,InTheHand.Net.Bluetooth.ServiceRecord)"/>
        public static String DumpRaw(ServiceRecord record)
        {
            using (System.IO.StringWriter writer = new System.IO.StringWriter(
                            System.Globalization.CultureInfo.InvariantCulture)) {
                DumpRaw(writer, record);
                writer.Close(); // close here, and to be sure to be sure also with using()
                String result = writer.ToString();
                return result;
            }
        }

        /// <summary>
        /// Produce a raw 'dump' of the given record, not including attribute names etc, to the given
        /// <see cref="T:System.IO.TextWriter"/>.
        /// </summary>
        /// <param name="writer">A <see cref="T:System.IO.TextWriter"/> where the 'dump'
        /// text is to be written.</param>
        /// <param name="record">A <see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/> to be dumped.</param>
        /// <seealso cref="M:InTheHand.Net.Bluetooth.ServiceRecordUtilities.DumpRaw(InTheHand.Net.Bluetooth.ServiceRecord)"/>
        public static void DumpRaw(System.IO.TextWriter writer, ServiceRecord record)
        {
            if (writer == null) { throw new ArgumentNullException("writer"); }
            if (record == null) { throw new ArgumentNullException("record"); }
            //
            bool firstRecord = true;
            foreach (ServiceAttribute attr in record) {
                if (!firstRecord) {
                    writer.WriteLine();
                }
                // All this casting of enum values to int/underlying type is for the
                // benefit of NETCFv1, where Enum has no support for hex formatting.
                writer.WriteLine("AttrId: 0x{0:X4}", unchecked((ushort)attr.Id));
                DumpRawElement(writer, 0, attr.Value);
                firstRecord = false;
            }//for
        }

        private static void DumpRawElement(System.IO.TextWriter writer, int depth, ServiceElement elem)
        {
            WritePrefix(writer, depth);
            if (elem.ElementType == ElementType.ElementSequence
                    || elem.ElementType == ElementType.ElementAlternative) {
                writer.WriteLine("{0}", elem.ElementType);
                foreach (ServiceElement element in elem.GetValueAsElementList()) {
                    DumpRawElement(writer, depth + 1, element);
                }
            } else if (elem.ElementType == ElementType.Nil) {
                writer.WriteLine("Nil:");
            } else if (elem.ElementType == ElementType.TextString
                 || elem.ElementType == ElementType.Boolean
                 || elem.ElementType == ElementType.Url) {
                // Non-numeric types
                writer.WriteLine("{0}: {1}", elem.ElementType, elem.Value);
            } else if (elem.ElementType == ElementType.Uuid128) {
                writer.WriteLine("{0}: {1}", elem.ElementType, elem.Value);
            } else if (elem.ElementType == ElementType.UInt128
                                || elem.ElementType == ElementType.Int128) {
                string valueText = BitConverter.ToString((byte[])elem.Value);
                writer.WriteLine("{0}: {1}", elem.ElementType, valueText);
            } else {
                writer.WriteLine("{0}: 0x{1:X}", elem.ElementType, elem.Value);
                //{catch(?FOrmatExptn){
                //   writer.WriteLine("{0}: 0x{1}", elem.Type, elem.Value);
                //}
            }
        }

        //--------------------------------------------------------------

        /// <overloads>
        /// Produces a 'dump' of the given record, including attribute names etc.
        /// </overloads>
        /// --
        /// <summary>
        /// Gets a <see cref="T:System.String"/> containing a 'dump' of the given record, including attribute names etc.
        /// </summary>
        /// -
        /// <param name="record">A <see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/> to be dumped.</param>
        /// <param name="attributeIdEnumDefiningTypes">
        /// An optional array of <see cref="T:System.Type"/> specifing a set of Ids 
        /// for the attributes contained in this record.  See the 
        /// <see cref="M:InTheHand.Net.Bluetooth.ServiceRecordUtilities.Dump(System.IO.TextWriter,InTheHand.Net.Bluetooth.ServiceRecord,System.Type[])"/> 
        /// overload for more information.
        /// </param>
        /// -
        /// <returns>A <see cref="T:System.String"/> containing the 'dump' text.</returns>
        /// -
        /// <seealso cref="M:InTheHand.Net.Bluetooth.ServiceRecordUtilities.Dump(System.IO.TextWriter,InTheHand.Net.Bluetooth.ServiceRecord,System.Type[])"/>
        public static String Dump(ServiceRecord record, params Type[] attributeIdEnumDefiningTypes)
        {
            using (System.IO.StringWriter writer = new System.IO.StringWriter(
                            System.Globalization.CultureInfo.InvariantCulture)) {
                Dump(writer, record, attributeIdEnumDefiningTypes);
                writer.Close(); // close here, and to be sure to be sure also with using()
                String result = writer.ToString();
                return result;
            }
        }

        /// <summary>
        /// Produce a 'dump' of the given record, including attribute names etc to the given
        /// <see cref="T:System.IO.TextWriter"/>.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>The system has built-in a set of mappings from Service Class to 
        /// its Attribute IDs. This is supplied by the 
        /// <see cref="T:InTheHand.Net.Bluetooth.MapServiceClassToAttributeIdList"/> class,
        /// and contains the Attribute IDs defined in the base SDP specification as 
        /// well as in Bluetooth Profiles specification e.g. ObjectPushProfile, Headset,
        /// Panu, etc.
        /// If however the record being decoded is a custom one then a set of extra 
        /// Attribute Id definitions can be supplied in the 
        /// <paramref name="attributeIdEnumDefiningTypes"/> parameter.
        /// The Attribute IDs for a particular Service Class 
        /// should be defined in a static class and the set of such classes should 
        /// be passed as their <see cref="T:System.Type"/> object. e.g.
        /// <code lang="C#">
        /// static class FooAttributeId
        /// {
        ///     public const ServiceAttributeId BarName = (ServiceAttributeId)0x0300;
        /// }
        /// 
        /// &#x2026;
        ///     ServiceRecordUtilities.Dump(writer, myRecord, typeof(FooAttributeId));
        /// &#x2026;
        /// </code>
        /// </para>
        /// </remarks>
        /// -
        /// <param name="writer">A <see cref="T:System.IO.TextWriter"/> where the 'dump'
        /// text is to be written.</param>
        /// <param name="record">A <see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/> to be dumped.</param>
        /// <param name="attributeIdEnumDefiningTypes">
        /// An optional array of <see cref="T:System.Type"/> specifing a set of Ids 
        /// for the attributes contained in this record.  See the 
        /// </param>
        /// <seealso cref="M:InTheHand.Net.Bluetooth.ServiceRecordUtilities.Dump(InTheHand.Net.Bluetooth.ServiceRecord,System.Type[])"/>
        public static void Dump(System.IO.TextWriter writer, ServiceRecord record, params Type[] attributeIdEnumDefiningTypes)
        {
            if (writer == null) { throw new ArgumentNullException("writer"); }
            if (record == null) { throw new ArgumentNullException("record"); }
            //
            // ....
            // Get the AttributeIdEnumDefiningType for the services contained in the record.
            Type[] recordSpecificAttributeIdEnumDefiningTypes = GetServiceClassSpecificAttributeIdEnumDefiningType(record);
            //
            // Prepend the Universal Attribute Id definition class to the supplied list.
            Type[] allAttributeIdEnumDefiningTypes;
            allAttributeIdEnumDefiningTypes = CombineAttributeIdEnumDefiningTypes(attributeIdEnumDefiningTypes, recordSpecificAttributeIdEnumDefiningTypes);
            //
            LanguageBaseItem[] langBaseList = record.GetLanguageBaseList();
            //
            bool firstAttr = true;
            foreach (ServiceAttribute attr in record) {
                if (!firstAttr) {
                    writer.WriteLine();
                }
                ServiceAttributeId id = (ServiceAttributeId)attr.Id;
                LanguageBaseItem applicableLangBase;
                string name = AttributeIdLookup.GetName(id, allAttributeIdEnumDefiningTypes, langBaseList, out applicableLangBase);
                if (name == null) {
                    writer.WriteLine("AttrId: 0x{0:X4}", unchecked((ushort)attr.Id));
                } else {
                    writer.WriteLine("AttrId: 0x{0:X4} -- {1}", (ushort)attr.Id, name);
                }
                //----
                if (attr.Value.ElementType == ElementType.TextString) {
                    DumpString(writer, 0, attr.Value, applicableLangBase);
                } else {
                    DumpElement(writer, 0, attr.Value);
                }
                // Now print descriptive information for some cases.
                // e.g. for PDL: "( ( L2Cap ), ( Rfcomm, ChannelNumber=1 ), ( Obex ) )"
                if (id == AttributeIds.UniversalAttributeId.ProtocolDescriptorList) {
                    DumpProtocolDescriptorList(writer, 0, attr.Value);
                }
                if (id == AttributeIds.UniversalAttributeId.AdditionalProtocolDescriptorLists) {
                    DumpAdditionalProtocolDescriptorLists(writer, 0, attr.Value);
                }
                //TODO (( DumpLanguageBaseAttributeIdList, use ParseListFromElementSequence ))
                firstAttr = false;
            }//for
        }

        private static Type[] CombineAttributeIdEnumDefiningTypes(Type[] attributeIdEnumDefiningTypes, Type[] recordSpecificAttributeIdEnumDefiningTypes)
        {
            Type[] allAttributeIdEnumDefiningTypes;
            if (attributeIdEnumDefiningTypes == null) {
                attributeIdEnumDefiningTypes = new Type[0];
            }
            allAttributeIdEnumDefiningTypes = new Type[1 + attributeIdEnumDefiningTypes.Length
                + recordSpecificAttributeIdEnumDefiningTypes.Length];
            //
            allAttributeIdEnumDefiningTypes[0] = typeof(AttributeIds.UniversalAttributeId);
            attributeIdEnumDefiningTypes.CopyTo(allAttributeIdEnumDefiningTypes, 1);
            recordSpecificAttributeIdEnumDefiningTypes.CopyTo(allAttributeIdEnumDefiningTypes,
                1 + attributeIdEnumDefiningTypes.Length);
            return allAttributeIdEnumDefiningTypes;
        }

        //--------
        private static void DumpAdditionalProtocolDescriptorLists(System.IO.TextWriter writer, int depth, ServiceElement element)
        {
            System.Diagnostics.Debug.Assert(element.ElementType == ElementType.ElementSequence);
            //
            // Is a list of PDLs
            foreach (ServiceElement curList in element.GetValueAsElementList()) {
                DumpProtocolDescriptorList(writer, depth + 1, curList);
            }//foreach
        }

        private static void DumpProtocolDescriptorList(System.IO.TextWriter writer, int depth, ServiceElement element)
        {
            System.Diagnostics.Debug.Assert(element.ElementType == ElementType.ElementAlternative
                || element.ElementType == ElementType.ElementSequence);
            //
            // If passes a list of alternatives, each a protocol descriptor list,
            // then call ourselves on each list.
            if (element.ElementType == ElementType.ElementAlternative) {
                foreach (ServiceElement curStack in element.GetValueAsElementList()) {
                    DumpProtocolDescriptorListList(writer, depth + 1, curStack);
                }//foreach
                return;
            }
            //else
            DumpProtocolDescriptorListList(writer, depth, element);
        }


        private static void DumpProtocolDescriptorListList(System.IO.TextWriter writer, int depth, ServiceElement element)
        {
            // The list.
            System.Diagnostics.Debug.Assert(element.ElementType == ElementType.ElementSequence);
            WritePrefix(writer, depth);
            writer.Write("( ");
            bool firstLayer = true;
            foreach (ServiceElement layer in element.GetValueAsElementList()) {
                ServiceElement[] items = layer.GetValueAsElementArray();
                int used = 0;
                System.Diagnostics.Debug.Assert(items[used].ElementTypeDescriptor == ElementTypeDescriptor.Uuid);
                Guid protoGuid = items[used].GetValueAsUuid();
                String protoStr;
                HackProtocolId proto = GuidToHackProtocolId(protoGuid, out protoStr);
                //
                used++;
                writer.Write("{0}( {1}", (firstLayer ? string.Empty : ", "), protoStr);
                if (proto == HackProtocolId.L2Cap) {
                    if (used < items.Length) {
                        System.Diagnostics.Debug.Assert(items[used].ElementType == ElementType.UInt16);
                        UInt16 u16 = (UInt16)items[used].Value;
                        HackProtocolServiceMultiplexer psm = unchecked((HackProtocolServiceMultiplexer)u16);
                        used++;
                        writer.Write(", PSM={0}", Enum_ToStringNameOrHex(psm));
                    }
                } else if (proto == HackProtocolId.Rfcomm) {
                    if (used < items.Length) {
                        System.Diagnostics.Debug.Assert(items[used].ElementType == ElementType.UInt8);
                        byte channelNumber = (byte)items[used].Value;
                        used++;
                        writer.Write(", ChannelNumber={0}", channelNumber);
                    }
                }
                // Others include BNEP for instance, which isn't defined in the base SDP spec.
                if (used < items.Length) {
                    writer.Write(", ...");
                }
                writer.Write(" )");
                firstLayer = false;
            }//foreach layer
            writer.WriteLine(" )");
        }

        private static String Enum_ToStringNameOrHex(Enum value) //NoWorkee--<T>...where T : Enum
        {
            // Which of these is best?
            //if (Enum.IsDefined(value.GetType(), value)) {
            //    return value.ToString();
            //} else {
            //    return String.Format("0x{0:X}", value);
            //}
            ////--
            //String name = Enum.GetName(value.GetType(), value);
            //if (name != null) {
            //    return name;
            //}
            //return String.Format("0x{0:X}", value);
            //--
            String text = value.ToString();
            if (HackIsNumeric(text)) { // No int.TryParse (etc) on CF!
                // All this casting of enum values to int/underlying type is for the
                // benefit of NETCFv1, where Enum has no support for hex formatting.
                Type u = Enum.GetUnderlyingType(value.GetType());
                object asUnderlyingNumber = ((IConvertible)value).ToType(u, null);
                text = String.Format(System.Globalization.CultureInfo.InvariantCulture, "0x{0:X}", asUnderlyingNumber);
            }
            return text;
        }

        private static bool HackIsNumeric(String text)
        {
            return Char.IsDigit(text[0]) || text[0] == '-';
        }

        //--------
        /// <summary>
        /// Attempt to get the name of the protocol,
        /// and optionally it's enum id if we handle it specially.
        /// </summary>
        /// -
        /// <param name="protocolGuid">The input.
        /// </param>
        /// <param name="protoStr">The protocol's name if known, or its
        /// Guid.ToString if not.
        /// We handle some explicitly, and otherwise we see if there's a
        /// matching value in BluetoothService that has its name suffixed "Protocol".
        /// </param>
        /// -
        /// <returns>The id as a <see cref="T:InTheHand.Net.Bluetooth.ServiceRecordUtilities.HackProtocolId"/>.
        /// We handle some explicitly,
        /// otherwise we see if its a UUID16 and convert it automatically,
        /// finally if neither we return <c>zero</c>.
        /// </returns>
        private static HackProtocolId GuidToHackProtocolId(Guid protocolGuid, out string protoStr)
        {
            HackProtocolId? proto = null;
            if (protocolGuid == BluetoothService.BnepProtocol) {
                proto = HackProtocolId.Bnep;
                //else if (protocolGuid == BluetoothService.hid......)
                // Missing to test automatic uint16->HackProtocolId covnersion
                //    return HackProtocolId.Hidp;
            } else if (protocolGuid == BluetoothService.L2CapProtocol) {
                proto = HackProtocolId.L2Cap;
            } else if (protocolGuid == BluetoothService.ObexProtocol) {
                proto = HackProtocolId.Obex;
            } else if (protocolGuid == BluetoothService.RFCommProtocol) {
                proto = HackProtocolId.Rfcomm;
            } else if (protocolGuid == BluetoothService.SdpProtocol) {
                proto = HackProtocolId.Sdp;
            }
            if (proto.HasValue) {
                protoStr = proto.ToString();
                return proto.Value;
            }
            //
            // Slower ways of picking up the rest.
            // Get HackProtocolId and string name separately.
            if (IsUuid16Value(protocolGuid))
                proto = GetAsUuid16Value(protocolGuid);
            else
                proto = 0;
            //
            if (Enum.IsDefined(typeof(HackProtocolId), proto)) {
                protoStr = Enum_ToStringNameOrHex(proto);
            } else {
                string nameAttempt = BluetoothService.GetName(protocolGuid);
                const string ProtocolSuffix = "Protocol";
                if (nameAttempt != null && nameAttempt.EndsWith(ProtocolSuffix, StringComparison.Ordinal)) {
                    // Has a known name
                    Debug.Assert(nameAttempt.Length > 0);
                    Debug.Assert(nameAttempt.Length > ProtocolSuffix.Length);
                    // (Two param version required for NETCF).
                    protoStr = nameAttempt.Remove(nameAttempt.Length - ProtocolSuffix.Length, ProtocolSuffix.Length);
                } else if (proto != 0) { // Conver the integer to hex
                    protoStr = Enum_ToStringNameOrHex(proto);
                } else { // Not a standard Bluetooth value so dump its UUID
                    protoStr = protocolGuid.ToString();
                }
            }
            return proto.Value;
        }

        internal static UInt16 GetAsUuid16Value_(Guid protocolGuid) //BluetoothGuidHelper.
        {
            if (!IsUuid16Value(protocolGuid))
                throw new ArgumentException("Guid is not a Bluetooth UUID.");
            byte[] bytes = protocolGuid.ToByteArray();
            Int32 i32 = BitConverter.ToInt32(bytes, 0);
            System.Diagnostics.Debug.Assert(i32 > 0, "Int32 conversion is negative!");
            System.Diagnostics.Debug.Assert(i32 < (int)UInt16.MaxValue + 1, "Int32 conversion is in UInt32 range.");
            UInt16 u16 = checked((UInt16)i32);
            return u16;
        }

        internal static HackProtocolId GetAsUuid16Value(Guid protocolGuid) //BluetoothGuidHelper.
        {
            UInt16 u16 = GetAsUuid16Value_(protocolGuid);
            Int16 i16 = unchecked((Int16)u16);
            HackProtocolId pid = (HackProtocolId)i16;
            return pid;
        }

        internal static bool IsUuid16Value(Guid protocolGuid)
        {
            byte[] bytes = protocolGuid.ToByteArray();
            bytes[0] = bytes[1] = 0;
            Guid copyWithZeros = new Guid(bytes);
            return copyWithZeros.Equals(BluetoothService.BluetoothBase);
        }

        internal static bool IsUuid32Value(Guid protocolGuid)
        {
            byte[] bytes = protocolGuid.ToByteArray();
            bytes[0] = bytes[1] = bytes[2] = bytes[3] = 0;
            Guid copyWithZeros = new Guid(bytes);
            return copyWithZeros.Equals(BluetoothService.BluetoothBase);
        }

        internal static UInt32 GetAsUuid32Value(Guid protocolGuid) //BluetoothGuidHelper.
        {
            if (!IsUuid32Value(protocolGuid))
                throw new ArgumentException("Guid is not a Bluetooth UUID.");
            byte[] bytes = protocolGuid.ToByteArray();
            UInt32 u32 = BitConverter.ToUInt32(bytes, 0);
            return u32;
        }

        //--------
#if CODE_ANALYSIS
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "depth", Justification = "consistency with peers")]
#endif
        private static void DumpString(System.IO.TextWriter writer, int depth, ServiceElement element, LanguageBaseItem langBase)
        {
            if (langBase != null) {
                try {
                    String value = element.GetValueAsString(langBase);
                    writer.WriteLine("{0}: [{1}] '{2}'", element.ElementType, langBase.NaturalLanguage, value);
                } catch (NotSupportedException ex) {
                    System.Diagnostics.Debug.Assert(ex.Message != null);
                    System.Diagnostics.Debug.Assert(ex.Message.StartsWith("Unrecognized character encoding"));
                    writer.WriteLine("{0}: Failure: {1}", element.ElementType, ex.Message);
                }
            } else {
                try {
                    String hack = element.GetValueAsStringUtf8();
                    if (hack.IndexOf((char)0) != -1) {
                        throw new
#if ! WinCE
 System.Text.DecoderFallbackException
#else
                            // NETCF doesn't support DecoderFallbackException this what the docs say.
                            ArgumentException
#endif
("EEEEE contains nulls!  UTF-16?!");
                    }
                    writer.WriteLine("{0} (guessing UTF-8): '{1}'", element.ElementType, hack);
                } catch (
                    // TODO ! What exception thrown by UTF8Encoding on NETCF?
#if ! WinCE
                    System.Text.DecoderFallbackException
#else
                    // NETCF doesn't support DecoderFallbackException this what the docs say.
                    ArgumentException
#endif
) {
                    writer.WriteLine("{0} (Unknown/bad encoding):", element.ElementType);
                    var arr = (byte[])element.Value;
                    var str = BitConverter.ToString(arr);
                    WritePrefix(writer, depth); // On another line.
                    writer.WriteLine("Length: {0}, >>{1}<<", arr.Length, str);
                }//try
            }
        }

        private static void DumpElement(System.IO.TextWriter writer, int depth, ServiceElement elem)
        {
            WritePrefix(writer, depth);
            if (elem.ElementType == ElementType.ElementSequence || elem.ElementType == ElementType.ElementAlternative) {
                writer.WriteLine("{0}", elem.ElementType);
                foreach (ServiceElement element in elem.GetValueAsElementList()) {
                    DumpElement(writer, depth + 1, element);
                }//for
            } else if (elem.ElementType == ElementType.Nil) {
                writer.WriteLine("Nil:");
            } else if (elem.ElementType == ElementType.TextString) {
                DumpString(writer, depth, elem, null);
            } else if (elem.ElementType == ElementType.Boolean
                  || elem.ElementType == ElementType.Url) {
                // Non-numeric types
                writer.WriteLine("{0}: {1}", elem.ElementType, elem.Value);
            } else {
                String name = null;
                String valueText = null;
                if (elem.ElementTypeDescriptor == ElementTypeDescriptor.Uuid) {
                    if (elem.ElementType == ElementType.Uuid16) {
                        name = BluetoothService.GetName((UInt16)elem.Value);
                    } else if (elem.ElementType == ElementType.Uuid32) {
                        name = BluetoothService.GetName((UInt32)elem.Value);
                    } else {
                        System.Diagnostics.Debug.Assert(elem.ElementType == ElementType.Uuid128);
                        name = BluetoothService.GetName((Guid)elem.Value);
                        valueText = ((Guid)elem.Value).ToString();
                    }
                }//if UUID
                if (valueText == null) {
                    if (elem.ElementTypeDescriptor == ElementTypeDescriptor.Unknown) {
                        valueText = "unknown";
                    } else if (elem.ElementType == ElementType.UInt128
                                || elem.ElementType == ElementType.Int128) {
                        valueText = BitConverter.ToString((byte[])elem.Value);
                    } else {
                        valueText = String.Format(System.Globalization.CultureInfo.InvariantCulture, "0x{0:X}", elem.Value);
                    }
                }
                if (name == null) {
                    writer.WriteLine("{0}: {1}", elem.ElementType, valueText);
                } else {
                    writer.WriteLine("{0}: {1} -- {2}", elem.ElementType, valueText, name);
                }
            }//else
        }


        //--------------------------------------------------------------

        private static Type[] GetServiceClassSpecificAttributeIdEnumDefiningType(ServiceRecord record)
        {
            MapServiceClassToAttributeIdList instance = new MapServiceClassToAttributeIdList();
            return instance.GetAttributeIdEnumTypes(record);
        }

        //--------------------------------------------------------------

        static char[] s_lotsOfSpaces = new String(' ', 100).ToCharArray();
        private static void WritePrefix(System.IO.TextWriter writer, int depth)
        {
            const int StepSize = 4;
            int count = depth * StepSize;
            count = Math.Min(count, s_lotsOfSpaces.Length);
            writer.Write(s_lotsOfSpaces, 0, count);
        }

    }//class

}
