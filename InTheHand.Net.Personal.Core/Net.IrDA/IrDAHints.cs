// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Sockets.IrDAHints
// 
// Copyright (c) 2003-2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

#region Using directives

using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace InTheHand.Net.Sockets
{
	/// <summary>
	/// Describes an enumeration of possible device types, such as Fax.
	/// </summary>
    /// <seealso cref="T:System.Net.Sockets.IrDAHints"/>
	[Flags()]
#if CODE_ANALYSIS
    [SuppressMessage("Microsoft.Design", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
#endif
	public enum IrDAHints
	{
		/// <summary>
		/// Unspecified device type.
		/// </summary>
		None = 0,
		/// <summary>
		/// A Plug and Play interface.
        /// </summary>
#if CODE_ANALYSIS
        [SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "Member", Justification = "Copied from NETCF's equivalent.")]
#endif
        PnP = 1,
		/// <summary>
		/// A Pocket PC or similar.
		/// </summary>
		PdaAndPalmtop = 2,
		/// <summary>
		/// A personal computer.
		/// </summary>
		Computer = 4,
		/// <summary>
		/// A printer.
		/// </summary>
		Printer = 8,
		/// <summary>
		/// A modem.
		/// </summary>
		Modem = 0x10,
		/// <summary>
		/// A fax.
		/// </summary>
		Fax = 0x20,
		/// <summary>
		/// A local area network access.
		/// </summary>
		LanAccess = 0x40,
        /// <summary>
        /// Contains extended hint bytes.
        /// </summary>
        Extension = 0x80,
		/// <summary>
		/// A telephonic device.
		/// </summary>
		Telephony = 0x100,
		/// <summary>
		/// A personal computer file server.
		/// </summary>
		FileServer = 0x200,
        /// <summary>
        /// Device supports IrCOMM.
        /// </summary>
#if CODE_ANALYSIS
        [SuppressMessage("Microsoft.Naming", "CA1705:LongAcronymsShouldBePascalCased", MessageId = "Member")]
#endif
        IrCOMM = 0x400,
        /// <summary>
        /// Device supports Object Exchange.
        /// </summary>
        Obex = 0x2000,
	}
}
