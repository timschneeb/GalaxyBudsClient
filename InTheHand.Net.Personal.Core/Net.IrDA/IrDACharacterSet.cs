// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Sockets.IrDACharacterSet
// 
// Copyright (c) 2003-2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System.Diagnostics.CodeAnalysis;

namespace InTheHand.Net.Sockets
{
	/// <summary>
	/// Describes the character sets supported by the device.
	/// </summary>
    /// <remarks>The <see cref="IrDACharacterSet"/> enumeration describes the following character sets, which are used by the <see cref="IrDAClient"/> and <see cref="IrDADeviceInfo"/> classes.</remarks>
    /// <seealso cref="IrDAClient"/>
#if CODE_ANALYSIS
    [SuppressMessage("Microsoft.Design", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
#endif
	public enum IrDACharacterSet
	{
        /// <summary>
        /// The ASCII character set.
        /// </summary>
#if CODE_ANALYSIS
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1705:LongAcronymsShouldBePascalCased", MessageId = "Member", Justification="Copied from NETCF's equivalent.")]
#endif
        ASCII = 0,
        /// <summary>
        /// The western European graphic character set.
        /// </summary>
		ISO8859Latin1 = 1,
        /// <summary>
        /// The eastern European graphic character set.
        /// </summary>
		ISO8859Latin2 = 2,
        /// <summary>
        /// The southern European graphic character set.
        /// </summary>
		ISO8859Latin3 = 3,
        /// <summary>
        /// The northern European graphic character set.
        /// </summary>
		ISO8859Latin4 = 4,
        /// <summary>
        /// The Cyrillic graphic character set.
        /// </summary>
		ISO8859Cyrillic = 5,
        /// <summary>
        /// The Arabic graphic character set.
        /// </summary>
		ISO8859Arabic = 6,
        /// <summary>
        /// The Greek graphic character set.
        /// </summary>
		ISO8859Greek = 7,
        /// <summary>
        /// The Hebrew graphic character set.
        /// </summary>
		ISO8859Hebrew = 8,
        /// <summary>
        /// The Turkish graphic character set.
        /// </summary>
		ISO8859Latin5 = 9,
        /// <summary>
        /// The Unicode character set.
        /// </summary>
		Unicode = 0xff
	}
}
