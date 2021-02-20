using System;
#if V1
using IList_ServiceAttribute = System.Collections.IList;
using IList_ServiceAttributeId = System.Collections.IList;
using IEnumerable_ServiceAttribute = System.Collections.IEnumerable;
using IEnumerator_ServiceAttribute = System.Collections.IEnumerator;
#else
using IList_ServiceAttribute = System.Collections.Generic.IList<InTheHand.Net.Bluetooth.ServiceAttribute>;
using IList_ServiceAttributeId = System.Collections.Generic.IList<InTheHand.Net.Bluetooth.ServiceAttributeId>;
using IEnumerable_ServiceAttribute = System.Collections.Generic.IEnumerable<InTheHand.Net.Bluetooth.ServiceAttribute>;
using IEnumerator_ServiceAttribute = System.Collections.Generic.IEnumerator<InTheHand.Net.Bluetooth.ServiceAttribute>;
#endif
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace InTheHand.Net.Bluetooth
{

    /// <summary>
    /// Holds an SDP service record.
    /// </summary>
    /// -
    /// <remarks>
    /// <para>A Service Record is the top-level container in the Service Discovery
    /// protocol/database.  It contains a list of Service Attributes each identified 
    /// by a numerical identifier (its <see cref="T:InTheHand.Net.Bluetooth.ServiceAttributeId"/>),
    /// and with its data held in a <see cref="T:InTheHand.Net.Bluetooth.ServiceElement"/>.
    /// <see cref="T:InTheHand.Net.Bluetooth.ServiceElement"/> has methods to access the
    /// various types of data it contains.
    /// </para>
    /// <para>The content of the record for a particular service class is defined in the
    /// profile&#x2019;s specification along with the IDs it uses. The IDs for the 
    /// common standard services have beed defined here, as e.g. 
    /// <see cref="T:InTheHand.Net.Bluetooth.AttributeIds.ObexAttributeId"/>,
    /// <see cref="T:InTheHand.Net.Bluetooth.AttributeIds.BasicPrintingProfileAttributeId"/>,
    /// etc. The Service Discovery profile itself defines IDs, some that can be used 
    /// in any record <see cref="T:InTheHand.Net.Bluetooth.AttributeIds.UniversalAttributeId"/>, 
    /// and others
    /// <see cref="T:InTheHand.Net.Bluetooth.AttributeIds.ServiceDiscoveryServerAttributeId"/>,
    /// and <see cref="T:InTheHand.Net.Bluetooth.AttributeIds.BrowseGroupDescriptorAttributeId"/>.
    /// </para>
    /// <para>Note that except for the attributes in the &#x201C;Universal&#x201D; category 
    /// the IDs are <i>not</i> unique, for instance the ID is 0x0200 for both 
    /// <see cref="F:InTheHand.Net.Bluetooth.AttributeIds.ServiceDiscoveryServerAttributeId.VersionNumberList"/>
    /// and <see cref="F:InTheHand.Net.Bluetooth.AttributeIds.BrowseGroupDescriptorAttributeId.GroupId"/>
    /// from <see cref="T:InTheHand.Net.Bluetooth.AttributeIds.ServiceDiscoveryServerAttributeId"/>
    /// and <see cref="T:InTheHand.Net.Bluetooth.AttributeIds.BrowseGroupDescriptorAttributeId"/>
    /// respectively.
    /// </para>
    /// <para><see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/> provides the normal 
    /// collection-type methods properties e.g. 
    /// <see cref="P:InTheHand.Net.Bluetooth.ServiceRecord.Count"/>, 
    /// <see cref="M:InTheHand.Net.Bluetooth.ServiceRecord.Contains(InTheHand.Net.Bluetooth.ServiceAttributeId)"/>,
    /// <see cref="M:InTheHand.Net.Bluetooth.ServiceRecord.GetAttributeById(InTheHand.Net.Bluetooth.ServiceAttributeId)"/>,
    /// <see cref="P:InTheHand.Net.Bluetooth.ServiceRecord.Item(System.Int32)"/>
    /// and <see cref="M:InTheHand.Net.Bluetooth.ServiceRecord.GetEnumerator"/>.  So, to 
    /// access a particular attribute&#x2019;s content get the 
    /// <see cref="T:InTheHand.Net.Bluetooth.ServiceAttribute"/> using one of those methods 
    /// and then read the data from the <see cref="T:InTheHand.Net.Bluetooth.ServiceElement"/>. 
    /// See the example below.
    /// </para>
    /// 
    /// <para>&#xA0;
    /// </para>
    /// 
    /// <para>The SDP specification defines the content of <c>TextString</c> element
    /// type very loosely and they are thus very difficult to handle when reading 
    /// from a record.
    /// The encoding of the string content is
    /// not set in the specification, and thus implementors are free to use any 
    /// encoding they fancy, for instance ASCII, UTF-8, 
    /// UTF-16, Windows-1252, etc &#x2014; all of which have been seen in record 
    /// from real devices.  It would have been much more sensible to mandate UTF-8 
    /// as the other part of the Bluetooth protocol suite do e.g. the PIN is always
    /// stored as a UTF-8 encoded string.
    /// </para>
    /// <para>Not only that but some of the attributes defined in the SDP specification
    /// can be included in more than one &#x2018;natural language&#x2019; version,
    /// and the definition of the language and the string&#x2019;s encoding
    /// is not included in the element, but is 
    /// instead defined in a separate element and the ID of the string attribute
    /// modified.  Yikes!
    /// </para>
    /// <para>  This makes it near impossible to decode the bytes in
    /// a string element at parse time and create the string object then.  Therefore
    /// the parser creates an element containing the raw bytes from the string which
    /// hopefully the user will know how to decode, passing the required encoding 
    /// information to one of methods on the element i.e.
    /// <see cref="M:InTheHand.Net.Bluetooth.ServiceElement.GetValueAsString(InTheHand.Net.Bluetooth.LanguageBaseItem)"/>,
    /// which takes a multi-language-base item from the same record (see e.g.
    /// <see cref="M:InTheHand.Net.Bluetooth.ServiceRecord.GetPrimaryLanguageBaseItem"/>),
    /// <see cref="M:InTheHand.Net.Bluetooth.ServiceElement.GetValueAsString(System.Text.Encoding)"/>
    /// which takes a .NET <see cref="T:System.Text.Encoding"/> object,
    /// or <see cref="M:InTheHand.Net.Bluetooth.ServiceElement.GetValueAsStringUtf8"/>,
    /// or <see cref="M:InTheHand.Net.Bluetooth.ServiceRecord.GetMultiLanguageStringAttributeById(InTheHand.Net.Bluetooth.ServiceAttributeId,InTheHand.Net.Bluetooth.LanguageBaseItem)"/>
    /// on the record which again takes a multi-language-base item.
    /// </para>
    /// 
    /// <para>&#xA0;
    /// </para>
    /// 
    /// <para>A Service Record can be created from the source byte array by using the 
    /// <see cref="M:InTheHand.Net.Bluetooth.ServiceRecord.CreateServiceRecordFromBytes(System.Byte[])"/>
    /// method or the 
    /// <see cref="M:InTheHand.Net.Bluetooth.ServiceRecordParser.Parse(System.Byte[],System.Int32,System.Int32)"/>
    /// on <see cref="T:InTheHand.Net.Bluetooth.ServiceRecordParser"/>.  A record
    /// can also be created from a list of <see cref="T:InTheHand.Net.Bluetooth.ServiceAttribute"/>
    /// passed to the constructor 
    /// <see cref="M:InTheHand.Net.Bluetooth.ServiceRecord.#ctor(System.Collections.Generic.IList{InTheHand.Net.Bluetooth.ServiceAttribute})"/>.
    /// </para>
    /// 
    /// <para>&#xA0;
    /// </para>
    /// 
    /// <para>From the SDP specification:
    /// </para>
    /// <list type="bullet">
    /// <item><term>2.2 ServiceRecord </term><description>&#x201C;&#x2026;
    /// a list of service attributes.&#x201D;</description></item>
    /// <item><term>2.3 ServiceAttribute</term><description>&#x201C;&#x2026;
    /// two components: an attribute id and an attribute value.&#x201D;</description></item>
    /// <item><term>2.4 Attribute ID</term><description>&#x201C;&#x2026;
    /// a 16-bit unsigned integer&#x201D;,
    /// &#x201C;&#x2026;represented as a data element.&#x201D;</description></item>
    /// <item><term>2.5 Attribute Value</term><description>&#x201C;&#x2026;
    /// a variable length field whose meaning is determined by the attribute ID&#x2026;&#x201D;,
    /// &#x201C;&#x2026;represented by a data element.&#x201D;</description></item>
    /// <item><term>3.1 Data Element</term><description>&#x201C;&#x2026;
    /// a typed data representation.
    /// It consists of two fields: a header field and a data field.
    /// The header field, in turn, is composed of two parts: a type descriptor and a size descriptor.
    /// &#x201D;</description></item>
    /// <item><term>3.2 Data Element Type Descriptor </term><description>&#x201C;&#x2026;
    /// a 5-bit type descriptor.&#x201D;</description></item>
    /// <item><term>3.3 Data Element Size Descriptor </term><description>&#x201C;&#x2026;
    /// The data element size descriptor is represented as a
    /// 3-bit size index followed by 0, 8, 16, or 32 bits.&#x201D;</description></item>
    /// </list>
    /// </remarks>
    /// -
    /// <example>
    /// <code lang="C#">
    /// ServiceRecord record = ...
    /// ServiceAttribute attr = record.GetAttributeById(UniversalAttributeId.ServiceRecordHandle);
    /// ServiceElement element = attr.Value;
    /// if(element.ElementType != ElementType.UInt32) {
    ///   throw new FooException("Invalid record content for ServiceRecordHandle");
    /// }
    /// UInt32 handle = (UInt32)element.Value;
    /// </code>
    /// or
    /// <code lang="VB.NET">
    /// Dim bppRecord As ServiceRecord = ...
    /// Dim attr As ServiceAttribute = bppRecord.GetAttributeById(BasicPrintingProfileAttributeId.PrinterName)
    /// Dim element As ServiceElement = attr.Value;
    /// ' Spec say it is in UTF-8
    /// Dim printerName As String = element.GetValueAsStringUtf8()
    /// </code>
    /// </example>
#if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
#endif
    public sealed class ServiceRecord : IEnumerable_ServiceAttribute
    {
        //--------------------------------------------------------------
        private IList_ServiceAttribute m_attributes;
        private byte[] m_srcBytes;

        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/> class 
        /// containing no <see cref="T:InTheHand.Net.Bluetooth.ServiceAttribute"/>s.
        /// </summary>
        public ServiceRecord()
        {
            m_attributes =
#if V1
                new System.Collections.ArrayList();
#else
                new System.Collections.Generic.List<ServiceAttribute>();
#endif
        }

        /// <overloads>
        /// Initializes a new instance of the 
        /// <see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/> class.
        /// </overloads>
        /// ----
        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/> class 
        /// with the specified set of <see cref="T:InTheHand.Net.Bluetooth.ServiceAttribute"/>s.
        /// </summary>
        /// -
        /// <param name="attributesList">The list of <see cref="T:InTheHand.Net.Bluetooth.ServiceAttribute"/>
        /// to add to the record,
        /// as an <see cref="T:System.Collections.Generic.IList`1"/>
        /// of <see cref="T:InTheHand.Net.Bluetooth.ServiceAttribute"/>.
        /// </param>
        public ServiceRecord(IList_ServiceAttribute attributesList)
        {
            if (attributesList == null) {
                throw new ArgumentNullException("attributesList");
            }
#if V1
            foreach (Object item in attributesList) {
                if (!(item is ServiceAttribute)) {
                    throw new ArgumentException(ErrorMsgListContainsNotAttribute);
                }
            }
#endif
            m_attributes = attributesList;
        }

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/> class 
        /// with the specified set of <see cref="T:InTheHand.Net.Bluetooth.ServiceAttribute"/>s.
        /// </summary>
        /// -
        /// <param name="attributesList">The list of <see cref="T:InTheHand.Net.Bluetooth.ServiceAttribute"/>
        /// to add to the record,
        /// as an array of <see cref="T:InTheHand.Net.Bluetooth.ServiceAttribute"/>.
        /// </param>
        public ServiceRecord(params ServiceAttribute[] attributesList)
            : this((IList_ServiceAttribute)attributesList)
        { }

        //--------------------------------------------------------------

        /// <summary>
        /// Create a <see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/> by parsing
        /// the given array of <see cref="T:System.Byte"/>.
        /// </summary>
        /// -
        /// <remarks>This uses the <see cref="T:InTheHand.Net.Bluetooth.ServiceRecordParser"/>
        /// with its default settings.
        /// See <see cref="M:InTheHand.Net.Bluetooth.ServiceRecordParser.Parse(System.Byte[],System.Int32,System.Int32)"/>
        /// for more information.  In particular for the errors that can result, two
        /// of which are listed here.
        /// </remarks>
        /// -
        /// <param name="recordBytes">A byte array containing the encoded Service Record.
        /// </param>
        /// -
        /// <returns>The new <see cref="ServiceRecord"/> parsed from the byte array.
        /// </returns>
        /// -
        /// <exception cref="T:System.Net.ProtocolViolationException">
        /// The record contains invalid content.
        /// </exception>
        /// <exception cref="T:System.NotImplementedException">
        /// The record contains an element type not supported by the parser.
        /// </exception>
        /// -
        /// <seealso cref="M:InTheHand.Net.Bluetooth.ServiceRecordParser.Parse(System.Byte[],System.Int32,System.Int32)"/>
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "bytes")]
        public static ServiceRecord CreateServiceRecordFromBytes(byte[] recordBytes)
        {
            if (recordBytes == null) {
                throw new ArgumentNullException("recordBytes");
            }
            ServiceRecord parsedRecord = new ServiceRecordParser().Parse(recordBytes);
            //// ...and copy the result to 'this'.
            //this.m_attributes = tmpRecord.m_attributes;
            //this.m_srcBytes = tmpRecord.m_srcBytes;
            //System.Diagnostics.Debug.Assert(this.m_srcBytes == recordBytes);
            return parsedRecord;
        }

        //--------------------------------------------------------------

        /// <summary>
        /// Gets the count of attributes in the record.
        /// </summary>
        public Int32 Count
        {
            [System.Diagnostics.DebuggerStepThroughAttribute]
            get { return m_attributes.Count; }
        }


        //[Obsolete("obsolete", true)]
        //internal ServiceAttribute[] HackGetAllAttributes()
        //{
        //    ServiceAttribute[] arr = new ServiceAttribute[Count];
        //    m_attributes.CopyTo(arr, 0);
        //    return arr;
        //}

        //--------------------------------------------------------------

        //[CLSCompliant(false)] // use ServiceAttributeId version
        //public void Add(ushort id, ServiceElement value)
        //{
        //    Add(unchecked((ServiceAttributeId)id), value);
        //}

        //TODO public void Add(ServiceAttributeId id, ServiceElement value)
        //{
        //    m_attributes.Add(new ServiceAttribute(id, value));
        //}

        //--------------------------------------------------------------

        /// <summary>
        /// Gets the attribute at the specified index.
        /// </summary>
        /// -
        /// <param name="index">The zero-based index of the attribute to get.</param>
        /// -
        /// <returns>A <see cref="T:InTheHand.Net.Bluetooth.ServiceAttribute"/> holding 
        /// the attribute at the specified index.</returns>
        /// -
        /// <exception cref="T:System.Exception">
        /// <para>index is less than 0.</para>
        /// <para>-or-</para>
        /// <para>index is equal to or greater than Count. </para>
        /// </exception>
        public ServiceAttribute this[Int32 index]
        {
            get { return GetAttributeByIndex(index); }
        }

        /// <summary>
        /// Gets the attribute at the specified index.
        /// </summary>
        /// -
        /// <param name="index">The zero-based index of the attribute to get.</param>
        /// -
        /// <returns>A <see cref="T:InTheHand.Net.Bluetooth.ServiceAttribute"/> holding 
        /// the attribute at the specified index.
        /// Is never <see langword="null"/>.
        /// </returns>
        /// -
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <para>index is less than 0.</para>
        /// <para>-or-</para>
        /// <para>index is equal to or greater than Count. </para>
        /// </exception>
        public ServiceAttribute GetAttributeByIndex(Int32 index)
        {
            // The following will itself check the index to throw ArgumentOutOfRangeException.
            ServiceAttribute attr = (ServiceAttribute)m_attributes[index]; // cast for non-Generics build.
            System.Diagnostics.Debug.Assert(attr != null);
            return attr;
        }

        //--------------------------------------------------------------

        /// <overloads>
        /// Determines whether a service attribute with the specified ID, 
        /// and optional natural language, is in the List.
        /// </overloads>
        /// -
        /// <summary>
        /// Determines whether a service attribute with the specified ID is in the List.
        /// </summary>
        /// -
        /// <param name="id">The id of the service attribute to locate, as a 
        /// <see cref="T:InTheHand.Net.Bluetooth.ServiceAttributeId"/>.</param>
        /// -
        /// <returns>true if item is found in the record; otherwise, false. </returns>
        public bool Contains(ServiceAttributeId id)
        {
            ServiceAttribute tmp;
            return TryGetAttributeById(id, out tmp);
        }

        /// <overloads>
        /// Returns the attribute with the given ID.
        /// </overloads>
        /// -
        /// <summary>
        /// Returns the attribute with the given ID.
        /// </summary>
        /// -
        /// <param name="id">The Attribute Id as a <see cref="T:InTheHand.Net.Bluetooth.ServiceAttributeId"/>.</param>
        /// -
        /// <returns>A <see cref="T:InTheHand.Net.Bluetooth.ServiceAttribute"/> holding 
        /// the attribute with the specified ID.
        /// Is never <see langword="null"/>.
        /// </returns>
        /// -
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">
        /// There is no attribute with the given Id in the record.
        /// Throws <see cref="T:System.ArgumentException"/> in NETCFv1
        /// </exception>
        public ServiceAttribute GetAttributeById(ServiceAttributeId id)
        {
            ServiceAttribute attribute;
            bool found = TryGetAttributeById(id, out  attribute);
            if (!found) {
#if V1
                throw new ArgumentException(ErrorMsgNoAttributeWithId);
#else
                throw new System.Collections.Generic.KeyNotFoundException(ErrorMsgNoAttributeWithId);
#endif
            }
            System.Diagnostics.Debug.Assert(attribute != null);
            return attribute;
        }


        //NODO No-(((TryGetAttributeById public? Also one with language param.)))

        private bool TryGetAttributeById(ServiceAttributeId id, out ServiceAttribute attribute)
        {
            foreach (ServiceAttribute curAttr in m_attributes) {
                if (curAttr.Id == id) {
                    attribute = curAttr;
                    System.Diagnostics.Debug.Assert(attribute != null);
                    return true;
                }
            }//for
            attribute = null;
            return false;
        }

        //--------------------------------------------------------------

        /// <summary>
        /// Get a list of the numerical IDs of the Attributes in the record 
        /// as an <see cref="T:System.Collections.Generic.IList`1"/>
        /// of <see cref="T:InTheHand.Net.Bluetooth.ServiceAttributeId"/>.
        /// </summary>
        /// -
        /// <remarks>
        /// This method will likely be only rarely used: instead 
        /// one would generally want either to read a specific attribute using 
        /// <see cref="M:InTheHand.Net.Bluetooth.ServiceRecord.GetAttributeById(InTheHand.Net.Bluetooth.ServiceAttributeId)"/>,
        /// or read every attribute by using 
        /// <see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/>'s
        /// <c>IEnumerable</c> ability e.g.
        /// <code lang="VB.NET">
        ///    For Each curAttr As ServiceAttribute In record
        ///       If curAttr.Id = UniversalAttributeId.ProtocolDescriptorList Then
        ///       ...
        ///    Next
        /// </code>
        /// <para>Note, for NETCFv1 this returns an instance of the non-Generic list 
        /// <see cref="T:System.Collections.IList"/>.
        /// </para>
        /// </remarks>
        /// -
        /// (Provide a pure example since NDocs makes big mess of displaying Generic types).
        /// <example>
        /// In C#:
        /// <code lang="C#">
        ///   IList&lt;ServiceAttributeId&gt; ids = record.GetAttributeIds();
        /// </code>
        /// In VB.NET:
        /// <code lang="VB.NET">
        ///   Dim ids As IList(Of ServiceAttributeId) = record.GetAttributeIds()
        /// </code>
        /// Or without Generics in .NET 1.1 (NETCFv1) in VB.NET:
        /// <code lang="VB.NET">
        ///   Dim ids As IList = record.GetAttributeIds()
        /// </code>
        /// </example>
#if CODE_ANALYSIS
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
#endif
        public IList_ServiceAttributeId AttributeIds
        {
            get
            {
                ServiceAttributeId[] ids = new ServiceAttributeId[Count];
                int i = 0;
                foreach (ServiceAttribute curAttr in m_attributes) {
                    ids[i] = curAttr.Id;
                    ++i;
                }//for
                return ids;
            }
        }

        //--------------------------------------------------------------
        /// <summary>
        /// Determines whether a TextString service attribute with the specified ID 
        /// and natural language 
        /// is in the List.
        /// </summary>
        /// -
        /// <param name="id">The id of the service attribute to locate, as a 
        /// <see cref="T:InTheHand.Net.Bluetooth.ServiceAttributeId"/>.</param>
        /// <param name="language">
        /// Which multi-language version of the string attribute to locate.
        /// </param>
        /// -
        /// <returns>true if item is found in the record; otherwise, false. </returns>
        public bool Contains(ServiceAttributeId id, LanguageBaseItem language)
        {
            if (language == null) { throw new ArgumentNullException("language"); }
            ServiceAttributeId actualId = CreateLanguageBasedAttributeId(id, language.AttributeIdBase);
            ServiceAttribute tmp;
            bool found = TryGetAttributeById(actualId, out tmp);
            return found && (tmp.Value.ElementType == ElementType.TextString);
        }

        /// <summary>
        /// Returns the attribute with the given ID and natural language.
        /// </summary>
        /// -
        /// <param name="id">The id of the service attribute to locate, as a 
        /// <see cref="T:InTheHand.Net.Bluetooth.ServiceAttributeId"/>.</param>
        /// <param name="language">
        /// Which multi-language version of the string attribute to locate.
        /// </param>
        /// -
        /// <returns>A <see cref="T:InTheHand.Net.Bluetooth.ServiceAttribute"/> holding 
        /// the attribute with the specified ID and language.
        /// Is never <see langword="null"/>.
        /// </returns>
        /// -
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">
        /// There is no attribute with the given Id with the given language base in the record.
        /// </exception>
        public ServiceAttribute GetAttributeById(ServiceAttributeId id, LanguageBaseItem language)
        {
            if (language == null) { throw new ArgumentNullException("language"); }
            ServiceAttributeId actualId = CreateLanguageBasedAttributeId(id, language.AttributeIdBase);
            ServiceAttribute attr = GetAttributeById(actualId);
            System.Diagnostics.Debug.Assert(attr != null);
            return attr;
        }

        /// <summary>
        /// Create the attribute id resulting for adding the language base attribute id.
        /// </summary>
        /// -
        /// <returns>The result <see cref="T:InTheHand.Net.Bluetooth.ServiceAttributeId"/>.</returns>
        /// -
        /// <exception cref="T:System.OverflowException">
        /// <paramref name="baseId"/> added to the <paramref name="id"/>
        /// would create an id that cannot be represented as an Attribute Id.
        /// </exception>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
        public static ServiceAttributeId CreateLanguageBasedAttributeId(ServiceAttributeId id, ServiceAttributeId baseId)
        {
            System.Diagnostics.Debug.Assert(typeof(short) == Enum.GetUnderlyingType(typeof(ServiceAttributeId)));
            short offset = (short)baseId;
            ServiceAttributeId actualId = id + offset;
            // If either had the MSB set, then the result must also!
            if ((actualId < 0) ^ ((id < 0) || (baseId < 0))) {
                throw new OverflowException();
            }
            return actualId;
        }

        /// <summary>
        /// Gets a <see cref="T:System.String"/> containing the value of the 
        /// <see cref="F:InTheHand.Net.Bluetooth.ElementTypeDescriptor.TextString"/>
        /// service attribute with the specified ID,
        /// using the specified natural language.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>As noted in the documentation on this class, string are defined in 
        /// an odd manner, and the multi-language strings defined in the base SDP 
        /// specification are defined in a very very odd manner.  The natural language and the 
        /// string&#x2019;s encoding are not included in the element, but instead are 
        /// defined in a separate element, and the ID of the string attribute is 
        /// modified.  This pair is present for each natural language.
        /// </para>
        /// <para>This method is provided to simplify accessing those strings, given 
        /// the Language attribute it should use it to find and decode the string.
        /// If the primary Language attribute is to be used, then use the 
        /// <see cref="M:InTheHand.Net.Bluetooth.ServiceRecord.GetPrimaryMultiLanguageStringAttributeById(InTheHand.Net.Bluetooth.ServiceAttributeId)"/> 
        /// method that takes only the id parameter.
        /// </para>
        /// </remarks>
        /// -
        /// <param name="id">The id of the service attribute to locate, as a 
        /// <see cref="T:InTheHand.Net.Bluetooth.ServiceAttributeId"/>.</param>
        /// <param name="language">
        /// Which multi-language version of the string attribute to locate.
        /// </param>
        /// -
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">
        /// There is no attribute with the given Id in the record.
        /// Throws <see cref="T:System.ArgumentException"/> in NETCFv1
        /// </exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// The service element is not of type 
        /// <see cref="F:InTheHand.Net.Bluetooth.ElementTypeDescriptor.TextString"/>.
        /// </exception>
        /// <exception cref="T:System.Text.DecoderFallbackException">
        /// If the value in the service element is not a valid string in the encoding 
        /// specified in the given <see cref="T:InTheHand.Net.Bluetooth.LanguageBaseItem"/>.
        /// </exception>
        /// -
        /// <example>
        /// C#:
        /// <code lang="C#">
        /// LanguageBaseItem primaryLang = record.GetPrimaryLanguageBaseItem();
        /// if (primaryLang == null) {
        ///   Console.WriteLine("Primary multi-language not present, would have to guess the string's encoding.");
        ///   return;
        /// }
        /// try {
        ///   String sn = record.GetMultiLanguageStringAttributeById(UniversalAttributeId.ServiceName, primaryLang);
        ///   Console.WriteLine("ServiceName: " + sn);
        /// } catch (KeyNotFoundException) {
        ///   Console.WriteLine("The record has no ServiceName Attribute.");
        /// }
        /// </code>
        /// </example>
#if CODE_ANALYSIS
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi")]
#endif
        public String GetMultiLanguageStringAttributeById(ServiceAttributeId id, LanguageBaseItem language)
        {
            if (language == null) { throw new ArgumentNullException("language"); }
            ServiceAttributeId actualId = CreateLanguageBasedAttributeId(id, language.AttributeIdBase);
            ServiceAttribute attr = GetAttributeById(actualId);
            ServiceElement element = attr.Value;
            // (No need to check that element is of type TextString, that's handled inside the following).
            String str = element.GetValueAsString(language);
            return str;
        }

        /// <summary>
        /// Gets a <see cref="T:System.String"/> containing the value of the 
        /// <see cref="F:InTheHand.Net.Bluetooth.ElementTypeDescriptor.TextString"/>
        /// service attribute with the specified ID,
        /// using the primary natural language.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>As noted in the documentation on this class, string are defined in 
        /// an odd manner, and the multi-language strings defined in the base SDP 
        /// specification are defined in a very very odd manner.  The natural language and the 
        /// string&#x2019;s encoding are not included in the element, but instead are 
        /// defined in a separate element, and the ID of the string attribute is 
        /// modified.  This pair is present for each natural language.
        /// </para>
        /// <para>This method is provided to simplify accessing those strings, it will 
        /// find the primary Language attribute and use it to find and decode the string.
        /// And if there is no primary Language attribute, which is the case in many 
        /// of the records one sees on mobile phones, it will attempt the operation 
        /// assuming the string is encoded in UTF-8 (or ASCII).
        /// </para>
        /// </remarks>
        /// -
        /// <param name="id">The id of the service attribute to locate, as a 
        /// <see cref="T:InTheHand.Net.Bluetooth.ServiceAttributeId"/>.</param>
        /// -
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">
        /// There is no attribute with the given Id in the record.
        /// Throws <see cref="T:System.ArgumentException"/> in NETCFv1
        /// </exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// The service element is not of type 
        /// <see cref="F:InTheHand.Net.Bluetooth.ElementTypeDescriptor.TextString"/>.
        /// </exception>
        /// <exception cref="T:System.Text.DecoderFallbackException">
        /// If the value in the service element is not a valid string in the encoding 
        /// specified in the given <see cref="T:InTheHand.Net.Bluetooth.LanguageBaseItem"/>.
        /// </exception>
        /// -
        /// <example>
        /// C#:
        /// <code lang="C#">
        /// try {
        ///   String sn = record.GetMultiLanguageStringAttributeById(UniversalAttributeId.ServiceName);
        ///   Console.WriteLine("ServiceName: " + sn);
        /// } catch (KeyNotFoundException) {
        ///   Console.WriteLine("The record has no ServiceName Attribute.");
        /// }
        /// </code>
        /// </example>
#if CODE_ANALYSIS
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi")]
#endif
        public String GetPrimaryMultiLanguageStringAttributeById(ServiceAttributeId id)
        {
            LanguageBaseItem lang = this.GetPrimaryLanguageBaseItem();
            if (lang == null) {
                lang = LanguageBaseItem.CreateEnglishUtf8PrimaryLanguageItem();
            }
            return GetMultiLanguageStringAttributeById(id, lang);
        }


        //--------------------------------------------------------------

        /// <summary>
        /// Gets the list of LanguageBaseAttributeId items in the service record.
        /// </summary>
        /// -
        /// <remarks>
        /// See also <see cref="M:InTheHand.Net.Bluetooth.ServiceRecord.GetPrimaryLanguageBaseItem"/>.
        /// </remarks>
        /// -
        /// <returns>
        /// An array of <see cref="T:InTheHand.Net.Bluetooth.LanguageBaseItem"/>.
        /// An array of length zero is returned if the service record contains no such attribute.
        /// </returns>
        /// -
        /// <seealso cref="M:InTheHand.Net.Bluetooth.ServiceRecord.GetPrimaryLanguageBaseItem"/>
        public LanguageBaseItem[] GetLanguageBaseList()
        {
            if (!Contains(InTheHand.Net.Bluetooth.AttributeIds.UniversalAttributeId.LanguageBaseAttributeIdList)) {
                return new LanguageBaseItem[0];
            }
            ServiceAttribute attr = GetAttributeById(InTheHand.Net.Bluetooth.AttributeIds.UniversalAttributeId.LanguageBaseAttributeIdList);
            if (attr.Value.ElementType != ElementType.ElementSequence) {
                return new LanguageBaseItem[0];
            }
            LanguageBaseItem[] langList;
            try {
                langList = LanguageBaseItem.ParseListFromElementSequence(attr.Value);
            } catch (System.Net.ProtocolViolationException) {
                return new LanguageBaseItem[0];
            }
            return langList;
        }


        /// <summary>
        /// Gets the primary LanguageBaseAttributeId item in the service record.
        /// </summary>
        /// -
        /// <remarks>
        /// For instance, can be used with methods 
        /// <see cref="M:InTheHand.Net.Bluetooth.ServiceRecord.GetMultiLanguageStringAttributeById(InTheHand.Net.Bluetooth.ServiceAttributeId,InTheHand.Net.Bluetooth.LanguageBaseItem)"/>,
        /// and <see cref="M:InTheHand.Net.Bluetooth.ServiceRecord.GetAttributeById(InTheHand.Net.Bluetooth.ServiceAttributeId,InTheHand.Net.Bluetooth.LanguageBaseItem)"/>
        /// etc.  See example code in the first.
        /// </remarks>
        /// -
        /// <returns>
        /// A <see cref="T:InTheHand.Net.Bluetooth.LanguageBaseItem"/>, or null
        /// if the service record contains no such attribute, or 
        /// no primary language item (one with Base Id 0x0100) is included.
        /// </returns>
        /// -
        /// <seealso cref="M:InTheHand.Net.Bluetooth.ServiceRecord.GetLanguageBaseList"/>
#if CODE_ANALYSIS
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
#endif
        public LanguageBaseItem GetPrimaryLanguageBaseItem()
        {
            LanguageBaseItem[] list = GetLanguageBaseList();
            System.Diagnostics.Debug.Assert(list != null);
            const ServiceAttributeId PrimaryBaseId = (ServiceAttributeId)0x0100;
            foreach (LanguageBaseItem item in list) {
                if (item.AttributeIdBase == PrimaryBaseId) { return item; }
            }//for
            return null;
        }


        //--------------------------------------------------------------

        #region IEnumerable<ServiceAttribute> Members
#if ! V1

        /// <summary>
        /// Gets an enumerator that can be used to navigate through the record's 
        /// list of <see cref="T:InTheHand.Net.Bluetooth.ServiceAttribute"/>s.
        /// </summary>
        /// -
        /// <returns>An <see cref="T:System.Collections.Generic.IEnumerator`1"/>
        /// of type <see cref="T:InTheHand.Net.Bluetooth.ServiceAttribute"/>.
        /// </returns>
        /// -
        /// <example>
        /// In C#:
        /// <code lang="C#">
        /// foreach (ServiceAttribute curAttr in record) {
        ///    if (curAttr.Id == UniversalAttributeId.ProtocolDescriptorList) {
        ///    ...
        /// }
        /// </code>
        /// In Visual Basic:
        /// <code lang="VB.NET">
        /// For Each curAttr As ServiceAttribute In record
        ///    If curAttr.Id = UniversalAttributeId.ProtocolDescriptorList Then
        ///    ...
        /// Next
        /// </code>
        /// </example>
        public IEnumerator_ServiceAttribute GetEnumerator()
        {
            return new ServiceRecordEnumerator(this);
        }
#endif
        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Gets an enumerator that can be used to navigate through the record's 
        /// list of <see cref="T:InTheHand.Net.Bluetooth.ServiceAttribute"/>s.
        /// </summary>
#if V1
        // This is the only GetEnumerator method.
        public System.Collections.IEnumerator GetEnumerator()
#else
        // EIMI, to not conflict with the Generics version.
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
#endif
        {
            return new ServiceRecordEnumerator(this);
        }
        #endregion


        sealed internal class ServiceRecordEnumerator : IEnumerator_ServiceAttribute
        {
            ServiceRecord m_record;
            int m_currentIndex = -1;

            internal ServiceRecordEnumerator(ServiceRecord record)
            {
                m_record = record;
            }


            #region IEnumerator<ServiceAttribute> Members

            public ServiceAttribute Current //x
            {
                get
                {
                    if (m_record == null) { throw new ObjectDisposedException(this.GetType().Name); }
                    if (m_currentIndex < 0) { throw new InvalidOperationException(); }
                    if (m_currentIndex >= m_record.Count) { throw new InvalidOperationException(); }
                    return m_record[m_currentIndex];
                }
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                m_record = null;
            }

            #endregion

            #region IEnumerator Members

            object System.Collections.IEnumerator.Current
            {
                // Just call the strongly-type version.
                get { return Current; }
            }

            public bool MoveNext()//x
            {
                if (m_record == null) { throw new ObjectDisposedException(this.GetType().Name); }
                ++m_currentIndex;
                System.Diagnostics.Debug.Assert(m_currentIndex >= 0);
                if (m_currentIndex >= 0 && m_currentIndex < m_record.Count) {
                    return true;
                } else {
                    m_currentIndex = m_record.Count;
                    return false;
                }
            }

            public void Reset()//x
            {
                if (m_record == null) { throw new ObjectDisposedException(this.GetType().Name); }
                m_currentIndex = -1;
            }

            #endregion
        }//2class

        //--------------------------------------------------------------
        internal void SetSourceBytes(byte[] recordBytes)
        {
            System.Diagnostics.Debug.Assert(recordBytes != null);
            m_srcBytes = (byte[])recordBytes.Clone();
        }


        /// <summary>
        /// Get the raw byte array from which the record was parsed.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>A <see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/> can be created either by manually building new 
        /// <see cref="T:InTheHand.Net.Bluetooth.ServiceAttribute"/>s holding new 
        /// <see cref="T:InTheHand.Net.Bluetooth.ServiceElement"/>s, or it can be created
        /// by <see cref="T:InTheHand.Net.Bluetooth.ServiceRecordParser"/> parsing an array
        /// of bytes read from another machine by e.g. 
        /// <see cref="M:InTheHand.Net.Sockets.BluetoothDeviceInfo.GetServiceRecords(System.Guid)"/>.
        /// In that case this method returns that source byte array.
        /// </para>
        /// <para>To creates a Service Record byte array from the contained
        /// <see cref="T:InTheHand.Net.Bluetooth.ServiceAttribute"/>s use
        /// <see cref="ToByteArray"/> or <see cref="T:InTheHand.Net.Bluetooth.ServiceRecordCreator"/>.
        /// </para>
        /// </remarks>
        /// -
        /// <value>
        /// An array of <see cref="T:System.Byte"/>, or <see langword="null"/> if
        /// the record was not created by parsing a raw record.
        /// </value>
        /// -
        /// <seealso cref="ToByteArray"/>
#if CODE_ANALYSIS
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
#endif
        public byte[] SourceBytes
        {
            get
            {
                return m_srcBytes;
            }
        }

        /// <summary>
        /// Return the byte array representing the service record.
        /// </summary>
        /// -
        /// <remarks>The byte array content is created dynamically from the
        /// <see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/> instance using
        /// the <see cref="T:InTheHand.Net.Bluetooth.ServiceRecordCreator"/> class.
        /// </remarks>
        /// -
        /// <returns>The result as an array of <see cref="T:System.Byte"/>.
        /// </returns>
        /// -
        /// <seealso cref="SourceBytes"/>
        public byte[] ToByteArray()
        {
            byte[] createdFromParsedRecord = new ServiceRecordCreator().CreateServiceRecord(this);
            return createdFromParsedRecord;
        }

        //--------------------------------------------------------------
        //internal static void Arrays_Equal(byte[] x, byte[] y) // as NETCFv1 not Generic <T>
        //{
        //    if (x.Length != y.Length) {
        //        throw new InvalidOperationException("diff lengs!!!");
        //    }
        //    for (int i = 0; i < x.Length; ++i) {
        //        if (!x[i].Equals(y[i])) {
        //            throw new InvalidOperationException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
        //                "diff at {0}, x: 0x{1:X2}, y: 0x{2:X2} !!!", i, x[i], y[i]));
        //        }
        //    }
        //}

        //--------------------------------------------------------------
        /// <exclude/>
        public const String ErrorMsgNotSeq = "LanguageBaseList attribute not of type ElementSequence.";
        /// <exclude/>
        public const string ErrorMsgNoAttributeWithId
            = "No Service Attribute with that ID.";
        /// <exclude/>
        public const string ErrorMsgListContainsNotAttribute
            = "The list contains a element which is not a ServiceAttribute.";


    }//class


}
