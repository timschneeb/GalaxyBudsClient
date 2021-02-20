// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.IrDA.IrDAAttributeType
// 
// Copyright (c) 2003-2006 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

#region Using directives

using System;
using System.Diagnostics.CodeAnalysis;

#endregion

namespace InTheHand.Net.IrDA
{
    /// <summary>
    /// Defines the type of an IAS attribute.
    /// </summary>
#if CODE_ANALYSIS
    [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue"),
    SuppressMessage("Microsoft.Design", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
#endif
    public enum IrDAAttributeType
    {
        //#define IAS_ATTRIB_NO_ATTRIB    0x00000000
        //#define IAS_ATTRIB_NO_CLASS     0x00000010

        /// <summary>
        /// Identifies an integer attribute value.
        /// </summary>
        Integer = 0x00000001,
        /// <summary>
        /// Identifies a binary, or octet, attribute value.
        /// </summary>
        OctetSequence = 0x00000002,
        /// <summary>
        /// Identifies a string attribute value.
        /// </summary>
        String = 0x00000003,
    }
}
