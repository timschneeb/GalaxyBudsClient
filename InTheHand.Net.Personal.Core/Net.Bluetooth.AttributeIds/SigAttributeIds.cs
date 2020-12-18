using System;

namespace InTheHand.Net.Bluetooth.AttributeIds
{

    /// <summary>
    /// Service Attribute IDs defined by the OBEX related specifications,
    /// i.e. Object Push and Synchronization Profiles specifications.
    /// </summary>
    public
#if ! V1
 static
#endif
 class ObexAttributeId
    {
#if V1
        private ObexAttributeId() { }
#endif

        /// <summary>
        /// GOEP L2Cap PSM
        /// </summary>
        /// <remarks>
        /// New in GOEP v2.0 but not numbered there.
        /// New in OPP v1.2, FTP v1.2, and BIP v1.1.
        /// <para>[<c>UInt16</c>]</para>
        /// </remarks>
        public const ServiceAttributeId GoepL2capPsm = (ServiceAttributeId)0x0200;

        /// <summary>
        /// Supported Data Stores List (Synchronization Profile)
        /// </summary>
        /// <remarks>
        /// Synchronization Profile &#x2014; 
        /// <see cref="F:InTheHand.Net.Bluetooth.BluetoothService.IrMCSync"/> 
        /// service class.
        /// <para>[<c>Data Element Sequence of UInt8</c>]</para>
        /// <list type="table">
        /// Values
        /// <listheader><term>Value</term><description>Meaning</description></listheader>
        /// <item><term>0x01</term><description>Phonebook</description></item>
        /// <item><term>0x03</term><description>Calendar</description></item>
        /// <item><term>0x05</term><description>Notes</description></item>
        /// <item><term>0x06</term><description>Message</description></item>
        /// </list>
        /// </remarks>
        public const ServiceAttributeId SupportedDataStoresList = (ServiceAttributeId)0x0301;

        /// <summary>
        /// Supported Formats List (Object Push Profile)
        /// </summary>
        /// <remarks>
        /// Object Push Profile &#x2014; 
        /// <see cref="F:InTheHand.Net.Bluetooth.BluetoothService.ObexObjectPush"/> 
        /// service class.
        /// <para>[<c>Data Element Sequence of UInt8</c>]</para>
        /// <list type="table">
        /// Values
        /// <listheader><term>Value</term><description>Meaning</description></listheader>
        /// <item><term>0x01</term><description>vCard 2.1</description></item>
        /// <item><term>0x02</term><description>vCard 3.0</description></item>
        /// <item><term>0x03</term><description>vCard 2.1</description></item>
        /// <item><term>0x04</term><description>vCal 1.0</description></item>
        /// <item><term>0x05</term><description>vNote</description></item>
        /// <item><term>0x06</term><description>vMessage</description></item>
        /// <item><term>0xFF</term><description>any type of object</description></item>
        /// </list>
        /// </remarks>
        public const ServiceAttributeId SupportedFormatsList = (ServiceAttributeId)0x0303;


        // -- BIP --

        /// <summary>
        /// Supported Capabilities (BIP)
        /// </summary>
        /// <remarks>
        /// Basic Imaging Profile &#x2014; 
        /// <see cref="F:InTheHand.Net.Bluetooth.BluetoothService.Imaging"/>, 
        /// <see cref="F:InTheHand.Net.Bluetooth.BluetoothService.ImagingResponder"/>, 
        /// <see cref="F:InTheHand.Net.Bluetooth.BluetoothService.ImagingAutomaticArchive"/>, 
        /// <see cref="F:InTheHand.Net.Bluetooth.BluetoothService.ImagingReferenceObjects"/> 
        /// service classes.
        /// <para>[<c>UInt8</c>]</para>
        /// <list type="table">
        /// Values
        /// <listheader><term>Value</term><description>Meaning</description></listheader>
        /// <item><term>Bit 0</term><description>Generic imaging</description></item>
        /// <item><term>Bit 1</term><description>Capturing</description></item>
        /// <item><term>Bit 2</term><description>Printing</description></item>
        /// <item><term>Bit 3</term><description>Displaying</description></item>
        /// <item><term>Bit 4..7</term><description>Reserved</description></item>
        /// </list>
        /// </remarks>
        public const ServiceAttributeId SupportedCapabilities = (ServiceAttributeId)0x0310;

        /// <summary>
        /// Supported Features (BIP)
        /// </summary>
        /// <remarks>
        /// Basic Imaging Profile &#x2014; 
        /// <see cref="F:InTheHand.Net.Bluetooth.BluetoothService.Imaging"/>, 
        /// <see cref="F:InTheHand.Net.Bluetooth.BluetoothService.ImagingResponder"/>, 
        /// <see cref="F:InTheHand.Net.Bluetooth.BluetoothService.ImagingAutomaticArchive"/>, 
        /// <see cref="F:InTheHand.Net.Bluetooth.BluetoothService.ImagingReferenceObjects"/> 
        /// service classes.
        /// <para>[<c>UInt16</c>]</para>
        /// <list type="table">
        /// Values
        /// <listheader><term>Value</term><description>Meaning</description></listheader>
        /// <item><term>Bit 0</term><description>ImagePush</description></item>
        /// <item><term>Bit 1</term><description>ImagePush-Store</description></item>
        /// <item><term>Bit 2</term><description>ImagePush-Print</description></item>
        /// <item><term>Bit 3</term><description>ImagePush-Display</description></item>
        /// <item><term>Bit 4</term><description>ImagePull</description></item>
        /// <item><term>Bit 5</term><description>AdvancedImagePrinting</description></item>
        /// <item><term>Bit 6</term><description>AutomaticArchive</description></item>
        /// <item><term>Bit 7</term><description>RemoteCamera</description></item>
        /// <item><term>Bit 8</term><description>RemoteDisplay</description></item>
        /// <item><term>Bit 9..15</term><description>Reserved</description></item>
        /// </list>
        /// </remarks>
        public const ServiceAttributeId SupportedFeatures = (ServiceAttributeId)0x0311;

        /// <summary>
        /// Supported Functions (BIP)
        /// </summary>
        /// <remarks>
        /// Basic Imaging Profile &#x2014; 
        /// <see cref="F:InTheHand.Net.Bluetooth.BluetoothService.Imaging"/>, 
        /// <see cref="F:InTheHand.Net.Bluetooth.BluetoothService.ImagingResponder"/>, 
        /// <see cref="F:InTheHand.Net.Bluetooth.BluetoothService.ImagingAutomaticArchive"/>, 
        /// <see cref="F:InTheHand.Net.Bluetooth.BluetoothService.ImagingReferenceObjects"/> 
        /// service classes.
        /// <para>[<c>UInt32</c>]</para>
        /// <list type="table">
        /// Values
        /// <listheader><term>Value</term><description>Meaning</description></listheader>
        /// <item><term>Bit 0</term><description>GetCapabilities</description></item>
        /// <item><term>Bit 1</term><description>PutImage</description></item>
        /// <item><term>Bit 2</term><description>PutLinkedAttachment</description></item>
        /// <item><term>Bit 3</term><description>PutLinkedThumbnail</description></item>
        /// <item><term>Bit 4</term><description>RemoteDisplay</description></item>
        /// <item><term>Bit 5</term><description>GetImagesList</description></item>
        /// <item><term>Bit 6</term><description>GetImageProperties</description></item>
        /// <item><term>Bit 7</term><description>GetImage</description></item>
        /// <item><term>Bit 8</term><description>GetLinkedThumbnail</description></item>
        /// <item><term>Bit 9</term><description>GetLinkedAttachment</description></item>
        /// <item><term>Bit 10</term><description>DeleteImage</description></item>
        /// <item><term>Bit 11</term><description>StartPrint</description></item>
        /// <item><term>Bit 12</term><description>Reserved</description></item>
        /// <item><term>Bit 13</term><description>StartArchive</description></item>
        /// <item><term>Bit 14</term><description>GetMonitoringImage</description></item>
        /// <item><term>Bit 16</term><description>GetStatus</description></item>
        /// <item><term>Bit 15, 17..31</term><description>Reserved</description></item>
        /// </list>
        /// </remarks>
        public const ServiceAttributeId SupportedFunctions = (ServiceAttributeId)0x0312;

        /// <summary>
        /// Total Imaging Data Capacity (BIP)
        /// </summary>
        /// <remarks>
        /// Basic Imaging Profile &#x2014; 
        /// <see cref="F:InTheHand.Net.Bluetooth.BluetoothService.Imaging"/>, 
        /// <see cref="F:InTheHand.Net.Bluetooth.BluetoothService.ImagingResponder"/>, 
        /// <see cref="F:InTheHand.Net.Bluetooth.BluetoothService.ImagingAutomaticArchive"/>, 
        /// <see cref="F:InTheHand.Net.Bluetooth.BluetoothService.ImagingReferenceObjects"/> 
        /// service classes.
        /// <para>[<c>UInt64</c>]</para>
        /// </remarks>
        public const ServiceAttributeId TotalImagingDataCapacity = (ServiceAttributeId)0x0313;

    }//class


    /// <summary>
    /// Service Attribute IDs defined by the Basic Printing Profile specification.
    /// </summary>
    public
#if ! V1
 static
#endif
 class BasicPrintingProfileAttributeId
    {
#if V1
        private BasicPrintingProfileAttributeId() { }
#endif
        /// <summary>
        /// Document Formats Supported
        /// </summary>
        /// <remarks>[<c>String</c>]</remarks>
        public const ServiceAttributeId DocumentFormatsSupported = (ServiceAttributeId)0x0350;

        /// <summary>
        /// Character Repertoires Supported
        /// </summary>
        /// <remarks>[<c>UInt128</c>]</remarks>
        public const ServiceAttributeId CharacterRepertoiresSupported = (ServiceAttributeId)0x0352;

        /// <summary>
        /// XHTML-Print Image Formats Supported
        /// </summary>
        /// <remarks>[<c>String</c>]</remarks>
        public const ServiceAttributeId XhtmlPrintImageFormatsSupported = (ServiceAttributeId)0x0354;

        /// <summary>
        /// Color Supported
        /// </summary>
        /// <remarks>[<c>Boolean</c>]</remarks>
        public const ServiceAttributeId ColorSupported = (ServiceAttributeId)0x0356;

        /// <summary>
        /// 1284ID
        /// </summary>
        /// <remarks>[<c>String</c>]</remarks>
        public const ServiceAttributeId Model1284Id = (ServiceAttributeId)0x0358;

        /// <summary>
        /// Printer Name
        /// </summary>
        /// <remarks>[<c>String</c>]</remarks>
        public const ServiceAttributeId PrinterName = (ServiceAttributeId)0x035A;

        /// <summary>
        /// Printer Location
        /// </summary>
        /// <remarks>[<c>String</c>]</remarks>
        public const ServiceAttributeId PrinterLocation = (ServiceAttributeId)0x035C;

        /// <summary>
        /// Duplex Supported
        /// </summary>
        /// <remarks>[<c>Boolean</c>]</remarks>
        public const ServiceAttributeId DuplexSupported = (ServiceAttributeId)0x035E;

        /// <summary>
        /// Media Types Supported
        /// </summary>
        /// <remarks>[<c>String</c>]</remarks>
        public const ServiceAttributeId MediaTypesSupported = (ServiceAttributeId)0x0360;

        /// <summary>
        /// MaxMediaWidth
        /// </summary>
        /// <remarks>[<c>UInt16</c>]</remarks>
        public const ServiceAttributeId MaxMediaWidth = (ServiceAttributeId)0x0362;

        /// <summary>
        /// MaxMediaLength
        /// </summary>
        /// <remarks>[<c>UInt16</c>]</remarks>
        public const ServiceAttributeId MaxMediaLength = (ServiceAttributeId)0x0364;

        /// <summary>
        /// Enhanced Layout Supported
        /// </summary>
        /// <remarks>[<c>Boolean</c>]</remarks>
        public const ServiceAttributeId EnhancedLayoutSupported = (ServiceAttributeId)0x0366;

        /// <summary>
        /// RUI Formats Supported
        /// </summary>
        /// <remarks>[<c>String</c>]</remarks>
        public const ServiceAttributeId RuiFormatsSupported = (ServiceAttributeId)0x0368;

        /// <summary>
        /// Reference Printing RUI Supported
        /// </summary>
        /// <remarks>[<c>Boolean</c>]</remarks>
        public const ServiceAttributeId ReferencePrintingRuiSupported = (ServiceAttributeId)0x0370;

        /// <summary>
        /// Direct Printing RUI Supported
        /// </summary>
        /// <remarks>[<c>Boolean</c>]</remarks>
        public const ServiceAttributeId DirectPrintingRuiSupported = (ServiceAttributeId)0x0372;

        /// <summary>
        /// Reference Printing Top URL
        /// </summary>
        /// <remarks>[<c>URL</c>]</remarks>
        public const ServiceAttributeId ReferencePrintingTopUrl = (ServiceAttributeId)0x0374;

        /// <summary>
        /// Direct Printing Top URL
        /// </summary>
        /// <remarks>[<c>URL</c>]</remarks>
        public const ServiceAttributeId DirectPrintingTopUrl = (ServiceAttributeId)0x0376;

        /// <summary>
        /// Printer Admin RUI Top URL
        /// </summary>
        /// <remarks>[<c>URL</c>]</remarks>
        public const ServiceAttributeId PrinterAdminRuiTopUrl = (ServiceAttributeId)0x0378;

        /// <summary>
        /// Device Name
        /// </summary>
        /// <remarks>[<c>String</c>]</remarks>
        public const ServiceAttributeId DeviceName = (ServiceAttributeId)0x037A;
    }//class


    /// <summary>
    /// Service Attribute IDs defined by the Personal Area Networking Profile specification.
    /// </summary>PersonalAreaNetworkingProfile
    public
#if ! V1
 static
#endif
 class PersonalAreaNetworkingProfileAttributeId
    {
#if V1
        private PersonalAreaNetworkingProfileAttributeId() { }
#endif
        /// <summary>
        /// Security Description
        /// </summary>
        /// <remarks>&#x201C;Security Description&#x201D; [<c>UInt16</c>]</remarks>
        public const ServiceAttributeId SecurityDescription = (ServiceAttributeId)0x030A;

        /// <summary>
        /// NetAccessType
        /// </summary>
        /// <remarks>&#x201C;Type of Network Access Available&#x201D; [<c>UInt16</c>]</remarks>
        public const ServiceAttributeId NetAccessType = (ServiceAttributeId)0x030B;

        /// <summary>
        /// MaxNetAccessRate
        /// </summary>
        /// <remarks>&#x201C;Maximum possible Network Access Data Rate&#x201D; [<c>UInt32</c>]</remarks>
        public const ServiceAttributeId MaxNetAccessRate = (ServiceAttributeId)0x030C;

        /// <summary>
        /// IPv4Subnet
        /// </summary>
        /// <remarks>[<c>String</c>]</remarks>
        public const ServiceAttributeId IPv4Subnet = (ServiceAttributeId)0x030D;

        /// <summary>
        /// IPv6Subnet
        /// </summary>
        /// <remarks>[<c>String</c>]</remarks>
        public const ServiceAttributeId IPv6Subnet = (ServiceAttributeId)0x030E;
    }//class


    /// <summary>
    /// Service Attribute IDs defined by the Headset Profile specification.
    /// </summary>
    public
#if ! V1
 static
#endif
 class HeadsetProfileAttributeId
    {
#if V1
        private HeadsetProfileAttributeId() { }
#endif
        /// <summary>
        /// Remote audio volume control
        /// </summary>
        /// <remarks>[<c>Boolean</c>]</remarks>
        public const ServiceAttributeId RemoteAudioVolumeControl = (ServiceAttributeId)0x0302;
    }//class


    /// <summary>
    /// Service Attribute IDs defined by the Hand-Free Profile specification.
    /// </summary>HandFreeProfile
    public
#if ! V1
 static
#endif
 class HandsFreeProfileAttributeId
    {
#if V1
        private HandsFreeProfileAttributeId() { }
#endif

        //moved from above
        //[AttributeIdsOfServiceClass(HandsFreeProfileAttributeId.Handsfree)]
        //[AttributeIdsOfServiceClass(HandsFreeProfileAttributeId.GenericAudio)]
        //[AttributeIdsOfServiceClass(HandsFreeProfileAttributeId.HandsfreeAudioGateway)]


        /// <summary>
        /// Network
        /// </summary>
        /// <remarks>
        /// <para>&#x201C;The "Network" attribute states, if the AG has the capability 
        /// to reject incoming calls[4]. This attribute is not encoded as a data element 
        /// sequence; it is simply an 8-bit unsigned integer. The information given 
        /// in the “Network” attribute shall be the same as the information given 
        /// in Bit 5 of the unsolicited result code +BRSF (see Section 4.24.3). An 
        /// attribute value of 0x00 is translated to a bit value of 0; an attribute 
        /// value of 0x01 is translated to a bit value of 1.&#x201D;
        /// </para>
        /// [<c>UInt8</c>]
        /// </remarks>
        public const ServiceAttributeId Network = (ServiceAttributeId)0x301;

        /// <summary>
        /// SupportedFeatures
        /// </summary>
        /// <remarks>
        /// <para>&#x201C;The attribute &#x201C;SupportedFeatures&#x201D; states the features 
        /// supported in each device. &#x2026;
        /// The set of features supported in each case is bit-wise defined in this 
        /// attribute on a yes/no basis. The mapping between the features and their 
        /// corresponding bits within the attribute is listed below in for the HF 
        /// and in for the AG. &#x2026;
        /// <code lang="none">
        /// Bit     Feature                                                     Default in HF
        /// (0=LSB)
        /// 0       EC and/or NR function (yes/no, 1 = yes, 0 = no)             0
        /// 1       Call waiting and three way calling(yes/no, 1 = yes, 0 = no) 0
        /// 2       CLI presentation capability (yes/no, 1 = yes, 0 = no)       0
        /// 3       Voice recognition activation (yes/no, 1= yes, 0 = no)       0
        /// 4       Remote volume control (yes/no, 1 = yes, 0 = no)             0
        /// </code>
        /// <para>Table 5.2 “SupportedFeatures” attribute bit mapping for the HF</para>
        /// <code lang="none">
        /// Bit     Feature                                             Default in AG
        /// (0=LSB)
        /// 0       Three-way calling (yes/no, 1 = yes, 0 = no)         1
        /// 1       EC and/or NR function (yes/no, 1 = yes, 0 = no)     0
        /// 2       Voice recognition function (yes/no, 1 = yes, 0 = no)    0
        /// 3       In-band ring tone capability (yes/no, 1 = yes, 0 = no)  1
        /// 4       Attach a phone number to a voice tag (yes/no, 1 = yes, 0 = no)  0
        /// </code>
        /// Table 5.4 “SupportedFeatures” attribute bit mapping for the AG&#x201D;
        /// </para>
        /// [<c>UInt16</c>]</remarks>
        public const ServiceAttributeId SupportedFeatures = (ServiceAttributeId)0x0311;

        //internal const Int16 Handsfree = 0x111E;
        //internal const Int16 GenericAudio = 0x1203;
        //internal const Int16 HandsfreeAudioGateway = 0x111F;
    }//class

    /// <summary>
    /// Service Attribute IDs defined by the Health Device Profile specification.
    /// </summary>
public
#if ! V1
 static
#endif
 class HealthDeviceAttributeId
    {
#if V1
    private HealthDeviceAttributeId() { }
#endif
    //
        /// <summary>
        /// SupportFeaturesList
        /// </summary>
        /// -
        /// <remarks>
        /// <para>"This is a sequence for which each element is a sequence that
        /// describes a single application data end-point on the device. The
        /// Supported Features attribute (MDEP List) provides an indication of
        /// the data types that an MDEP supports.",
        /// "...each description is itself a sequence of three or more elements."
        /// </para>
        /// <c>[Sequence]</c>
        /// </remarks>
        public const ServiceAttributeId SupportFeaturesList = (ServiceAttributeId)0x0200;
        /// <summary>
        /// DataExchangeSpecification
        /// </summary>
        /// -
        /// <remarks>
        /// <para>"This attribute is a one-byte reference, with the value taken
        /// from the Bluetooth Assigned Numbers [3] to identify the Data Exchange
        /// Protocol used (e.g. ISO/IEEE 11073-20601 specification)."
        /// e.g. value 0x01 is ISO/IEEE 11073-20601, "Health informatics - Personal
        /// health device communication - Application profile - Optimized exchange
        /// protocol"
        /// </para>
        /// <c>[UInt8]</c>
        /// </remarks>
        public const ServiceAttributeId DataExchangeSpecification = (ServiceAttributeId)0x0301;
        /// <summary>
        /// MCAP Supported Procedures
        /// </summary>
        /// -
        /// <remarks>
        /// <para>"This attribute is a one byte bit-mask that indicates the MCAP
        /// procedures that are supported by this HDP service."
        /// </para>
        /// <code lang="none">
        /// 0x02  Supports Reconnect Initiation 3
        /// 0x04  Supports Reconnect Acceptance 4
        /// 0x08  Supports Clock Synchronization Protocol (includes support for at least Sync-Slave Role)
        /// 0x10  Supports Sync-Master Role
        /// </code>
        /// <c>[UInt8]</c>
        /// </remarks>
        public const ServiceAttributeId McapSupportedProcedures = (ServiceAttributeId)0x0302;
    }
}
