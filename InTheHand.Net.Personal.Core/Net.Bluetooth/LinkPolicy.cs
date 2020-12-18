// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.LinkPolicy
// 
// Copyright (c) 2003-2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Diagnostics.CodeAnalysis;

namespace InTheHand.Net.Bluetooth
{
	/// <summary>
	/// Flags to describe Link Policy.
	/// </summary>
	[Flags()]
    [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue"),
     SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32")]
	public enum LinkPolicy : short
	{
		/// <summary>
		/// Disables all LAN Manager (LM) modes. 
		/// </summary>
		Disabled = 0x0000, 
		/// <summary>
		/// Enables the master slave switch.
		/// </summary>
		MasterSlave = 0x0001,
		/// <summary>
		/// Enables Hold mode.
		/// </summary>
		Hold = 0x0002,  
		/// <summary>
		/// Enables Sniff Mode.
		/// </summary>
		Sniff = 0x0004,
		/// <summary>
		/// Enables Park Mode.
		/// </summary>
		Park = 0x0008,  

	}
}
