// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.BluetoothVersion
// 
// Copyright (c) 2003-2006 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;

namespace InTheHand.Net.Bluetooth
{
	/// <summary>
	/// Stores version information for a Bluetooth device.
	/// </summary>
	public class BluetoothVersion
	{
		private Version mHciVersion;
		private Version mLmpVersion;
		private int mManufacturer;
		private long mFeatures;

		internal BluetoothVersion(byte[] data)
		{
			byte hv = data[0];
			ushort hr = BitConverter.ToUInt16(data, 1);
			mHciVersion = new Version(hv, hr);
			byte lv = data[3];
			ushort ls = BitConverter.ToUInt16(data, 4);
			mLmpVersion = new Version(lv, ls);

			mManufacturer = BitConverter.ToUInt16(data, 6);
			mFeatures = BitConverter.ToInt64(data, 8);
		}
		internal BluetoothVersion(byte hciVersion, ushort hciRevision, byte lmpVersion, ushort lmpSubversion, ushort manufacturer, long features )
		{
			mHciVersion = new Version(hciVersion, hciRevision);
			mLmpVersion = new Version(lmpVersion, lmpSubversion);
			mManufacturer = manufacturer;
			mFeatures = features;
		}

		/// <summary>
		/// Version of the current Host Controller Interface (HCI) in the Bluetooth hardware.
		/// </summary>
		/// <remarks>This value changes only when new versions of the Bluetooth hardware are created for the new Bluetooth Special Interest Group (SIG) specifications.</remarks>
		public Version HciVersion
		{
			get
			{
				return mHciVersion;
			}
		}

		/// <summary>
		/// Version of the current Link Manager Protocol (LMP) in the Bluetooth hardware.
		/// </summary>
		public Version LmpVersion
		{
			get
			{
				return mLmpVersion;
			}
		}

		/// <summary>
		/// Name of the Bluetooth hardware manufacturer.
		/// </summary>
		public Manufacturer Manufacturer
		{
			get
			{
				return (Manufacturer)mManufacturer;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public long Features
		{
			get
			{
				return mFeatures;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return "Hci: " + HciVersion.ToString() + " Lmp: " + LmpVersion.ToString() + " Manufacturer: " + Manufacturer.ToString();
		}

	}
}
