using System;

namespace InTheHand.Net.Bluetooth.AttributeIds
{

    /// <summary>
    /// Defines the ids for the &#x201C;universal attributes&#x201D;, those 
    /// &#x201C;whose definitions are common to all service records.&#x201D;
    /// </summary>
    /// <remarks>
    /// <para>&#x201C;
    /// Universal attributes are those service attributes whose definitions are common
    /// to all service records. Note that this does not mean that every service record
    /// must contain values for all of these service attributes. However, if a service
    /// record has a service attribute with an attribute ID allocated to a universal
    /// attribute, the attribute value must conform to the universal attribute’s definition.
    /// </para><para>&#x201C;
    /// Only two attributes are required to exist in every service record instance. They
    /// are the ServiceRecordHandle (attribute ID 0x0000) and the ServiceClassIDList
    /// (attribute ID 0x0001). All other service attributes are optional within a service
    /// record.
    /// &#x201D;</para>
    /// <para>&#x201C;Attribute IDs in the range of 0x000D-0x01FF are reserved.&#x201D;</para>
    /// </remarks>
    public
#if ! V1
 static
#endif
 class UniversalAttributeId
    {
#if V1
        private UniversalAttributeId() { }
#endif

        /// <summary>
        /// A service record handle is a 32-bit number that uniquely identifies each service
        /// record within an SDP server.
        /// [0x0000]
        /// </summary>
        /// <remarks>
        /// <para>[<c>32-bit unsigned integer</c>]</para>
        /// </remarks>
        public const ServiceAttributeId ServiceRecordHandle = (ServiceAttributeId)(short)0x0000;
        // double cast required for NETCFv1 compiler

        /// <summary>
        /// The ServiceClassIDList attribute consists of a data element sequence in which
        /// each data element is a UUID representing the service classes that a given service
        /// record conforms to.
        /// [0x0001]
        /// </summary>
        /// <remarks>
        /// <para>[<c>Data Element Sequence</c>]</para>
        /// <para>&#x201C;The ServiceClassIDList attribute consists of a data element sequence in which
        /// each data element is a UUID representing the service classes that a given service
        /// record conforms to. The UUIDs are listed in order from the most specific
        /// class to the most general class. The ServiceClassIDList must contain at least
        /// one service class UUID.&#x201D;</para>
        /// </remarks>
        public const ServiceAttributeId ServiceClassIdList = (ServiceAttributeId)(ServiceAttributeId)0x0001;

        /// <summary>
        /// The ServiceRecordState is a 32-bit integer that is used to facilitate caching of
        /// ServiceAttributes.
        /// [0x0002]
        /// </summary>
        ///<remarks>
        /// <para>[<c>32-bit unsigned integer</c>]</para>
        /// <para>&#x201C;
        /// The ServiceRecordState is a 32-bit integer that is used to facilitate caching of
        /// ServiceAttributes. If this attribute is contained in a service record, its value is
        /// guaranteed to change when any other attribute value is added to, deleted from
        /// or changed within the service record. This permits a client to check the value of
        /// this single attribute. If its value has not changed since it was last checked, the
        /// client knows that no other attribute values within the service record have
        /// changed.
        /// &#x201D;</para>
        /// </remarks>
        public const ServiceAttributeId ServiceRecordState = (ServiceAttributeId)(ServiceAttributeId)0x0002;

        /// <summary>
        /// The ServiceID is a UUID that universally and uniquely identifies the service
        /// instance described by the service record.
        /// [0x0003]
        /// </summary>
        ///<remarks>
        /// <para>[<c>UUID</c>]</para>
        /// <para>&#x201C;
        /// The ServiceID is a UUID that universally and uniquely identifies the service
        /// instance described by the service record. This service attribute is particularly
        /// useful if the same service is described by service records in more than one
        /// SDP server.
        /// &#x201D;</para>
        /// </remarks>
        public const ServiceAttributeId ServiceId = (ServiceAttributeId)0x0003;

        /// <summary>
        /// The ProtocolDescriptorList attribute describes one or more protocol stacks that
        /// may be used to gain access to the service described by the service record.
        /// [0x0004]
        /// </summary>
        ///<remarks>
        /// <para>[<c>Data Element Sequence</c> or <c>Data Element Alternative</c>]</para>
        /// <para>&#x201C;
        /// The ProtocolDescriptorList attribute describes one or more protocol stacks that
        /// may be used to gain access to the service described by the service record.
        /// </para><para>&#x201C;
        /// If the ProtocolDescriptorList describes a single stack, it takes the form of a data
        /// element sequence in which each element of the sequence is a protocol
        /// descriptor. Each protocol descriptor is, in turn, a data element sequence whose
        /// first element is a UUID identifying the protocol and whose successive elements
        /// are protocol-specific parameters. Potential protocol-specific parameters are a
        /// protocol version number and a connection-port number. The protocol descriptors
        /// are listed in order from the lowest layer protocol to the highest layer protocol
        /// used to gain access to the service.
        /// </para><para>&#x201C;
        /// If it is possible for more than one kind of protocol stack to be used to gain
        /// access to the service, the ProtocolDescriptorList takes the form of a data element
        /// alternative where each member is a data element sequence as described
        /// in the previous paragraph.
        /// </para><para>&#x201C;
        /// Protocol Descriptors
        /// </para><para>&#x201C;
        /// A protocol descriptor identifies a communications protocol and provides protocol-
        /// specific parameters. A protocol descriptor is represented as a data element
        /// sequence. The first data element in the sequence must be the UUID that identifies
        /// the protocol. Additional data elements optionally provide protocol-specific
        /// information, such as the L2CAP protocol/service multiplexer (PSM) and the
        /// RFCOMM server channel number (CN) shown below.
        /// </para><para>&#x201C;
        /// ProtocolDescriptorList Examples
        /// </para><para>&#x201C;
        /// These examples are intended to be illustrative. The parameter formats for each
        /// protocol are not defined within this specification.
        /// </para><para>&#x201C;
        /// In the first two examples, it is assumed that a single RFCOMM instance exists
        /// on top of the L2CAP layer. In this case, the L2CAP protocol specific information
        /// (PSM) points to the single instance of RFCOMM. In the last example, two different
        /// and independent RFCOMM instances are available on top of the L2CAP
        /// layer. In this case, the L2CAP protocol specific information (PSM) points to a
        /// distinct identifier that distinguishes each of the RFCOMM instances. According
        /// to the L2CAP specification, this identifier takes values in the range
        /// 0x1000-0xFFFF.
        /// </para><para>&#x201C;
        /// IrDA-like printer
        /// </para><para>&#x201C;
        /// ( ( L2CAP, PSM=RFCOMM ), ( RFCOMM, CN=1 ), ( PostscriptStream ) )
        /// </para><para>&#x201C;
        /// IP Network Printing
        /// </para><para>&#x201C;
        /// ( ( L2CAP, PSM=RFCOMM ), ( RFCOMM, CN=2 ), ( PPP ), ( IP ), ( TCP ),
        /// ( IPP ) )
        /// </para><para>&#x201C;
        /// Synchronization Protocol Descriptor Example
        /// </para><para>&#x201C;
        /// ( ( L2CAP, PSM=0x1001 ), ( RFCOMM, CN=1 ), ( Obex ), ( vCal ) )
        /// </para><para>&#x201C;
        /// ( ( L2CAP, PSM=0x1002 ), ( RFCOMM, CN=1 ), ( Obex ),
        /// </para><para>&#x201C;
        /// ( otherSynchronisationApplication ) )
        /// &#x201D;</para>
        /// </remarks>
        public const ServiceAttributeId ProtocolDescriptorList = (ServiceAttributeId)0x0004;

        /// <summary>
        /// The BrowseGroupList attribute consists of a data element sequence in which
        /// each element is a UUID that represents a browse group to which the service
        /// record belongs.
        /// [0x0005]
        /// </summary>
        ///<remarks>
        /// <para>[<c>Data Element Sequence</c>]</para>
        /// <para>&#x201C;
        /// The BrowseGroupList attribute consists of a data element sequence in which
        /// each element is a UUID that represents a browse group to which the service
        /// record belongs. The top-level browse group ID, called PublicBrowseRoot and
        /// representing the root of the browsing hierarchy, has the value 
        /// 00001002-0000-1000-8000-00805F9B34FB 
        /// (UUID16: 0x1002) from the Bluetooth Assigned
        /// Numbers document.
        /// &#x201D;</para>
        /// </remarks>
        public const ServiceAttributeId BrowseGroupList = (ServiceAttributeId)0x0005;

        /// <summary>
        /// In order to support human-readable attributes for multiple natural languages in
        /// a single service record, a base attribute ID is assigned for each of the natural
        /// languages used in a service record. The human-readable universal attributes
        /// are then defined with an attribute ID offset from each of these base values,
        /// rather than with an absolute attribute ID.
        /// [0x0006]
        /// </summary>
        ///<remarks>
        /// <para>[<c>Data Element Sequence</c>]</para>
        /// <para>&#x201C;
        /// In order to support human-readable attributes for multiple natural languages in
        /// a single service record, a base attribute ID is assigned for each of the natural
        /// languages used in a service record. The human-readable universal attributes
        /// are then defined with an attribute ID offset from each of these base values,
        /// rather than with an absolute attribute ID.
        /// </para><para>&#x201C;
        /// The LanguageBaseAttributeIDList attribute is a list in which each member contains
        /// a language identifier, a character encoding identifier, and a base attribute
        /// ID for each of the natural languages used in the service record. The Language-
        /// BaseAttributeIDList attribute consists of a data element sequence in which
        /// each element is a 16-bit unsigned integer. The elements are grouped as triplets
        /// (threes).
        /// </para><para>&#x201C;
        /// The first element of each triplet contains an identifier representing the natural
        /// language. The language is encoded according to ISO 639:1988 (E/F): “Code
        /// for the representation of names of languages”.
        /// </para><para>&#x201C;
        /// The second element of each triplet contains an identifier that specifies a character
        /// encoding used for the language. Values for character encoding can be
        /// found in IANA's database1, and have the values that are referred to as MIBEnum
        /// values. The recommended character encoding is UTF-8.
        /// </para><para>&#x201C;
        /// The third element of each triplet contains an attribute ID that serves as the
        /// base attribute ID for the natural language in the service record. Different service
        /// records within a server may use different base attribute ID values for the
        /// same language.
        /// </para><para>&#x201C;
        /// To facilitate the retrieval of human-readable universal attributes in a principal
        /// language, the base attribute ID value for the primary language supported by a
        /// service record must be 0x0100. Also, if a LanguageBaseAttributeIDList
        /// attribute is contained in a service record, the base attribute ID value contained
        /// in its first element must be 0x0100.
        /// </para>
        /// </remarks>
        public const ServiceAttributeId LanguageBaseAttributeIdList = (ServiceAttributeId)0x0006;

        /// <summary>
        /// The ServiceTimeToLive attribute is a 32-bit integer that contains the number of
        /// seconds for which the information in a service record is expected to remain
        /// valid and unchanged.
        /// [0x0007]
        /// </summary>
        ///<remarks>
        /// <para>[<c>32-bit unsigned integer</c>]</para>
        /// <para>&#x201C;
        /// The ServiceTimeToLive attribute is a 32-bit integer that contains the number of
        /// seconds for which the information in a service record is expected to remain
        /// valid and unchanged. This time interval is measured from the time that the
        /// attribute value is retrieved from the SDP server. This value does not imply a
        /// guarantee that the service record will remain available or unchanged. It is
        /// simply a hint that a client may use to determine a suitable polling interval to revalidate
        /// the service record contents.
        /// &#x201D;</para>
        /// </remarks>
        public const ServiceAttributeId ServiceInfoTimeToLive = (ServiceAttributeId)0x0007;

        /// <summary>
        /// The ServiceAvailability attribute is an 8-bit unsigned integer that represents the
        /// relative ability of the service to accept additional clients.
        /// [0x0008]
        /// </summary>
        ///<remarks>
        /// <para>[<c>8-bit unsigned integer</c>]</para>
        /// <para>&#x201C;
        /// The ServiceAvailability attribute is an 8-bit unsigned integer that represents the
        /// relative ability of the service to accept additional clients. A value of 0xFF indicates
        /// that the service is not currently in use and is thus fully available, while a
        /// value of 0x00 means that the service is not accepting new clients. For services
        /// that support multiple simultaneous clients, intermediate values indicate the relative
        /// availability of the service on a linear scale.
        /// &#x201D;</para><para>&#x201C;
        /// For example, a service that can accept up to 3 clients should provide ServiceAvailability
        /// values of 0xFF, 0xAA, 0x55, and 0x00 when 0, 1, 2, and 3 clients, respectively,
        /// are utilizing the service. The value 0xAA is approximately (2/3) * 0xFF and
        /// represents 2/3 availability, while the value 0x55 is approximately (1/3)*0xFF and
        /// represents 1/3 availability. Note that the availability value may be approximated as
        /// &#x201D;</para><para>&#x201C;
        /// <c>( 1 - ( current_number_of_clients / maximum_number_of_clients ) ) * 0xFF</c>
        /// &#x201D;</para><para>&#x201C;
        /// When the maximum number of clients is large, this formula must be modified to
        /// ensure that ServiceAvailability values of 0x00 and 0xFF are reserved for their
        /// defined meanings of unavailability and full availability, respectively.
        /// &#x201D;</para><para>&#x201C;
        /// Note that the maximum number of clients a service can support may vary
        /// according to the resources utilized by the service's current clients.
        /// &#x201D;</para><para>&#x201C;
        /// A non-zero value for ServiceAvailability does not guarantee that the service will
        /// be available for use. It should be treated as a hint or an approximation of availability
        /// status.
        /// &#x201D;</para>
        /// </remarks>
        public const ServiceAttributeId ServiceAvailability = (ServiceAttributeId)0x0008;

        /// <summary>
        /// The BluetoothProfileDescriptorList attribute consists of a data element
        /// sequence in which each element is a profile descriptor that contains information
        /// about a Bluetooth profile to which the service represented by this service
        /// record conforms.
        /// [0x0009]
        /// </summary>
        ///<remarks>
        /// <para>[<c>Data Element Sequence</c>]</para>
        /// <para>&#x201C;
        /// The BluetoothProfileDescriptorList attribute consists of a data element
        /// sequence in which each element is a profile descriptor that contains information
        /// about a Bluetooth profile to which the service represented by this service
        /// record conforms. Each profile descriptor is a data element sequence whose
        /// first element is the UUID assigned to the profile and whose second element is
        /// a 16-bit profile version number.
        /// &#x201D;</para><para>&#x201C;
        /// Each version of a profile is assigned a 16-bit unsigned integer profile version
        /// number, which consists of two 8-bit fields. The higher-order 8 bits contain the
        /// major version number field and the lower-order 8 bits contain the minor version
        /// number field. The initial version of each profile has a major version of 1 and a
        /// minor version of 0. When upward compatible changes are made to the profile,
        /// the minor version number will be incremented. If incompatible changes are
        /// made to the profile, the major version number will be incremented.
        /// &#x201D;</para>
        /// </remarks>
        public const ServiceAttributeId BluetoothProfileDescriptorList = (ServiceAttributeId)0x0009;

        /// <summary>
        /// This attribute is a URL which points to documentation on the service described
        /// by a service record.
        /// [0x000A]
        /// </summary>
        ///<remarks>
        /// <para>[<c>URL</c>]</para>
        /// </remarks>
        public const ServiceAttributeId DocumentationUrl = (ServiceAttributeId)0x000A;

        /// <summary>
        /// This attribute contains a URL that refers to the location of an application that
        /// may be used to utilize the service described by the service record.
        /// [0x000B]
        /// </summary>
        ///<remarks>
        /// <para>[<c>URL</c>]</para>
        /// <para>&#x201C;
        /// This attribute contains a URL that refers to the location of an application that
        /// may be used to utilize the service described by the service record. Since different
        /// operating environments require different executable formats, a mechanism
        /// has been defined to allow this single attribute to be used to locate an executable
        /// that is appropriate for the client device’s operating environment. In the
        /// attribute value URL, the first byte with the value 0x2A (ASCII character ‘*’) is to
        /// be replaced by the client application with a string representing the desired
        /// operating environment before the URL is to be used.
        /// &#x201D;</para><para>&#x201C;
        /// The list of standardized strings representing operating environments is contained
        /// in the Bluetooth Assigned Numbers document.
        /// &#x201D;</para><para>&#x201C;
        /// For example, assume that the value of the ClientExecutableURL attribute is
        /// http://my.fake/public/*/client.exe. On a device capable of executing SH3 WindowsCE
        /// files, this URL would be changed to http://my.fake/public/sh3-
        /// microsoft-wince/client.exe. On a device capable of executing Windows 98 binaries,
        /// this URL would be changed to http://my.fake/public/i86-microsoft-win98/
        /// client.exe.
        /// &#x201D;</para>
        /// </remarks>
        public const ServiceAttributeId ClientExecutableUrl = (ServiceAttributeId)0x000B;

        /// <summary>
        /// This attribute contains a URL that refers to the location of an icon that may be
        /// used to represent the service described by the service record.
        /// [0x000C]
        /// </summary>
        ///<remarks>
        /// <para>[<c>URL</c>]</para>
        /// <para>&#x201C;
        /// This attribute contains a URL that refers to the location of an icon that may be
        /// used to represent the service described by the service record. Since different
        /// hardware devices require different icon formats, a mechanism has been
        /// defined to allow this single attribute to be used to locate an icon that is appropriate
        /// for the client device. In the attribute value URL, the first byte with the
        /// value 0x2A (ASCII character ‘*’) is to be replaced by the client application with
        /// a string representing the desired icon format before the URL is to be used.
        /// &#x201D;</para><para>&#x201C;
        /// The list of standardized strings representing icon formats is contained in the
        /// Bluetooth Assigned Numbers document.
        /// &#x201D;</para><para>&#x201C;
        /// For example, assume that the value of the IconURL attribute is http://my.fake/
        /// public/icons/*. On a device that prefers 24 x 24 icons with 256 colors, this URL
        /// would be changed to http://my.fake/public/icons/24x24x8.png. On a device that
        /// prefers 10 x 10 monochrome icons, this URL would be changed to http://
        /// my.fake/public/icons/10x10x1.png.
        /// &#x201D;</para>
        /// </remarks>
        public const ServiceAttributeId IconUrl = (ServiceAttributeId)0x000C;


        /// <summary>
        /// The ServiceName attribute is a string containing the name of the service represented
        /// by a service record.
        /// [0x0000 + LangBaseAttrId]
        /// </summary>
        ///<remarks>
        /// <para>[<c>String</c>]</para>
        /// <para>&#x201C;
        /// The ServiceName attribute is a string containing the name of the service represented
        /// by a service record. It should be brief and suitable for display with an
        /// Icon representing the service. The offset 0x0000 must be added to the attribute
        /// ID base (contained in the LanguageBaseAttributeIDList attribute) in order to
        /// compute the attribute ID for this attribute.
        /// &#x201D;</para>
        /// </remarks>
        [StringWithLanguageBaseAttribute]
        public const ServiceAttributeId ServiceName = (ServiceAttributeId)(short)0x0000;
        // double cast required for NETCFv1 compiler

        /// <summary>
        /// This attribute is a string containing a brief description of the service.
        /// [0x0001 + LangBaseAttrId]
        /// </summary>
        ///<remarks>
        /// <para>[<c>String</c>]</para>
        /// <para>&#x201C;
        /// This attribute is a string containing a brief description of the service. It should
        /// be less than 200 characters in length. The offset 0x0001 must be added to the
        /// attribute ID base (contained in the LanguageBaseAttributeIDList attribute) in
        /// order to compute the attribute ID for this attribute.
        /// &#x201D;</para>
        /// </remarks>
        [StringWithLanguageBaseAttribute]
        public const ServiceAttributeId ServiceDescription = (ServiceAttributeId)0x0001;

        /// <summary>
        /// This attribute is a string containing the name of the person or organization providing
        /// the service.
        /// [0x0002 + LangBaseAttrId]
        /// </summary>
        ///<remarks>
        /// <para>[<c>String</c>]</para>
        /// <para>&#x201C;
        /// This attribute is a string containing the name of the person or organization providing
        /// the service. The offset 0x0002 must be added to the attribute ID base
        /// (contained in the LanguageBaseAttributeIDList attribute) in order to compute
        /// the attribute ID for this attribute.
        /// &#x201D;</para>
        /// </remarks>
        [StringWithLanguageBaseAttribute]
        public const ServiceAttributeId ProviderName = (ServiceAttributeId)0x0002;

        /// <summary>
        /// The AdditionalProtocolDescriptorLists attribute supports services that 
        /// require more channels in addition to the service described in the ProtocolDescriptorList
        /// attribute.  It contains a sequence of ProtocolDescriptorList-elements.
        /// [0x000D]
        /// </summary>
        ///<remarks>
        /// <para>[<c>Data Element Sequence</c> or <c>Data Element Alternative</c>]</para>
        /// <para>Defined in Bluetooth version 2.1, SDP section 5.1.6.</para>
        /// <para>&#x201C;The AdditionalProtocolDescriptorLists attribute contains 
        /// a sequence of ProtocolDescriptorList-elements. Each element having the 
        /// same format as the <see cref="F:InTheHand.Net.Bluetooth.AttributeIds.UniversalAttributeId.ProtocolDescriptorList"/>
        /// described in section 5.1.5. The ordering of the elements is
        /// significant and should be specified and fixed in Profiles that make use of this
        /// attribute.</para>
        /// <para>&#x201D;The AdditionalProtocolDescriptorLists attribute supports services that require
        /// more channels in addition to the service described in Section 5.1.5 . If the AdditionalProtocolDescriptorLists
        /// attribute is included in a service record, the ProtocolDescriptorList
        /// attribute must be included.&#x201D;
        /// </para>
        /// </remarks>
        public const ServiceAttributeId AdditionalProtocolDescriptorLists = (ServiceAttributeId)0x000D;

    }//class



    /// <summary>
    /// This service class describes service records that contain attributes of service
    /// discovery server itself.
    /// </summary>
    /// <remarks>
    /// <para>&#x201C;
    /// This service class describes service records that contain attributes of service
    /// discovery server itself. The attributes listed in this section are only valid if the
    /// ServiceClassIDList attribute contains the
    /// ServiceDiscoveryServerServiceClassID. Note that all of the universal attributes
    /// may be included in service records of the ServiceDiscoveryServer class.
    /// &#x201D;</para>
    /// <para>&#x201C;Attribute IDs in the range of 0x0202-0x02FF are reserved.&#x201D;</para>
    /// </remarks>
    public
#if ! V1
 static
#endif
 class ServiceDiscoveryServerAttributeId
    {
#if V1
        private ServiceDiscoveryServerAttributeId() { }
#endif

        /// <summary>
        /// The VersionNumberList is a data element sequence in which each element of
        /// the sequence is a version number supported by the SDP server.
        /// </summary>
        ///<remarks>
        /// <para>[<c>Data Element Sequence</c>]</para>
        /// <para>&#x201C;
        /// The VersionNumberList is a data element sequence in which each element of
        /// the sequence is a version number supported by the SDP server.
        /// &#x201D;</para><para>&#x201C;
        /// A version number is a 16-bit unsigned integer consisting of two fields. The
        /// higher-order 8 bits contain the major version number field and the low-order 8
        /// bits contain the minor version number field. The initial version of SDP has a
        /// major version of 1 and a minor version of 0. When upward compatible changes
        /// are made to the protocol, the minor version number will be incremented. If
        /// incompatible changes are made to SDP, the major version number will be
        /// incremented. This guarantees that if a client and a server support a common
        /// major version number, they can communicate if each uses only features of the
        /// specification with a minor version number that is supported by both client and
        /// server.
        /// &#x201D;</para>
        /// </remarks>
        public const ServiceAttributeId VersionNumberList = (ServiceAttributeId)0x0200;

        /// <summary>
        /// The ServiceDatabaseState is a 32-bit integer that is used to facilitate caching
        /// of service records.
        /// </summary>
        ///<remarks>
        /// <para>[<c>32-bit unsigned integer</c>]</para>
        /// <para>&#x201C;
        /// The ServiceDatabaseState is a 32-bit integer that is used to facilitate caching
        /// of service records. If this attribute exists, its value is guaranteed to change
        /// when any of the other service records are added to or deleted from the server's
        /// database. If this value has not changed since the last time a client queried its
        /// value, the client knows that a) none of the other service records maintained by
        /// the SDP server have been added or deleted; and b) any service record handles
        /// acquired from the server are still valid. A client should query this attribute's
        /// value when a connection to the server is established, prior to using any service
        /// record handles acquired during a previous connection.
        /// &#x201D;</para><para>&#x201C;
        /// Note that the ServiceDatabaseState attribute does not change when existing
        /// service records are modified, including the addition, removal, or modification of
        /// service attributes. A service record's ServiceRecordState attribute indicates
        /// when that service record is modified.
        /// &#x201D;</para>
        /// </remarks>
        public const ServiceAttributeId ServiceDatabaseState = (ServiceAttributeId)0x0201;
    }//class



    /// <summary>
    /// This service class describes the ServiceRecord provided for each BrowseGroupDescriptor
    ///  service offered on a Bluetooth device.
    /// </summary>
    /// <remarks>
    /// <para>&#x201C;
    /// This service class describes the ServiceRecord provided for each BrowseGroupDescriptor
    ///  service offered on a Bluetooth device. The attributes listed in
    /// this section are only valid if the ServiceClassIDList attribute contains the BrowseGroupDescriptorServiceClassID.
    /// Note that all of the universal attributes may
    /// be included in service records of the BrowseGroupDescriptor class.
    /// &#x201D;</para>
    /// <para>&#x201C;Attribute IDs in the range of 0x0201-0x02FF are reserved.&#x201D;</para>
    /// </remarks>
    public
#if ! V1
 static
#endif
 class BrowseGroupDescriptorAttributeId
    {
#if V1
        private BrowseGroupDescriptorAttributeId() { }
#endif

        /// <summary>
        /// This attribute contains a UUID that can be used to locate services that are
        /// members of the browse group that this service record describes.
        /// </summary>
        ///<remarks>
        /// <para>[<c>UUID</c>]</para>
        /// </remarks>
        public const ServiceAttributeId GroupId = (ServiceAttributeId)0x0200;
    }//class

}
