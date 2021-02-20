// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Sockets.IrDADeviceInfo
// 
// Copyright (c) 2003-2006 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

#if !NO_IRDA
using System;
using System.Net;
using System.Net.Sockets;

namespace InTheHand.Net.Sockets
{
	/// <summary>
	/// Provides information about remote devices connected by infrared communications.
	/// </summary>
    /// <seealso cref="T:System.Net.Sockets.IrDADeviceInfo"/>
	public class IrDADeviceInfo
	{
		private IrDAAddress address;
		private string name;
		private IrDAHints hints;
		private IrDACharacterSet charset;

		internal IrDADeviceInfo(IrDAAddress id, string name, IrDAHints hints, IrDACharacterSet charset)
		{
			this.address = id;
			this.name = name;
            this.hints = hints;
            this.charset = charset;
		}

		/// <summary>
		/// Returns the address of the remote device.
		/// </summary>
		public IrDAAddress DeviceAddress
		{
			get
			{
				return this.address;
			}
		}

		/// <summary>
		/// Provided solely for compatibility with System.Net.IrDA - consider using <see cref="DeviceAddress"/> instead.
		/// </summary>
        [Obsolete("Use the DeviceAddress property to access the device Address.", false)]
        public byte[] DeviceID
		{
			get
			{
				return this.address.ToByteArray();
			}
		}

		/// <summary>
		/// Gets the name of the device.
		/// </summary>
		public string DeviceName
		{
			get
			{
				return name;
			}
		}

		/// <summary>
		/// Gets the character set used by the server, such as ASCII.
		/// </summary>
		public IrDACharacterSet CharacterSet
		{
			get
			{
				return charset;
			}
		}

		/// <summary>
		/// Gets the type of the device, such as a computer.
		/// </summary>
		public IrDAHints Hints
		{
			get
			{
				return hints;
			}
		}

		/// <summary>
		/// Compares two <see cref="IrDADeviceInfo"/> instances for equality.
		/// </summary>
        /// -
        /// <param name="obj">The <see cref="BluetoothDeviceInfo"/>
        /// to compare with the current instance.
        /// </param>
        /// -
        /// <returns><c>true</c> if <paramref name="obj"/>
        /// is a <see cref="BluetoothDeviceInfo"/> and equal to the current instance;
        /// otherwise, <c>false</c>.
        /// </returns>
		public override bool Equals(object obj)
		{
			//objects are equal if device address matches
			IrDADeviceInfo irdi = obj as IrDADeviceInfo;
			
			if(irdi!=null)
			{
				return this.DeviceAddress.Equals(irdi.DeviceAddress);
			}

			return base.Equals(obj);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
        /// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return this.address.GetHashCode();
		}
	}
}
#endif
