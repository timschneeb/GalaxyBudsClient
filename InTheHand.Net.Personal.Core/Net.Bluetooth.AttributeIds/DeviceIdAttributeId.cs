using InTheHand.Net.Bluetooth;

namespace InTheHand.Net.Bluetooth.AttributeIds
{
    /// <summary>
    /// Service Attribute IDs defined by the Device Identification Profile specification.
    /// </summary>
    /// -
    /// <remarks>
    /// <para>&#x201C;This document specifies a method by which Bluetooth devices may
    /// provide information that may be used by peer Bluetooth devices to
    /// find representative icons or load associated support software. This
    /// information is published as Bluetooth SDP records, and optionally in
    /// an Extended Inquiry Response.&#x201D;
    /// </para>
    /// <para>Used in records with Service Class ID:
    /// <see cref="F:InTheHand.Net.Bluetooth.BluetoothService.PnPInformation"/>.
    /// </para>
    /// <para>As well as the attributes defined here, use of some of the universal
    /// attributes is recommended, they are:
    /// <see cref="F:InTheHand.Net.Bluetooth.AttributeIds.UniversalAttributeId.ClientExecutableUrl"/>,
    /// <see cref="F:InTheHand.Net.Bluetooth.AttributeIds.UniversalAttributeId.ServiceDescription"/>,
    /// and <see cref="F:InTheHand.Net.Bluetooth.AttributeIds.UniversalAttributeId.DocumentationUrl"/>.
    /// </para>
    /// </remarks>
    public static class DeviceIdProfileAttributeId
    {
        /// <summary>
        /// SpecificationId [0x0200]
        /// </summary>
        /// <remarks>The version of the Bluetooth Device ID Profile Specification
        /// supported by the device.
        /// e.g. version 1.3 will be value 0x0103. [<c>UInt16</c>]</remarks>
        public const ServiceAttributeId SpecificationId = (ServiceAttributeId)0x0200;

        /// <summary>
        /// VendorId [0x0201]
        /// </summary>
        /// <remarks>
        /// <para>The id assigned by the organisation in <see cref="F:InTheHand.Net.Bluetooth.AttributeIds.DeviceIdProfileAttributeId.VendorIdSource" />. [<c>UInt16</c>]
        /// </para>
        /// <para>&#x201C;The value <c>FFFF</c> is reserved as the default id when
        /// no Device ID Service Record is present in the device.&#x201D;
        /// </para>
        /// </remarks>
        public const ServiceAttributeId VendorId = (ServiceAttributeId)0x0201;

        /// <summary>
        /// ProductId [0x0202]
        /// </summary>
        /// <remarks>Distinguishes between different products made by the same vendor. [<c>UInt16</c>]</remarks>
        public const ServiceAttributeId ProductId = (ServiceAttributeId)0x0202;

        /// <summary>
        /// Version [0x0203]
        /// </summary>
        /// <remarks>The version of the product. [<c>UInt16</c>]</remarks>
        public const ServiceAttributeId Version = (ServiceAttributeId)0x0203;

        /// <summary>
        /// PrimaryRecord [0x0204]
        /// </summary>
        /// <remarks>If multiple Device ID records are present this indicates the one &#x2019;primary&#x201A; record. [<c>Boolean</c>]</remarks>
        public const ServiceAttributeId PrimaryRecord = (ServiceAttributeId)0x0204;

        /// <summary>
        /// VendorIdSource [0x0205]
        /// </summary>
        /// <remarks>Designates which organisation assigned the Vendor ID value. [<c>UInt16</c>]
        /// <list type="table">
        /// <listheader><term>Value</term><term>Assigning Organisation</term></listheader>
        /// <item><term>1</term><term>Bluetooth SIG</term></item>
        /// <item><term>2</term><term>USB Implementors Forum</term></item>
        /// <item><term>0, 3-FFFF</term><term>reserved</term></item>
        /// </list>
        /// </remarks>
        public const ServiceAttributeId VendorIdSource = (ServiceAttributeId)0x0205;

        // Supplied by Universal
        //public const ServiceAttributeId ClientExecutableUrl = (ServiceAttributeId)0x000B;
        //public const ServiceAttributeId ServiceDescription = (ServiceAttributeId)0x0001;
        //public const ServiceAttributeId DocumentationUrl = (ServiceAttributeId)0x000A;

    }//class
}