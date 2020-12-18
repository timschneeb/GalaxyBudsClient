using System;
using System.Diagnostics.CodeAnalysis;

namespace InTheHand.Net.Bluetooth
{
    /// <summary>
    /// HCI_Version &#x2014; Assigned Numbers &#x2014; Host Controller Interface
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32")]
    public enum HciVersion : byte
    {
        /// <summary>
        /// Bluetooth Core Specification 1.0b
        /// </summary>
        v1_0_b = 0,
        /// <summary>
        /// Bluetooth Core Specification 1.1
        /// </summary>
        v1_1 = 1,
        /// <summary>
        /// Bluetooth Core Specification 1.2
        /// </summary>
        v1_2 = 2,
        /// <summary>
        /// Bluetooth Core Specification 2.0 + EDR
        /// </summary>
        v2_0wEdr = 3,
        /// <summary>
        /// Bluetooth Core Specification 2.1 + EDR
        /// </summary>
        v2_1wEdr = 4,
        /// <summary>
        /// Bluetooth Core Specification 3.0 + HS
        /// </summary>
        v3_0wHS = 5,
        /// <summary>
        /// Bluetooth Core Specification 4.0
        /// </summary>
        v4_0 = 6,
        /// <summary>
        /// Bluetooth Core Specification 4.1
        /// </summary>
        v4_1 = 7,
        /// <summary>
        /// Bluetooth Core Specification 4.2
        /// </summary>
        v4_2 = 8,
        /// <summary>
        /// Bluetooth Core Specification 5.0
        /// </summary>
        V5_0 = 9,
        //
        /// <summary>
        /// Unknown version &#x2104; probably the stack API
        /// does not provide the value.
        /// </summary>
        Unknown = 255
    }

    /// <summary>
    /// LMP VerNr &#x2014; Assigned Numbers &#x2014; Link Manager Protocol
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32")]
    public enum LmpVersion : byte
    {
        /// <summary>
        /// Bluetooth Core Specification 1.0b
        /// </summary>
        v1_0_b = 0,
        /// <summary>
        /// Bluetooth Core Specification 1.1
        /// </summary>
        v1_1 = 1,
        /// <summary>
        /// Bluetooth Core Specification 1.2
        /// </summary>
        v1_2 = 2,
        /// <summary>
        /// Bluetooth Core Specification 2.0 + EDR
        /// </summary>
        v2_0wEdr = 3,
        /// <summary>
        /// Bluetooth Core Specification 2.1 + EDR
        /// </summary>
        v2_1wEdr = 4,
        /// <summary>
        /// Bluetooth Core Specification 3.0 + HS
        /// </summary>
        v3_0wHS = 5,
        /// <summary>
        /// Bluetooth Core Specification 4.0
        /// </summary>
        v4_0 = 6,
        /// <summary>
        /// Bluetooth Core Specification 4.1
        /// </summary>
        v4_1 = 7,
        /// <summary>
        /// Bluetooth Core Specification 4.2
        /// </summary>
        v4_2 = 8,
        /// <summary>
        /// Bluetooth Core Specification 5.0
        /// </summary>
        V5_0 = 9,
        //
        /// <summary>
        /// Unknown version &#x2104; probably the stack API
        /// does not provide the value.
        /// </summary>
        Unknown = 255
    }
}
