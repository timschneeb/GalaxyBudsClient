using System;
using InTheHand.Net.Bluetooth.AttributeIds;
#if V1
using List_ServiceElement = System.Collections.ArrayList;
using List_ServiceAttribute = System.Collections.ArrayList;
#else
using List_ServiceElement = System.Collections.Generic.List<InTheHand.Net.Bluetooth.ServiceElement>;
using List_ServiceAttribute = System.Collections.Generic.List<InTheHand.Net.Bluetooth.ServiceAttribute>;
#endif
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;


namespace InTheHand.Net.Bluetooth
{

    /// <summary>
    /// Provides a simple way to build a <see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/>, 
    /// including ServiceClassIds and ServiceNames attributes etc.
    /// </summary>
    /// -
    /// <remarks>
    /// <para>The service&#x2019;s Class Id can be set with the 
    /// <see cref="M:InTheHand.Net.Bluetooth.ServiceRecordBuilder.AddServiceClass(System.Guid)"/>/<see cref="M:InTheHand.Net.Bluetooth.ServiceRecordBuilder.AddServiceClass(System.UInt16)"/>/etc
    /// methods, the protocol stack set with the <see cref="P:InTheHand.Net.Bluetooth.ServiceRecordBuilder.ProtocolType"/>
    /// property (default RFCOMM), and the Service Name set with the 
    /// <see cref="P:InTheHand.Net.Bluetooth.ServiceRecordBuilder.ServiceName"/>
    /// property.  Other properties and methods exist for controlling the more advanced 
    /// attributes.
    /// </para>
    /// <para>Adding the standard text-string attributes (ServiceName etc) is normally quite
    /// difficult due to the very baroque manner of specifying these strings&#x2019; character 
    /// encoding and natural language.  The builder handles all the complexity internally; 
    /// the strings are written in UTF-8 encoding and marked as 'English' language.
    /// </para>
    /// </remarks>
    /// -
    /// <example>
    /// <code>
    /// ServiceRecordBuilder bldr = new ServiceRecordBuilder();
    /// bldr.AddServiceClass(BluetoothService.SerialPort);
    /// bldr.ServiceName = "Alan's SPP service";
    /// //
    /// ServiceRecord rcd = bldr.ServiceRecord;
    /// </code>
    /// 
    /// <code>
    /// ServiceRecordBuilder bldr = new ServiceRecordBuilder();
    /// bldr.ProtocolType = BluetoothProtocolDescriptorType.GeneralObex;
    /// bldr.AddServiceClass(BluetoothService.ObexFileTransfer);
    /// bldr.ServiceName = "Alan's FTP service";
    /// //
    /// ServiceRecord rcd = bldr.ServiceRecord;
    /// </code>
    /// </example>
    public class ServiceRecordBuilder
    {
        private String m_ServiceName, m_ProviderName, m_ServiceDescription;
        private System.Collections.ArrayList m_classIds = new System.Collections.ArrayList();
        private BluetoothProtocolDescriptorType m_ProtocolType = BluetoothProtocolDescriptorType.Rfcomm;
#if ! V1
        private System.Collections.Generic.List<BtPdlItem> m_profileDescrs = new System.Collections.Generic.List<BtPdlItem>();
#else
        private System.Collections.ArrayList m_profileDescrs = new System.Collections.ArrayList();
#endif
        private List_ServiceAttribute m_customList = new List_ServiceAttribute();
        private const bool m_allowDuplicates = false; //remove the const if add a set property

        private const String ErrorMsg_Duplicate = "ServiceRecordBuilder is configured to allow only one of each attribute id.";
        private const String ErrorMsg_NoServiceClasses = "Record has no Service Class IDs.";

        /// <summary>
        /// Create a new instance of the <see cref="T:InTheHand.Net.Bluetooth.ServiceRecordBuilder"/> class.
        /// </summary>
        public ServiceRecordBuilder()
        {
        }

        #region Creator method
        /// <summary>
        /// Gets the <see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/> instance 
        /// constructed by the specified <see cref="ServiceRecordBuilder"/> instance.
        /// </summary>
        /// -
        /// <value>
        /// A <see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/> that contains 
        /// the URI constructed by the <see cref="ServiceRecordBuilder"/> .
        /// </value>
        /// -
        /// <exception cref="T:System.InvalidOperationException">The <see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/> 
        /// created by the <see cref="ServiceRecordBuilder"/> properties is invalid.
        /// For instance, if duplicates attributes are disallowed but duplicates are 
        /// present.
        /// </exception>
        public ServiceRecord ServiceRecord
        {
            get
            {
                List_ServiceAttribute attrList = new List_ServiceAttribute();
                bool needsLangBaseId = false;
                // -- ClassIDs --
                attrList.Add(new ServiceAttribute(UniversalAttributeId.ServiceClassIdList,
                    new ServiceElement(ElementType.ElementSequence,
                        BuildServiceClassIdList())));
                // -- PDL --
                ServiceElement pdl;
                switch (m_ProtocolType) {
                    case BluetoothProtocolDescriptorType.L2Cap:
                        pdl = ServiceRecordHelper.CreateL2CapProtocolDescriptorList();
                        break;
                    case BluetoothProtocolDescriptorType.Rfcomm:
                        pdl = ServiceRecordHelper.CreateRfcommProtocolDescriptorList();
                        break;
                    case BluetoothProtocolDescriptorType.GeneralObex:
                        pdl = ServiceRecordHelper.CreateGoepProtocolDescriptorList();
                        break;
                    case BluetoothProtocolDescriptorType.None:
                        pdl = null;
                        break;
                    default:
                        throw new InvalidOperationException("Unknown protocol type: " + m_ProtocolType.ToString() + ".");
                }
                if (pdl != null)
                    attrList.Add(new ServiceAttribute(UniversalAttributeId.ProtocolDescriptorList, pdl));
                // -- Strings --
                if (m_ServiceName != null) {
                    attrList.Add(new ServiceAttribute(
                        UniversalAttributeId.ServiceName
                            + (int)LanguageBaseItem.PrimaryLanguageBaseAttributeId,
                        new ServiceElement(ElementType.TextString, m_ServiceName)));
                    needsLangBaseId = true;
                }
                if (m_ProviderName != null) {
                    attrList.Add(new ServiceAttribute(
                        UniversalAttributeId.ProviderName
                            + (int)LanguageBaseItem.PrimaryLanguageBaseAttributeId,
                        new ServiceElement(ElementType.TextString, m_ProviderName)));
                    needsLangBaseId = true;
                }
                if (m_ServiceDescription != null) {
                    attrList.Add(new ServiceAttribute(
                        UniversalAttributeId.ServiceDescription
                            + (int)LanguageBaseItem.PrimaryLanguageBaseAttributeId,
                        new ServiceElement(ElementType.TextString, m_ServiceDescription)));
                    needsLangBaseId = true;
                }
                if (needsLangBaseId) {
                    attrList.Add(new ServiceAttribute(UniversalAttributeId.LanguageBaseAttributeIdList,
                        CreateEnglishUtf8PrimaryLanguageServiceElement()));
                }
                // -- BtPDL --
                if (m_profileDescrs.Count != 0) {
                    System.Collections.ArrayList items = new System.Collections.ArrayList();
                    foreach (BtPdlItem cur in m_profileDescrs) {
                        ServiceElement item = new ServiceElement(ElementType.ElementSequence,
                            ServiceElementFromUuid(cur.m_classId),
                            new ServiceElement(ElementType.UInt16,
                                (UInt16)(cur.m_version * 256 + cur.m_subVersion)));
                        items.Add(item);
                    }//for
                    attrList.Add(new ServiceAttribute(
                        UniversalAttributeId.BluetoothProfileDescriptorList,
                        new ServiceElement(ElementType.ElementSequence,
                            items.ToArray(typeof(ServiceElement)))));
                }
                // -- Custom --
                attrList.AddRange(m_customList);
                // -- Create! --
                if (!m_allowDuplicates) {
                    ReportIfDuplicates(attrList, true);
                }
#if false&&true
#warning Hey Hey Hey Hey Hey Hey Hey Hey Hey Hey
#else
                attrList.Sort(delegate(ServiceAttribute x, ServiceAttribute y) {
                    int r = x.IdAsOrdinalNumber.CompareTo(y.IdAsOrdinalNumber);
                    return r;
                });
#endif
                ServiceRecord result = new ServiceRecord(attrList);
                return result;
            }
        }
        #endregion

        /// <param name="list">The list to check for duplicates.
        /// </param>
        /// <param name="storedList"><c>true</c> if checking a previously stored list 
        /// of attributes, and <c>false</c> if checking a immediate addition of an 
        /// attribute. Thus throws <c>InvalidOperationException</c> and 
        /// <c>ArgumentException</c> respectively.
        /// </param>
        private static void ReportIfDuplicates(List_ServiceAttribute list, bool storedList)
        {
#if ! V1
            System.Collections.Generic.Dictionary<ServiceAttributeId, ServiceAttribute> ids
                = new System.Collections.Generic.Dictionary<ServiceAttributeId, ServiceAttribute>(list.Count);
#else
            System.Collections.Hashtable ids = new System.Collections.Hashtable();
#endif
            foreach (ServiceAttribute cur in list) {
                if (ids.ContainsKey(cur.Id)) {
                    if (storedList)
                        throw new InvalidOperationException(ErrorMsg_Duplicate);
                    else
                        throw new ArgumentException(ErrorMsg_Duplicate);
                }
                ids.Add(cur.Id, cur);
            }//for
        }

        private List_ServiceElement BuildServiceClassIdList()
        {
            List_ServiceElement children = new List_ServiceElement();
            if (m_classIds.Count == 0) {
                throw new InvalidOperationException(ErrorMsg_NoServiceClasses);
            }
            foreach (object classRaw in m_classIds) {
                children.Add(ServiceElementFromUuid(classRaw));
            }//foreach/m_classes
            return children;
        }

        private static ServiceElement ServiceElementFromUuid(object classRaw)
        {
            ServiceElement tmp = null;

            UInt32 classU32 = 99;
            bool writeIntegral;
            // First check raw type, and also if u16/u32 inside Guid.
            // If Guid write it, otherwise handle all integral value.
            if (classRaw is Guid) {
                Guid uuid128 = (Guid)classRaw;
                if (ServiceRecordUtilities.IsUuid32Value(uuid128)) {
                    classU32 = ServiceRecordUtilities.GetAsUuid32Value(uuid128);
                    writeIntegral = true;
                } else {
                    tmp = new ServiceElement(ElementType.Uuid128, uuid128);
                    writeIntegral = false;
                }
            } else {
                System.Diagnostics.Debug.Assert(classRaw != null,
                    "Unexpected ServiceClassId value: null");
                System.Diagnostics.Debug.Assert(classRaw is Int32,
                    "Unexpected ServiceClassId type: " + classRaw.GetType().Name);
                Int32 i32 = (Int32)classRaw;
                classU32 = unchecked((UInt32)i32);
                writeIntegral = true;
            }
            if (writeIntegral) {
                try {
                    UInt16 u16 = Convert.ToUInt16(classU32);
                    Debug.Assert(classU32 <= UInt16.MaxValue, "NOT replace the throw, LTE");
                    tmp = new ServiceElement(ElementType.Uuid16, u16);
                } catch (OverflowException) {
                    Debug.Assert(classU32 > UInt16.MaxValue, "NOT replace the throw, GT");
                    tmp = new ServiceElement(ElementType.Uuid32, classU32);
                }
            }

            return tmp;
        }

        private static ServiceElement CreateEnglishUtf8PrimaryLanguageServiceElement()
        {
            ServiceElement englishUtf8PrimaryLanguage = LanguageBaseItem.CreateElementSequenceFromList(
                new LanguageBaseItem[] { LanguageBaseItem.CreateEnglishUtf8PrimaryLanguageItem() });
            return englishUtf8PrimaryLanguage;
        }

        //--------------------------------------------------------------

        #region Properties
        //public bool AllowDuplicates
        //{
        //    get { return m_allowDuplicates; }
        //    set { m_allowDuplicates = value; }
        //}

        /// <summary>
        /// Get or set a value for the <see cref="F:InTheHand.Net.Bluetooth.AttributeIds.UniversalAttributeId.ServiceName"/> 
        /// attribute.
        /// </summary>
        /// -
        /// <remarks><para>When present, a corresponding <see cref="F:InTheHand.Net.Bluetooth.AttributeIds.UniversalAttributeId.LanguageBaseAttributeIdList"/> 
        /// attribute will be added too.
        /// </para>
        /// </remarks>
        public String ServiceName
        {
            get { return m_ServiceName; }
            set { m_ServiceName = value; }
        }

        /// <summary>
        /// Get or set a value for the <see cref="F:InTheHand.Net.Bluetooth.AttributeIds.UniversalAttributeId.ProviderName"/> 
        /// attribute.
        /// </summary>
        /// -
        /// <remarks><para>When present, a corresponding <see cref="F:InTheHand.Net.Bluetooth.AttributeIds.UniversalAttributeId.LanguageBaseAttributeIdList"/> 
        /// attribute will be added too.
        /// </para>
        /// </remarks>
        public String ProviderName
        {
            get { return m_ProviderName; }
            set { m_ProviderName = value; }
        }

        /// <summary>
        /// Get or set a value for the <see cref="F:InTheHand.Net.Bluetooth.AttributeIds.UniversalAttributeId.ServiceDescription"/> 
        /// attribute.
        /// </summary>
        /// -
        /// <remarks><para>When present, a corresponding <see cref="F:InTheHand.Net.Bluetooth.AttributeIds.UniversalAttributeId.LanguageBaseAttributeIdList"/> 
        /// attribute will be added too.
        /// </para>
        /// </remarks>
        public String ServiceDescription
        {
            get { return m_ServiceDescription; }
            set { m_ServiceDescription = value; }
        }

        /// <summary>
        /// Get or set which type of element will be added for the <see cref="F:InTheHand.Net.Bluetooth.AttributeIds.UniversalAttributeId.ProtocolDescriptorList"/> 
        /// attribute.
        /// </summary>
        /// -
        /// <value>An instance of the <see cref="T:InTheHand.Net.Bluetooth.BluetoothProtocolDescriptorType"/> 
        /// enumeration.
        /// </value>
        /// -
        /// <remarks><para>Supported type are the following:
        /// </para>
        /// <list type="bullet">
        /// <item><term>None</term>
        /// <description>No PDL attribute will be added.</description>
        /// </item>
        /// <item><term>Rfcomm</term>
        /// <description>A standard RFCOMM element will be added.</description>
        /// </item>
        /// <item><term>Goep</term>
        /// <description>A standard GOEP (OBEX) element will be added.</description>
        /// </item>
        /// <item><term>L2Cap</term>
        /// <description>A standard L2CAP element will be added.</description>
        /// </item>
        /// </list>
        /// <para>The default is <see cref="F:InTheHand.Net.Bluetooth.BluetoothProtocolDescriptorType.Rfcomm"/>.
        /// </para>
        /// </remarks>
        public BluetoothProtocolDescriptorType ProtocolType
        {
            get { return m_ProtocolType; }
            set { m_ProtocolType = value; }
        }
        #endregion

        #region Class Id methods
        /// <summary>
        /// Add a Service Class Id.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>Multiple class ids can be added, and they will be written to the 
        /// <see cref="F:InTheHand.Net.Bluetooth.AttributeIds.UniversalAttributeId.ServiceClassIdList"/>
        /// attribute in the order in which they were set.
        /// </para>
        /// </remarks>
        /// -
        /// <param name="uuid128">A <see cref="T:System.Guid"/> containing a 
        /// UUID for the advertised service.
        /// </param>
        public void AddServiceClass(Guid uuid128)
        {
            m_classIds.Add(uuid128);
        }

        /// <summary>
        /// Add a Service Class Id.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>Multiple class ids can be added, and they will be written to the 
        /// <see cref="F:InTheHand.Net.Bluetooth.AttributeIds.UniversalAttributeId.ServiceClassIdList"/>
        /// attribute in the order in which they were set.
        /// </para>
        /// </remarks>
        /// -
        /// <param name="uuid16">A <see cref="T:System.UInt16"/> containing a short-form 
        /// UUID for the advertised service.
        /// </param>
        [CLSCompliant(false)] //->
        public void AddServiceClass(UInt16 uuid16)
        {
            m_classIds.Add((int)uuid16);
        }

        /// <summary>
        /// Add a Service Class Id.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>Multiple class ids can be added, and they will be written to the 
        /// <see cref="F:InTheHand.Net.Bluetooth.AttributeIds.UniversalAttributeId.ServiceClassIdList"/>
        /// attribute in the order in which they were set.
        /// </para>
        /// </remarks>
        /// -
        /// <param name="uuid32">A <see cref="T:System.UInt32"/> containing a short-form 
        /// UUID for the advertised service.
        /// </param>
        [CLSCompliant(false)] //->
        public void AddServiceClass(UInt32 uuid32)
        {
            m_classIds.Add(unchecked((int)uuid32));
        }

        /// <summary>
        /// Add a Service Class Id.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>Multiple class ids can be added, and they will be written to the 
        /// <see cref="F:InTheHand.Net.Bluetooth.AttributeIds.UniversalAttributeId.ServiceClassIdList"/>
        /// attribute in the order in which they were set.
        /// </para>
        /// </remarks>
        /// -
        /// <param name="uuid16or32">A <see cref="T:System.Int32"/> containing a short-form 
        /// UUID for the advertised service.
        /// </param>
        public void AddServiceClass(int uuid16or32)
        {
            m_classIds.Add(uuid16or32);
        }
        #endregion

        #region BluetoothProfileDescriptor methods
        /// <summary>
        /// Add a <see cref="F:InTheHand.Net.Bluetooth.AttributeIds.UniversalAttributeId.BluetoothProfileDescriptorList"/> 
        /// element.
        /// </summary>
        /// -
        /// <param name="classId">The Service Class Id of the Bluetooth profile, 
        /// as a <see cref="T:System.Guid"/>
        /// </param>
        /// <param name="majorVersion">The major version number, as a <see cref="T:System.Byte"/>.
        /// </param>
        /// <param name="minorVersion">The minor version number, as a <see cref="T:System.Byte"/>.
        /// </param>
        public void AddBluetoothProfileDescriptor(Guid classId, byte majorVersion, byte minorVersion)
        {
            m_profileDescrs.Add(new BtPdlItem(classId, majorVersion, minorVersion));
        }

        internal struct BtPdlItem
        {
            internal readonly Guid m_classId;
            internal readonly byte m_version, m_subVersion;
            internal BtPdlItem(Guid classId, byte version, byte subVersion)
            {
                m_classId = classId;
                m_version = version;
                m_subVersion = subVersion;
            }
        }
        #endregion

        #region Custom attribute methods
#if ! V1
        /// <summary>
        /// Add a set of custom attribute.
        /// </summary>
        /// -
        /// <param name="serviceAttributes">A set of attributes as an 
        /// <see cref="T:System.Collections.Generic.IEnumerable`1"/> returning 
        /// <see cref="T:InTheHand.Net.Bluetooth.ServiceAttribute"/> instances.
        /// </param>
        public void AddCustomAttributes(System.Collections.Generic.IEnumerable<ServiceAttribute> serviceAttributes)
        {
            List_ServiceAttribute newList = new List_ServiceAttribute(m_customList);
            newList.AddRange(serviceAttributes);
            if (!m_allowDuplicates) {
                ReportIfDuplicates(newList, false);
            }
            m_customList = newList;
        }
#endif

        /// <summary>
        /// Add a set of custom attribute.
        /// </summary>
        /// -
        /// <param name="serviceAttributes">A set of attributes as an 
        /// <see cref="T:System.Collections.IEnumerable"/> returning 
        /// <see cref="T:InTheHand.Net.Bluetooth.ServiceAttribute"/> instances.
        /// </param>
        public void AddCustomAttributes(System.Collections.IEnumerable serviceAttributes)
        {
            List_ServiceAttribute newList = new List_ServiceAttribute(m_customList);
            // Have to verify the type of each element.
            foreach (object cur in serviceAttributes) {
                var sa = cur as ServiceAttribute;
                if (sa == null)
                    throw new ArgumentException("Every item in the list must be a ServiceAttribute");
                newList.Add(sa);
            }
            if (!m_allowDuplicates) {
                ReportIfDuplicates(newList, false);
            }
            m_customList = newList;
        }

        /// <summary>
        /// Add a set of custom attribute.
        /// </summary>
        /// -
        /// <param name="serviceAttributes">A set of attributes as an array of 
        /// <see cref="T:InTheHand.Net.Bluetooth.ServiceAttribute"/>.
        /// </param>
        public void AddCustomAttributes(params ServiceAttribute[] serviceAttributes)
        {
#if ! V1
            System.Collections.Generic.IEnumerable<ServiceAttribute> eable;
#else
            System.Collections.IEnumerable eable;
#endif
            eable = serviceAttributes;
            this.AddCustomAttributes(eable);
        }

        /// <overloads>
        /// Add a custom attribute.
        /// </overloads>
        /// -
        /// <summary>
        /// Add a custom attribute from a given <see cref="T:InTheHand.Net.Bluetooth.ServiceAttribute"/>
        /// </summary>
        /// -
        /// <param name="serviceAttribute">An attribute as a 
        /// <see cref="T:InTheHand.Net.Bluetooth.ServiceAttribute"/> instance.
        /// </param>
        public void AddCustomAttribute(ServiceAttribute serviceAttribute)
        {
            this.AddCustomAttributes(new ServiceAttribute[] { serviceAttribute });
        }

        /// <summary>
        /// Add a custom attribute of simple type.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>If the <paramref name="elementType"/> is a numerical type
        /// then this is equivalent to using
        /// <see cref="M:InTheHand.Net.Bluetooth.ServiceElement.CreateNumericalServiceElement(InTheHand.Net.Bluetooth.ElementType,System.Object)"/>
        /// otherwise the value is used directly in creating the
        /// <see cref="T:InTheHand.Net.Bluetooth.ServiceElement"/>.
        /// </para>
        /// </remarks>
        /// -
        /// <param name="id">The Attribute Id as a <see cref="T:InTheHand.Net.Bluetooth.ServiceAttributeId"/>.</param>
        /// <param name="elementType">The type of the element as an <see cref="T:InTheHand.Net.Bluetooth.ElementType"/>.</param>
        /// <param name="value">The value for the new element.</param>
        public void AddCustomAttribute(ServiceAttributeId id, ElementType elementType, object value)
        {
            ServiceElement e;
            ElementTypeDescriptor etd = ServiceRecordParser.GetEtdForType(elementType);
            if ((etd == ElementTypeDescriptor.UnsignedInteger
                   || etd == ElementTypeDescriptor.TwosComplementInteger)) {
                e = ServiceElement.CreateNumericalServiceElement(elementType, value);
            } else {
                e = new ServiceElement(elementType, value);
            }
            this.AddCustomAttribute(new ServiceAttribute(id, e));
        }

        /// <summary>
        /// Add a custom attribute of simple type.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>If the <paramref name="elementType"/> is a numerical type
        /// then this is equivalent to using
        /// <see cref="M:InTheHand.Net.Bluetooth.ServiceElement.CreateNumericalServiceElement(InTheHand.Net.Bluetooth.ElementType,System.Object)"/>
        /// otherwise the value is used directly in creating the
        /// <see cref="T:InTheHand.Net.Bluetooth.ServiceElement"/>.
        /// </para>
        /// </remarks>
        /// -
        /// <param name="id">The Attribute Id as a <see cref="T:System.UInt16"/>.</param>
        /// <param name="elementType">The type of the element as an <see cref="T:InTheHand.Net.Bluetooth.ElementType"/>.</param>
        /// <param name="value">The value for the new element.</param>
        [CLSCompliant(false)]
        public void AddCustomAttribute(ushort id, ElementType elementType, object value)
        {
            var ids = unchecked((short)id);
            var idid = (ServiceAttributeId)ids;
            AddCustomAttribute(idid, elementType, value);
        }
        #endregion

        //--------------------------------------------------------------

        /// <summary>
        /// Converts a Java JSR 82 Bluetooth server URL into a 
        /// <see cref="T:InTheHand.Net.Bluetooth.ServiceRecordBuilder"/> instance.
        /// </summary>
        /// -
        /// <remarks>
        /// <note type="caution">The <c>authenticate</c> and <c>encrypt</c> and any 
        /// related parameters are completely disregarded.  When using with 
        /// <see cref="T:InTheHand.Net.Sockets.BluetoothListener"/> you must take 
        /// care to set the required security requirements on it directly.
        /// </note>
        /// This method is intended to read the Service Record (SDP) related items only; 
        /// in particular the Service Class ID UUID and Service Name parameters.
        /// It supports only the <c>btspp</c> and <c>btObex</c> schemes and only for
        /// server-side use only.  For instance
        ///<code lang="none">btspp://localhost:3B9FA89520078C303355AAA694238F08;name=FooBar</code>
        /// and
        ///<code lang="none">btgoep://localhost:3B9FA89520078C303355AAA694238F08</code>
        /// There is no suppport for e.g.
        ///<code lang="none">btl2cap://localhost:3B9FA89520078C303355AAA694238F08;name=Aserv</code>
        /// as the library supports only RFCOMM connections currently.
        /// </remarks>
        /// -
        /// <param name="url">A server-side JSR 82 URL in one of the supported forms.
        /// </param>
        /// -
        /// <returns>A <see cref="T:InTheHand.Net.Bluetooth.ServiceRecordBuilder"/> 
        /// initialised with the supported components of the supplied JSR 82 URL.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#")]
        public static ServiceRecordBuilder FromJsr82ServerUri(String url)
        {
            // srvString = protocol colon slashes srvHost 0*5(srvParams)
            // srvParams = name / master / encrypt / authorize / authenticate
            // text = 1*( ALPHA / DIGIT / SP / "-" / "_" )
            //
            ServiceRecordBuilder bldr = new ServiceRecordBuilder();
            const String pattern
                = @"^([a-z0-9]+)"  //scheme
                + "://localhost:"
                + "([0-9a-fA-F]{32})"   //uuid
                //param(s)
                + "(?:;([a-zA-Z]+)=([a-zA-Z0-9"
                + " " // space
                + "_" //underscore
                + "\x2D" //hyphen
                + "]+))*$"
                ;
            Match match = Regex.Match(url, pattern);
            if (!match.Success)
                throw new ArgumentException("Invalid URI format.");
            System.Diagnostics.Debug.Assert(match.Groups.Count >= 3,
                "Expect 2 non-optional groups, plus input pseudo group");
            String scheme = match.Groups[1].Value;
            String classId = match.Groups[2].Value;
            //
            switch (scheme) {
                case "btl2cap":
                    bldr.ProtocolType = BluetoothProtocolDescriptorType.L2Cap;
                    break;
                case "btspp":
                    bldr.ProtocolType = BluetoothProtocolDescriptorType.Rfcomm;
                    break;
                case "btgoep":
                    bldr.ProtocolType = BluetoothProtocolDescriptorType.GeneralObex;
                    break;
                default:
                    throw new ArgumentException("Unknown JSR82 URI scheme part.");
            }
            //
            Guid guid = new Guid(classId);
            bldr.AddServiceClass(guid);
            //
            for (int i = 3; i < match.Groups.Count; i += 2) {
                if ("NAME".Equals(match.Groups[i].Value.ToUpper(CultureInfo.InvariantCulture))) {
                    bldr.ServiceName = match.Groups[i + 1].Value;
                }
            }
            return bldr;
        }

    }//class

}
