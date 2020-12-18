namespace InTheHand.Net.Bluetooth.AttributeIds
{

    /// <summary>
    /// Service Attribute IDs defined by the Human Interface Device (HID) Profile specification.
    /// </summary>
    public
#if ! V1
    static
#endif
    class HidProfileAttributeId
    {
#if V1
       private HidProfileAttributeId() { }
#endif

        /// <summary>
        /// HIDDeviceReleaseNumber
        /// </summary>
        /// <remarks><para>[<c>16-bit unsigned integer</c>]</para>
        /// <para>
        /// &#x201C;A numeric expression identifying the device release number in Binary-Coded 
        /// Decimal. This is a vendor-assigned field, which defines the version of 
        /// the product identified by the Bluetooth Device Identification [13] VendorID 
        /// and ProductID attributes. This attribute is intended to differentiate 
        /// between versions of products with identical VendorIDs and ProductIDs. 
        /// The value of the field is 0xJJMN for version JJ.M.N (JJ – major version 
        /// number, M – minor version number, N – sub-minor version number). &#x2026;&#x201D;
        /// </para>
        /// </remarks>
        public const ServiceAttributeId DeviceReleaseNumber = (ServiceAttributeId)0x0200;

        /// <summary>
        /// HIDParserVersion
        /// </summary>
        /// <remarks><para>[<c>16-bit unsigned integer</c>]</para>
        /// <para>
        /// &#x201C;Each version of a profile is assigned a 16-bit unsigned integer version
        /// number of the base HID Specification [4] that the device was designed to. The value
        /// of the field is 0xJJMN for version JJ.M.N &#x2026;&#x201D;
        /// </para>
        /// </remarks>
        public const ServiceAttributeId ParserVersion = (ServiceAttributeId)0x0201;

        /// <summary>
        /// HIDDeviceSubclass
        /// </summary>
        /// <remarks><para>[<c>8-bit unsigned integer</c>]</para>
        /// <para>
        /// &#x201C;The HIDDeviceSubclass attribute is an 8-bit integer, which
        /// identifies the type of device (keyboard, mouse, joystick, gamepad,
        /// remote control, sensing device, etc.). Keyboards and mice are required
        /// to support boot mode operation. In boot mode, a device presents a fixed
        /// report, thus negating the requirement for a HID parser.
        /// <para></para>The Attribute value is identical to the low-order 8 bits
        /// of the Class of Device/Service (CoD) field in the FHS packet, where
        /// bits 7-2 contain the 6 bit Minor Device Class value (defined in Section
        /// 1.2 of the Bluetooth Assigned Numbers document [8]) and bits 1-0 are
        /// set to zero.  &#x2026;&#x201D;
        /// </para>
        /// </remarks>
        public const ServiceAttributeId DeviceSubclass = (ServiceAttributeId)0x0202;

        /// <summary>
        /// HIDCountryCode
        /// </summary>
        /// <remarks><para>[<c>8-bit unsigned integer</c>]</para>
        /// <para>
        /// &#x201C;The HIDCountryCode attribute is an 8-bit integer, which identifies
        /// which country the hardware is localized for. Most hardware is not localized
        /// and thus this value would be zero (0).&#x2026; 
        /// </para><para>The valid country codes are listed in the HID Specification
        /// [4].&#x201D;
        /// </para>
        /// </remarks>
        public const ServiceAttributeId CountryCode = (ServiceAttributeId)0x0203;

        /// <summary>
        /// HIDVirtualCable
        /// </summary>
        /// <remarks><para>[<c>8-bit Boolean</c>]</para>
        /// <para>
        /// &#x201C;The HIDVirtualCable attribute is a boolean value, which indicates
        /// whether the device supports virtual connections as described in Section
        /// Virtual Cables and Connection Re-Establishment. Devices that have this
        /// attribute True indicate that the device supports 1:1 bonding with a host,
        /// and the device expects to automatically reconnect if the connection is
        /// dropped for any unknown reason.&#x201D;
        /// </para>
        /// </remarks>
        public const ServiceAttributeId VirtualCable = (ServiceAttributeId)0x0204;

        /// <summary>
        /// HIDReconnectInitiate
        /// </summary>
        /// <remarks><para>[<c>8-bit Boolean</c>]</para>
        /// <para>
        /// &#x201C;The HIDReconnectInitiate attribute is a boolean value, which
        /// indicates whether the device initiates the reconnection process or
        /// expects the host to. &#x2026;&#x201D;
        /// </para>
        /// </remarks>
        public const ServiceAttributeId ReconnectInitiate = (ServiceAttributeId)0x0205;

        /// <summary>
        /// HIDDescriptorList
        /// </summary>
        /// <remarks><para>[<c>Data element sequence</c>]</para>
        /// <para>
        /// &#x201C;The HIDDescriptorList Data Element Sequence performs the function of the
        /// HID Descriptor that is defined in Section 6.2 of the HID Specification [4]. The
        /// HIDDescriptorList identifies the descriptors associated with the device. &#x2026;
        /// </para><para>The HIDDescriptorList is a Data Element Sequence that consists of
        /// one or more HIDDescriptors. A HIDDescriptor is a data element sequence containing, 
        /// minimally, a pair of elements. For compatibility with future versions of the HID
        /// profile, addition elements found in a HIDDescriptor shall be ignored. &#x2026;
        ///        &#x201D;
        /// </para>
        /// </remarks>
        public const ServiceAttributeId DescriptorList = (ServiceAttributeId)0x0206;

        /// <summary>
        /// HIDLANGIDBaseList
        /// </summary>
        /// <remarks><para>[<c>Data element sequence</c>]</para>
        /// <para>
        /// &#x201C;The HIDLANGIDBaseList is a Data Element Sequence that consists of one or
        /// more HIDLANGIDBases. A HIDLANGIDBase is a data element sequence containing, minimally, 
        /// two elements for each of the languages used in the service record: a language identifier
        /// (LANGID) and a base attribute ID. For compatibility with future versions of the
        /// HID profile, additional elements found in a HIDLANGIDBase shall be ignored.
        /// </para><para>The first element, called the HIDLANGID, contains an identifier representing
        /// the natural language ID. The language is encoded according to the “Universal Serial
        /// Bus Language Identifiers (LANGIDs)” Specification [9].
        /// </para><para>The second element, called the HIDLanguageBase, contains an attribute
        /// ID that serves as the base attribute ID for the natural language in the service
        /// record. Different service records within a server may use different base attribute
        /// ID values for the same language. &#x2026;&#x201D;
        /// </para>
        /// </remarks>
        public const ServiceAttributeId LangIdBaseList = (ServiceAttributeId)0x0207;

        /// <summary>
        /// HIDSDPDisable
        /// </summary>
        /// <remarks><para>[<c>8-bit Boolean</c>]</para>
        /// <para>
        /// &#x201C;The HIDSDPDisable attribute is a boolean value, which indicates whether
        /// connection to the SDP channel and Control or Interrupt channels are mutually exclusive.
        /// &#x2026;&#x201D;
        /// </para>
        /// </remarks>
        public const ServiceAttributeId SdpDisable = (ServiceAttributeId)0x0208;

        /// <summary>
        /// HIDBatteryPower
        /// </summary>
        /// <remarks><para>[<c>8-bit Boolean</c>]</para>
        /// <para>
        /// &#x201C;The HIDBatteryPower attribute is a boolean value, which indicates whether
        /// the device is battery powered (and requires careful power management) or has some
        /// other source of power that requires minimal management. &#x2026;&#x201D;
        /// </para>
        /// </remarks>
        public const ServiceAttributeId BatteryPower = (ServiceAttributeId)0x0209;

        /// <summary>
        /// HIDRemoteWake
        /// </summary>
        /// <remarks><para>[<c>8-bit Boolean</c>]</para>
        /// <para>
        /// &#x201C;The HIDRemoteWake attribute is a boolean value, which indicates whether
        /// the device considers itself remote wake up-capable. When a system enters a suspend
        /// (or standby) state, this flag shall be used to determine whether the host includes
        /// this device in the set of devices that can wake it up. A mouse or keyboard are
        /// typical examples of Remote Wake up devices.&#x201D;
        /// </para>
        /// </remarks>
        public const ServiceAttributeId RemoteWake = (ServiceAttributeId)0x020A;

        /// <summary>
        /// HIDBootDevice
        /// </summary>
        /// <remarks><para>[<c>8-bit Boolean</c>]</para>
        /// <para>
        /// &#x201C;HIDBootDevice is an 8-bit Boolean value that when True indicates whether
        /// the device supports boot protocol mode and by inference the Set_Protocol and Get_Protocol
        /// commands. &#x2026;&#x201D;
        /// </para>
        /// </remarks>
        public const ServiceAttributeId BootDevice = (ServiceAttributeId)0x020E;

        /// <summary>
        /// HIDSupervisionTimeout
        /// </summary>
        /// <remarks><para>[<c>16-bit unsigned integer</c>]</para>
        /// <para>
        /// &#x201C;The HIDSupervisionTimeout is a 16-bit value which indicates the device
        /// vendor’s recommended baseband Link Supervision Timeout value in slots. &#x2026;&#x201D;
        /// </para>
        /// </remarks>
        public const ServiceAttributeId SupervisionTimeout = (ServiceAttributeId)0x020C;

        /// <summary>
        /// HIDNormallyConnectable
        /// </summary>
        /// <remarks><para>[<c>8-bit Boolean</c>]</para>
        /// <para>
        /// &#x201C;HIDNormallyConnectable is an optional Boolean attribute that specifies
        /// whether a HID is normally in Page Scan mode (when no connection is active) or not.
        /// &#x2026;&#x201D;
        /// </para>
        /// </remarks>
        public const ServiceAttributeId NormallyConnectable = (ServiceAttributeId)0x020D;

        /// <summary>
        /// HIDProfileVersion
        /// </summary>
        /// <remarks><para>[<c>16-bit unsigned integer</c>]</para>
        /// <para>
        /// &#x201C;Each device designed to this specification shall include a 16-bit unsigned
        /// integer version number of the Bluetooth HID Specification (this document) that
        /// the device was designed to. The value of the field is 0xJJMN for version JJ.M.N
        /// (JJ – major version number, M – minor version number, N – sub-minor version number);
        /// &#x2026;&#x201D;
        /// </para>
        /// </remarks>
        public const ServiceAttributeId ProfileVersion = (ServiceAttributeId)0x020B;

    }//class

}