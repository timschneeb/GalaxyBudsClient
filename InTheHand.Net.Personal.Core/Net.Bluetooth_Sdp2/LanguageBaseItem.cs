using System;
#if V1
using System.Collections;
#else
using System.Collections.Generic;
#endif
using System.Text;

namespace InTheHand.Net.Bluetooth
{

    /// <summary>
    /// Represents a member of the SDP 
    /// <see cref="F:InTheHand.Net.Bluetooth.AttributeIds.UniversalAttributeId.LanguageBaseAttributeIdList"/>,
    /// Attribute
    /// which provides for multi-language strings in a record.
    /// </summary>
    /// <remarks>
    /// &#x201C;The 
    /// <see cref="F:InTheHand.Net.Bluetooth.AttributeIds.UniversalAttributeId.LanguageBaseAttributeIdList"/>
    /// attribute is a list in which each 
    /// member contains a language identifier, a character encoding identifier, and 
    /// a base attribute ID for each of the natural languages used in the service 
    /// record.&#x201D;
    /// </remarks>
    public sealed class LanguageBaseItem
    {
        /// <summary>
        /// The primary language is specified to have base attribute ID 0x0100.
        /// </summary>
        public const ServiceAttributeId PrimaryLanguageBaseAttributeId = (ServiceAttributeId)0x0100;

        /// <summary>
        /// The Id for the UTF-8 encoding.
        /// </summary>
        public const Int16 Utf8EncodingId = 106;

        /*
         * Name: UTF-8                                                    [RFC3629]
         * MIBenum: 106
         * Source: RFC 3629
         * Alias: None 
         * 
         * 
         * Name: windows-1252
         * MIBenum: 2252
         * Source: Microsoft  (http://www.iana.org/assignments/charset-reg/windows-1252)       [Wendt]
         * Alias: None
         */


        private readonly UInt16 m_naturalLanguage;
        private readonly ServiceAttributeId m_baseAttrId;
        private readonly UInt16 m_encodingId;

        //--------------------

        /// <summary>
        /// Initialize a new instance of the <see cref="T:InTheHand.Net.Bluetooth.LanguageBaseItem"/> class.
        /// </summary>
        /// -
        /// <param name="naturalLanguage">The Natural Language field of the entry.
        /// Some example values are 0x656E which is "en", and 0x6672 which is "fr".
        /// </param>
        /// <param name="encodingId">The IETF Charset identifier for this language.
        /// e.g. 3 for US-ASCII and 106 for UTF-8,
        /// see <see cref="P:InTheHand.Net.Bluetooth.LanguageBaseItem.EncodingId"/>
        /// </param>
        /// <param name="baseAttributeId">The base Attribute Id for this language
        /// in the record.
        /// e.g. 0x100 for the Primary language.
        /// </param>
        [CLSCompliant(false)] // internal use only
        public LanguageBaseItem(UInt16 naturalLanguage, UInt16 encodingId, UInt16 baseAttributeId)
            : this(naturalLanguage, encodingId, unchecked((ServiceAttributeId)baseAttributeId))
        { }

        /// <summary>
        /// Initialize a new instance of the <see cref="T:InTheHand.Net.Bluetooth.LanguageBaseItem"/> class.
        /// </summary>
        /// -
        /// <param name="naturalLanguage">The Natural Language field of the entry.
        /// Some example values are 0x656E which is "en", and 0x6672 which is "fr".
        /// </param>
        /// <param name="encodingId">The IETF Charset identifier for this language.
        /// e.g. 3 for US-ASCII and 106 for UTF-8,
        /// see <see cref="P:InTheHand.Net.Bluetooth.LanguageBaseItem.EncodingId"/>
        /// </param>
        /// <param name="baseAttributeId">The base Attribute Id for this language
        /// in the record.
        /// e.g. 0x100 for the Primary language.
        /// </param>
        public LanguageBaseItem(Int16 naturalLanguage, Int16 encodingId, Int16 baseAttributeId)
            : this(unchecked((UInt16)naturalLanguage), unchecked((UInt16)encodingId), (ServiceAttributeId)baseAttributeId)
        { }

        //----

        /// <overloads>
        /// Initialize a new instance of the <see cref="T:InTheHand.Net.Bluetooth.LanguageBaseItem"/> class.
        /// </overloads>
        /// -
        /// <summary>
        /// Initialize a new instance of the <see cref="T:InTheHand.Net.Bluetooth.LanguageBaseItem"/> class.
        /// </summary>
        /// -
        /// <param name="naturalLanguage">The Natural Language field of the entry.
        /// Some example values are 0x656E which is "en", and 0x6672 which is "fr".
        /// </param>
        /// <param name="encodingId">The IETF Charset identifier for this language.
        /// e.g. 3 for US-ASCII and 106 for UTF-8,
        /// see <see cref="P:InTheHand.Net.Bluetooth.LanguageBaseItem.EncodingId"/>
        /// </param>
        /// <param name="baseAttributeId">The base Attribute Id for this language
        /// in the record.
        /// e.g. 0x100 for the Primary language.
        /// </param>
        [CLSCompliant(false)] // internal use only
        public LanguageBaseItem(UInt16 naturalLanguage, UInt16 encodingId, ServiceAttributeId baseAttributeId)
        {
            if (baseAttributeId == 0) {
                throw new ArgumentOutOfRangeException("baseAttributeId");
            }
            m_naturalLanguage = naturalLanguage;
            m_baseAttrId = baseAttributeId;
            m_encodingId = encodingId;
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="T:InTheHand.Net.Bluetooth.LanguageBaseItem"/> class.
        /// </summary>
        /// -
        /// <param name="naturalLanguage">The Natural Language field of the entry.
        /// Some example values are 0x656E which is "en", and 0x6672 which is "fr".
        /// </param>
        /// <param name="encodingId">The IETF Charset identifier for this language.
        /// e.g. 3 for US-ASCII and 106 for UTF-8,
        /// see <see cref="P:InTheHand.Net.Bluetooth.LanguageBaseItem.EncodingId"/>
        /// </param>
        /// <param name="baseAttributeId">The base Attribute Id for this language
        /// in the record.
        /// e.g. 0x100 for the Primary language.
        /// </param>
        public LanguageBaseItem(Int16 naturalLanguage, Int16 encodingId, ServiceAttributeId baseAttributeId)
            :this(unchecked((UInt16)naturalLanguage), unchecked((UInt16)encodingId), baseAttributeId)
        { }


        //----

        /// <summary>
        /// Initialize a new instance of the <see cref="T:InTheHand.Net.Bluetooth.LanguageBaseItem"/> class.
        /// </summary>
        /// -
        /// <param name="naturalLanguage">The Natural Language field of the entry.
        /// Some example values are "en", and "fr".
        /// </param>
        /// <param name="encodingId">The IETF Charset identifier for this language.
        /// e.g. 3 for US-ASCII and 106 for UTF-8,
        /// see <see cref="P:InTheHand.Net.Bluetooth.LanguageBaseItem.EncodingId"/>
        /// </param>
        /// <param name="baseAttributeId">The base Attribute Id for this language
        /// in the record.
        /// e.g. 0x100 for the Primary language.
        /// </param>
        [CLSCompliant(false)]
        public LanguageBaseItem(String naturalLanguage, UInt16 encodingId, ServiceAttributeId baseAttributeId)
            : this(GetLanguageIdStringAsBytes(naturalLanguage), encodingId, baseAttributeId)
        { }

        /// <summary>
        /// Initialize a new instance of the <see cref="T:InTheHand.Net.Bluetooth.LanguageBaseItem"/> class.
        /// </summary>
        /// -
        /// <param name="naturalLanguage">The Natural Language field of the entry.
        /// Some example values are "en", and "fr".
        /// </param>
        /// <param name="encodingId">The IETF Charset identifier for this language.
        /// e.g. 3 for US-ASCII and 106 for UTF-8,
        /// see <see cref="P:InTheHand.Net.Bluetooth.LanguageBaseItem.EncodingId"/>
        /// </param>
        /// <param name="baseAttributeId">The base Attribute Id for this language
        /// in the record.
        /// e.g. 0x100 for the Primary language.
        /// </param>
        public LanguageBaseItem(String naturalLanguage, Int16 encodingId, ServiceAttributeId baseAttributeId)
            : this(GetLanguageIdStringAsBytes(naturalLanguage), unchecked((UInt16)encodingId), baseAttributeId)
        { }

        //--------------------
        private static UInt16 GetLanguageIdStringAsBytes(String language)
        {
            if (language.Length != 2) {
                throw new ArgumentException(ErrorMsgLangMustAsciiTwoChars);
            }
            byte[] strBytes = System.Text.Encoding.UTF8.GetBytes(language);
            if (strBytes.Length != 2) {
                throw new ArgumentException(ErrorMsgLangMustAsciiTwoChars);
            }
            Int16 net16 = BitConverter.ToInt16(strBytes, 0);
            Int16 host16 = System.Net.IPAddress.NetworkToHostOrder(net16);
            UInt16 u16 = unchecked((UInt16)host16);
            return u16;
        }

        private string GetLanguageIdBytesAsString()
        {
            Int16 host16 = unchecked((Int16)m_naturalLanguage);
            Int16 net16 = System.Net.IPAddress.HostToNetworkOrder(host16);
            byte[] asBytes = BitConverter.GetBytes(net16);
            String asString = Encoding.ASCII.GetString(asBytes, 0, asBytes.Length);
            return asString;
        }

        //--------------------
        //

        /// <summary>
        /// Gets the list of <see cref="T:InTheHand.Net.Bluetooth.LanguageBaseItem"/>
        /// items in the service record.
        /// </summary>
        /// -
        /// <param name="elementSequence">
        /// A <see cref="T:InTheHand.Net.Bluetooth.ServiceElement"/> holding the 
        /// data from the 
        /// <see cref="F:InTheHand.Net.Bluetooth.AttributeIds.UniversalAttributeId.LanguageBaseAttributeIdList"/>
        /// attribute.
        /// </param>
        /// -
        /// <returns>
        /// An array of <see cref="T:InTheHand.Net.Bluetooth.LanguageBaseItem"/>.  
        /// An array length zero is returned if the service record contains no such attribute.
        /// </returns>
        /// -
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="elementSequence"/> is not of type 
        /// <see cref="F:InTheHand.Net.Bluetooth.ElementType.ElementSequence"/>.
        /// </exception>
        /// <exception cref="T:System.Net.ProtocolViolationException">
        /// The element sequence contains incorrectly formatted or invalid content,
        /// for example it contains the wrong element data types, or doesn't contain
        /// the elements in groups of three as required.
        /// </exception>
        public static LanguageBaseItem[] ParseListFromElementSequence(ServiceElement elementSequence)
        {
            if (elementSequence.ElementType != ElementType.ElementSequence) {
                throw new ArgumentException(ErrorMsgLangBaseListParseNotSequence);
            }
#if V1
            IList  elementList = elementSequence.GetValueAsElementList();
#else
            IList<ServiceElement> elementList = elementSequence.GetValueAsElementList();
#endif
            int numElements = elementList.Count;
            const int ElementsPerItem = 3;
            if (numElements == 0 || (numElements % ElementsPerItem) != 0) {
                throw new System.Net.ProtocolViolationException(ErrorMsgLangBaseListParseNotInThrees);
            }
            int numItems = numElements / ElementsPerItem;
            LanguageBaseItem[] items = new LanguageBaseItem[numItems];
            for (int i = 0; i < numItems; ++i) {
                // Casts are for the non-Generic version.
                ServiceElement e1Lang = (ServiceElement)elementList[i * ElementsPerItem];
                ServiceElement e2EncId = (ServiceElement)elementList[i * ElementsPerItem + 1];
                ServiceElement e3BaseId = (ServiceElement)elementList[i * ElementsPerItem + 2];
                if (e1Lang.ElementType != ElementType.UInt16 || e2EncId.ElementType != ElementType.UInt16 || e3BaseId.ElementType != ElementType.UInt16) {
                    throw new System.Net.ProtocolViolationException(ErrorMsgLangBaseListParseNotU16);
                }
                if ((UInt16)e3BaseId.Value == 0) {
                    throw new System.Net.ProtocolViolationException(ErrorMsgLangBaseListParseBaseInvalid);
                }
                LanguageBaseItem item = new LanguageBaseItem(
                        (UInt16)e1Lang.Value, (UInt16)e2EncId.Value, (UInt16)e3BaseId.Value);
                items[i] = item;
            }
            return items;
        }

        //--------------------

        /// <summary>
        /// Create a data element for the 
        /// <see cref="F:InTheHand.Net.Bluetooth.AttributeIds.UniversalAttributeId.LanguageBaseAttributeIdList"/>
        /// attribute
        /// from the list of <see cref="T:InTheHand.Net.Bluetooth.LanguageBaseItem"/>
        /// </summary>
        /// -
        /// <param name="list">
        /// An array of <see cref="T:InTheHand.Net.Bluetooth.LanguageBaseItem"/>.
        /// </param>
        /// -
        /// <returns>
        /// A <see cref="T:InTheHand.Net.Bluetooth.ServiceElement"/> holding the 
        /// <see cref="F:InTheHand.Net.Bluetooth.AttributeIds.UniversalAttributeId.LanguageBaseAttributeIdList"/>
        /// element, to be added to a generally the 
        /// <see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/>.
        /// </returns>
        public static ServiceElement CreateElementSequenceFromList(LanguageBaseItem[] list)
        {
#if !  V1
            IList<ServiceElement> children = new List<ServiceElement>();
#else
            IList children = new ArrayList();
#endif
            foreach (LanguageBaseItem item in list) {
                //String lang = item.NaturalLanguage;
                //UInt16 langNumerical = GetLanguageIdStringAsBytes(lang);
                UInt16 langNumerical = item.NaturalLanguageAsUInt16;
                children.Add(new ServiceElement(ElementType.UInt16,
                    (UInt16)langNumerical));
                children.Add(new ServiceElement(ElementType.UInt16,
                    (UInt16)item.EncodingId));
                children.Add(new ServiceElement(ElementType.UInt16,
                    (UInt16)item.AttributeIdBase));
            }
            return new ServiceElement(ElementType.ElementSequence,
                children);
        }

        /// <summary>
        /// Create a <see cref="T:InTheHand.Net.Bluetooth.LanguageBaseItem"/> instance
        /// for a primary language of English and a string encoding of UTF-8.
        /// </summary>
        /// <returns>The <see cref="T:InTheHand.Net.Bluetooth.LanguageBaseItem"/> instance.
        /// </returns>
        public static LanguageBaseItem CreateEnglishUtf8PrimaryLanguageItem()
        {
            return new LanguageBaseItem("en", LanguageBaseItem.Utf8EncodingId,
                LanguageBaseItem.PrimaryLanguageBaseAttributeId);
        }

        //--------------------

        /// <summary>
        /// Gets the value of the Natural Language field of the entry.
        /// </summary>
        /// <example>Some example value may be "en", and "fr".</example>
        public string NaturalLanguage
        {
            get
            {
                return GetLanguageIdBytesAsString();
            }
        }

        /// <summary>
        /// Gets the value of the Natural Language field of the entry, as a <see cref="T:System.UInt16"/>.
        /// </summary>
        /// <example>Some example value may be 0x656e for "en", and 0x6672 for "fr".</example>
        [CLSCompliant(false)] //use NaturalLanguageAsInt16 
        public UInt16 NaturalLanguageAsUInt16 { get { return m_naturalLanguage; } }

        /// <summary>
        /// Gets the value of the Natural Language field of the entry, as a <see cref="T:System.UInt16"/>.
        /// </summary>
        /// <example>Some example value may be 0x656e for "en", and 0x6672 for "fr".</example>
        public Int16 NaturalLanguageAsInt16 { get { return unchecked((Int16)m_naturalLanguage); } }

        /// <summary>
        /// Gets the base Attribute Id for this language.
        /// </summary>
        public ServiceAttributeId AttributeIdBase { get { return m_baseAttrId; } }

        //TODO (( ?IETF EncodingId as enum ))
        /// <summary>
        /// Get the IETF Charset identifier for this language.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>Example values are 3 for US-ASCII and 106 for UTF-8.
        /// See the full list at <see href="http://www.iana.org/assignments/character-sets"/>
        /// </para>
        /// </remarks>
        /// -
        /// <seealso cref="P:InTheHand.Net.Bluetooth.LanguageBaseItem.EncodingIdAsInt16"/>
        [CLSCompliant(false)] //use EncodingIdAsInt16
        public UInt16 EncodingId { get { return m_encodingId; } }

        /// <summary>
        /// Get the IETF Charset identifier for this language, as an Int16.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>
        /// See <see cref="P:InTheHand.Net.Bluetooth.LanguageBaseItem.EncodingId"/>.
        /// </para>
        /// </remarks>
        /// -
        /// <seealso cref="P:InTheHand.Net.Bluetooth.LanguageBaseItem.EncodingId"/>
        public Int16 EncodingIdAsInt16 { get { return unchecked((Int16)m_encodingId); } }

        //--------------------

        /// <summary>
        /// Gets an <see cref="T:System.Text.Encoding"/> appropriate for this language base item.
        /// </summary>
        /// -
        /// <returns>The <see cref="T:System.Text.Encoding"/>
        /// appropriate for this language base item.
        /// </returns>
        /// -
        /// <remarks>
        /// <para>We support the following set of mappings from encoding id to .NET
        /// Encoding name.
        /// <list type="table">
        /// <listheader><term>Id</term><description>Encoding</description></listheader>
        /// <item><term>3</term><description>us-ascii</description></item>
        /// <item><term>4</term><description>iso-8859-1</description></item>
        /// <item><term>5</term><description>iso-8859-2</description></item>
        /// <item><term>6</term><description>iso-8859-3</description></item>
        /// <item><term>7</term><description>iso-8859-4</description></item>
        /// <item><term>8</term><description>iso-8859-5</description></item>
        /// <item><term>9</term><description>iso-8859-6</description></item>
        /// <item><term>10</term><description>iso-8859-7</description></item>
        /// <item><term>11</term><description>iso-8859-8</description></item>
        /// <item><term>12</term><description>iso-8859-9</description></item>
        /// <item><term>13</term><description>iso-8859-10</description></item>
        /// <item><term>106 (0x006a)</term><description>UTF-8</description></item>
        /// <item><term>109</term><description>iso-8859-13</description></item>
        /// <item><term>110</term><description>iso-8859-14</description></item>
        /// <item><term>111</term><description>iso-8859-15</description></item>
        /// <item><term>112</term><description>iso-8859-16</description></item>
        /// <item><term>1013 (0x03f5)</term><description>unicodeFFFE (UTF-16BE)</description></item>
        /// <item><term>1014</term><description>utf-16 (UTF-16LE)</description></item>
        /// <item><term>1015</term><description>utf-16 (UTF-16, we assume UTF16-LE)</description></item>
        /// <item><term>2252 to 2258 (0x08cc to 0x08d2)</term><description>windows-1252 to Windows-1258</description></item>
        /// </list>
        /// Note that not all platforms support all these Encodings, for instance on
        /// my Windows XP SP2 box iso-8859-10/-14/-16 are not supported.  On NETCF on
        /// Windows Mobile 5 only five of the ISO-8859 encodings are supported.
        /// Regardless I've seen no SDP records that use ISO-8859 encodings so this is 
        /// not a problem, most records actually use UTF-8.
        /// </para>
        /// </remarks>
        /// -
        /// <exception cref="T:System.NotSupportedException">
        /// The IETF encoding id for this language base item is currently unknown.
        /// If valid, add it to the <c>s_IetfCharsetIdToDotNetEncodingNameTable</c> table, 
        /// providing a mapping to its Windows code page name.
        /// </exception>
#if CODE_ANALYSIS
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification="but throws")]
#endif
        public Encoding GetEncoding()
        {
            if (m_encodingId >= 2252 && m_encodingId <= 2258) { // Windows-125x
                int num = m_encodingId - 1000;
                Encoding enc = Encoding.GetEncoding("windows-" + num.ToString(System.Globalization.CultureInfo.InvariantCulture));
                return enc;
            }
            foreach (IetfCharsetIdToDotNetEncodingNameMap row in s_IetfCharsetIdToDotNetEncodingNameTable) {
                if (row.IetfCharsetId == m_encodingId) {
                    Encoding enc = Encoding.GetEncoding(row.DotNetEncodingName);
                    return enc;
                }
            }
            //TODO LanguageBaseItem.GetEncoding--NotImplementedException rather than NotSupportedException?
            throw new NotSupportedException(
                String.Format(System.Globalization.CultureInfo.InvariantCulture,
                    ErrorMsgFormatUnrecognizedEncodingId,
                    m_encodingId));
        }

        //--------------------

        private struct IetfCharsetIdToDotNetEncodingNameMap
        {
            public readonly UInt16 IetfCharsetId;
            public readonly String DotNetEncodingName;

            internal IetfCharsetIdToDotNetEncodingNameMap(
                UInt16 ietfCharsetId_, String DotNetEncodingName_)
            {
                this.IetfCharsetId = ietfCharsetId_;
                this.DotNetEncodingName = DotNetEncodingName_;
            }
        }
        private static readonly IetfCharsetIdToDotNetEncodingNameMap[] s_IetfCharsetIdToDotNetEncodingNameTable ={
            //--------
            //
            new IetfCharsetIdToDotNetEncodingNameMap(3, "us-ascii"),
            //--------
            //
            new IetfCharsetIdToDotNetEncodingNameMap(4, "iso-8859-" + "1"),
            new IetfCharsetIdToDotNetEncodingNameMap(5, "iso-8859-" + "2"),
            new IetfCharsetIdToDotNetEncodingNameMap(6, "iso-8859-" + "3"),
            new IetfCharsetIdToDotNetEncodingNameMap(7, "iso-8859-" + "4"),
            new IetfCharsetIdToDotNetEncodingNameMap(8, "iso-8859-" + "5"),
            new IetfCharsetIdToDotNetEncodingNameMap(9, "iso-8859-" + "6"),
            new IetfCharsetIdToDotNetEncodingNameMap(10, "iso-8859-" + "7"),
            new IetfCharsetIdToDotNetEncodingNameMap(11, "iso-8859-" + "8"),
            new IetfCharsetIdToDotNetEncodingNameMap(12, "iso-8859-" + "9"),
            new IetfCharsetIdToDotNetEncodingNameMap(13, "iso-8859-" + "10"), // not in XpSp2.
            //--------
            //
            new IetfCharsetIdToDotNetEncodingNameMap(106/*0x006a*/, "UTF-8"),
            //--------
            //
            new IetfCharsetIdToDotNetEncodingNameMap(109, "iso-8859-" + "13"),
            new IetfCharsetIdToDotNetEncodingNameMap(110, "iso-8859-" + "14"), // not in XpSp2.
            new IetfCharsetIdToDotNetEncodingNameMap(111, "iso-8859-" + "15"),
            new IetfCharsetIdToDotNetEncodingNameMap(112, "iso-8859-" + "16"), // not in XpSp2.
            //--------
            //  Name: UTF-16BE      MIBenum: 1013   = 0x03f5
            //  Name: UTF-16LE      MIBenum: 1014
            //  Name: UTF-16        MIBenum: 1015
            //  .NET encoding names:
            //  1200    utf-16          Unicode
            //  1201    unicodeFFFE     Unicode (Big-Endian) 
            new IetfCharsetIdToDotNetEncodingNameMap(1013, "unicodeFFFE"),
            new IetfCharsetIdToDotNetEncodingNameMap(1014, "utf-16"),
            // For the id for unspecified use LE to suit Windows' servers.
            new IetfCharsetIdToDotNetEncodingNameMap(1015, "utf-16"),
        };


        /// <exclude/>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
#if CODE_ANALYSIS
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "0#")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
#endif
        public static String TestAllDefinedEncodingMappingRows(out int numberSuccessful, out int numberFailed)
        {
            numberSuccessful = 0;
            numberFailed = 0;
            StringBuilder bldr = new StringBuilder();
            foreach (IetfCharsetIdToDotNetEncodingNameMap row in s_IetfCharsetIdToDotNetEncodingNameTable) {
                bldr.AppendFormat(System.Globalization.CultureInfo.InvariantCulture,
                    "id: {0}, name: {1}.  ", row.IetfCharsetId, row.DotNetEncodingName);
                try {
                    Encoding enc = Encoding.GetEncoding(row.DotNetEncodingName);
                    bldr.Append("Success");
                    ++numberSuccessful;
                } catch (Exception ex) {
                    bldr.AppendFormat(System.Globalization.CultureInfo.InvariantCulture,
                        "Failed with {0}:{1}", ex.GetType().FullName, ex.Message);
                    ++numberFailed;
                }
                bldr.Append("\r\n");//no AppendLine on NETCFv1
            }
            return bldr.ToString();
        }

        //--------------------------------------------------------------
        /// <exclude/>
        public const String ErrorMsgLangBaseListParseNotU16
            = "Element in LanguageBaseAttributeIdList not type UInt16.";
        /// <exclude/>
        public const String ErrorMsgLangBaseListParseBaseInvalid
            = "Base element in LanguageBaseAttributeIdList has unacceptable value."; 
        /// <exclude/>
        public const String ErrorMsgLangBaseListParseNotSequence
            = "LanguageBaseAttributeIdList elementSequence not an ElementSequence.";
        /// <exclude/>
        public const String ErrorMsgLangBaseListParseNotInThrees
            = "LanguageBaseAttributeIdList must contain items in groups of three.";
        /// <exclude/>
        public const String ErrorMsgFormatUnrecognizedEncodingId
            = "Unrecognized character encoding ({0}); add to LanguageBaseItem mapping table.";
        /// <exclude/>
        public const String ErrorMsgLangMustAsciiTwoChars
            = "A language code must be a two byte ASCII string.";
            
    }//class


}
