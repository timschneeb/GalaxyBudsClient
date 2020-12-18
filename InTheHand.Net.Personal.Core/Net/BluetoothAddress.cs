// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.BluetoothAddress
// 
// Copyright (c) 2003-2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Diagnostics;

namespace InTheHand.Net
{
	/// <summary>
	/// Represents a Bluetooth device address.
	/// </summary>
    /// <remarks>The BluetoothAddress class contains the address of a bluetooth device.</remarks>
#if !NETCF
    [Serializable]
#endif
	public sealed class BluetoothAddress : IComparable, IFormattable
#if !NETCF
        , System.Xml.Serialization.IXmlSerializable //could be supported on NETCFv2
        , System.Runtime.Serialization.ISerializable
#endif
	{
        [NonSerialized] // Custom serialized in text format, to avoid any endian or length issues etc.
		private byte[] data;

        #region Constructor
        internal BluetoothAddress()
		{
			data = new byte[8];
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BluetoothAddress"/> class with the specified address.
		/// </summary>
		/// <param name="address"><see cref="Int64"/> representation of the address.</param>
		public BluetoothAddress(long address) : this()
		{
			//copy value to array
			BitConverter.GetBytes(address).CopyTo(data,0);
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="BluetoothAddress"/> class with the specified address.
        /// </summary>
        /// <param name="address"><see cref="UInt64"/> representation of the address.</param>
        [CLSCompliant(false)]
        public BluetoothAddress(ulong address) : this()
        {
            //copy value to array
            BitConverter.GetBytes(address).CopyTo(data, 0);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BluetoothAddress"/> class with the specified address.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>Note: The address should be supplied in little-endian order on the
        /// current Windows platform (which is little-endian).
        /// For forward compatibility it would be safer to use the 
        /// <see cref="M:InTheHand.Net.BluetoothAddress.Parse(System.String)"/> method, 
        /// which will be correct for all platforms.
        /// Or consider
        /// <see cref="M:InTheHand.Net.BluetoothAddress.CreateFromLittleEndian(System.Byte[])"/>
        /// or 
        /// <see cref="M:InTheHand.Net.BluetoothAddress.CreateFromBigEndian(System.Byte[])"/>.
        /// 
        /// </para>
        /// </remarks>
        /// -
        /// <param name="address">Address as 6 byte array.</param>
        /// <exception cref="T:System.ArgumentNullException">address passed was <see langword="null"/>.</exception>
        /// <exception cref="T:System.ArgumentException">address passed was not a 6 byte array.</exception>
        public BluetoothAddress(byte[] address) : this()
		{
            if (address == null) {
                throw new ArgumentNullException("address");
            }
			if(address.Length == 6 || address.Length == 8)
			{
				Buffer.BlockCopy(address, 0, data, 0, 6);
			}
			else
			{
				throw new ArgumentException("Address must be six bytes long.", "address");
			}
        }
        #endregion

        #region FactoryMethods
        /// <summary>
        /// Create a <see cref="T:InTheHand.Net.BluetoothAddress"/> from an Array of <see cref="T:System.Byte"/>
        /// where the array is in standard order.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>Different protocol stacks have different ways of storing a
        /// Bluetooth Address.   Some use an array of bytes e.g. "byte[6]",
        /// which means that the first byte of the address comes first in
        /// memory (which we&#x2019;ll call big-endian format).  Others
        /// e.g. the Microsoft stack use a long integer (e.g. uint64) which
        /// means that the *last* byte of the address come comes first in
        /// memory (which we&#x2019;ll call little-endian format)
        /// </para>
        /// <para>This method creates an address for the first form.
        /// See <see cref="M:InTheHand.Net.BluetoothAddress.CreateFromLittleEndian(System.Byte[])"/> for the second form.
        /// </para>
        /// </remarks>
        /// -
        /// <param name="address">An Array of <see cref="T:System.Byte"/>
        /// with the Bluetooth Address ordered as described above.
        /// </param>
        /// -
        /// <returns>The resultant <see cref="T:InTheHand.Net.BluetoothAddress"/>.
        /// </returns>
        /// -
        /// <seealso cref="M:InTheHand.Net.BluetoothAddress.CreateFromLittleEndian(System.Byte[])"/>
        public static BluetoothAddress CreateFromBigEndian(byte[] address)
        {
            var clone = (byte[])address.Clone();
            Array.Reverse(clone);
            return new BluetoothAddress(clone);
        }
        /// <summary>
        /// Create a <see cref="T:InTheHand.Net.BluetoothAddress"/> from an Array of <see cref="T:System.Byte"/>
        /// where the array is in reverse order.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>Different protocol stacks have different ways of storing a
        /// Bluetooth Address.   Some use an array of bytes e.g. "byte[6]",
        /// which means that the first byte of the address comes first in
        /// memory (which we&#x2019;ll call big-endian format).  Others
        /// e.g. the Microsoft stack use a long integer (e.g. uint64) which
        /// means that the *last* byte of the address come comes first in
        /// memory (which we&#x2019;ll call little-endian format)
        /// </para>
        /// <para>This method creates an address for the second form.
        /// See <see cref="M:InTheHand.Net.BluetoothAddress.CreateFromLittleEndian(System.Byte[])"/> for the first form.
        /// </para>
        /// </remarks>
        /// -
        /// <param name="address">An Array of <see cref="T:System.Byte"/>
        /// with the Bluetooth Address ordered as described above.
        /// </param>
        /// -
        /// <returns>The resultant <see cref="T:InTheHand.Net.BluetoothAddress"/>.
        /// </returns>
        /// -
        /// <seealso cref="M:InTheHand.Net.BluetoothAddress.CreateFromBigEndian(System.Byte[])"/>
        public static BluetoothAddress CreateFromLittleEndian(byte[] address)
        {
            var clone = (byte[])address.Clone();
            return new BluetoothAddress(clone);
        }
        #endregion

        #region Parse
        /// <summary>
        /// Converts the string representation of an address to it's <see cref="BluetoothAddress"/> equivalent.
        /// A return value indicates whether the operation succeeded.
        /// </summary>
        /// <param name="bluetoothString">A string containing an address to convert.</param>
        /// <param name="result">When this method returns, contains the <see cref="BluetoothAddress"/> equivalent to the address contained in s, if the conversion succeeded, or null (Nothing in Visual Basic) if the conversion failed.
        /// The conversion fails if the s parameter is null or is not of the correct format.</param>
        /// <returns>true if s is a valid Bluetooth address; otherwise, false.</returns>
        public static bool TryParse(string bluetoothString, out BluetoothAddress result)
        {
            Exception ex = ParseInternal(bluetoothString, out result);
            if (ex != null) return false;
            else return true;
        }

        /// <summary>
        /// Converts the string representation of a Bluetooth address to a new <see cref="BluetoothAddress"/> instance.
        /// </summary>
        /// <param name="bluetoothString">A string containing an address to convert.</param>
        /// <returns>New <see cref="BluetoothAddress"/> instance.</returns>
        /// <remarks>Address must be specified in hex format optionally separated by the colon or period character e.g. 000000000000, 00:00:00:00:00:00 or 00.00.00.00.00.00.</remarks>
        /// <exception cref="T:System.ArgumentNullException">bluetoothString is null.</exception>
        /// <exception cref="T:System.FormatException">bluetoothString is not a valid Bluetooth address.</exception>
        public static BluetoothAddress Parse(string bluetoothString)
        {
            BluetoothAddress result;
            Exception ex = ParseInternal(bluetoothString, out result);
            if (ex != null) throw ex;
            else return result;
        }
        #endregion

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Returned to caller.")]
        static Exception ParseInternal(string bluetoothString, out BluetoothAddress result)
        {
            const Exception Success = null;
            result = null;

            if (bluetoothString == null)
            {
                return new ArgumentNullException("bluetoothString");
            }

            if (bluetoothString.IndexOf(":", StringComparison.Ordinal) > -1)
            {
                //assume address in standard hex format 00:00:00:00:00:00

                //check length
                if (bluetoothString.Length != 17)
                {
                    return new FormatException("bluetoothString is not a valid Bluetooth address.");
                }

                try
                {
                    byte[] babytes = new byte[8];
                    //split on colons
                    string[] sbytes = bluetoothString.Split(':');
                    for (int ibyte = 0; ibyte < 6; ibyte++)
                    {
                        //parse hex byte in reverse order
                        babytes[ibyte] = byte.Parse(sbytes[5 - ibyte], System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    }
                    result = new BluetoothAddress(babytes);
                    return Success;
                }
                catch (Exception ex)
                {
                    return ex;
                }
            }
            else if (bluetoothString.IndexOf(".", StringComparison.Ordinal) > -1)
            {
                //assume address in uri hex format 00.00.00.00.00.00
                //check length
                if (bluetoothString.Length != 17)
                {
                    return new FormatException("bluetoothString is not a valid Bluetooth address.");
                }

                try
                {
                    byte[] babytes = new byte[8];
                    //split on periods
                    string[] sbytes = bluetoothString.Split('.');
                    for (int ibyte = 0; ibyte < 6; ibyte++)
                    {
                        //parse hex byte in reverse order
                        babytes[ibyte] = byte.Parse(sbytes[5 - ibyte], System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    }
                    result = new BluetoothAddress(babytes);
                    return Success;
                }
                catch (Exception ex)
                {
                    return ex;
                }
            }
            else
            {
                //assume specified as long integer
                if ((bluetoothString.Length < 12) | (bluetoothString.Length > 16))
                {
                    return new FormatException("bluetoothString is not a valid Bluetooth address.");
                }
                try
                {
                    result = new BluetoothAddress(long.Parse(bluetoothString, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                    return Success;
                }
                catch (Exception ex)
                {
                    return ex;
                }
            }
        }

        #region SAP
        /// <summary>
        /// Significant address part.
        /// </summary>
        [CLSCompliant(false)]
        public uint Sap
        {
            get
            {
                return BitConverter.ToUInt32(data, 0);
            }
        }
        #endregion
        #region LAP
        #endregion
        #region UAP
        #endregion
        #region NAP
        /// <summary>
        /// Non-significant address part.
        /// </summary>
        [CLSCompliant(false)]
        public ushort Nap
        {
            get
            {
                return BitConverter.ToUInt16(data, 4);
            }
        }
        #endregion

        #region To Byte Array
        /// <summary>
		/// Returns the value as a byte array.
		/// </summary>
        /// -
        /// <remarks>In previous versions this returned the internal array, it now
        /// returns a copy.  Addresses should be immutable, particularly for the
        /// None const!
        /// </remarks>
        /// -
		/// <returns>An array of byte</returns>
		public byte[] ToByteArray()
		{
            //return data;
            return (byte[])data.Clone();
        }

        /// <summary>
        /// Returns the value as a byte array,
        /// where the array is in reverse order.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>See <see cref="M:InTheHand.Net.BluetoothAddress.CreateFromBigEndian(System.Byte[])"/> for discussion of
        /// different stack#x2019;s storage formats for Bluetooth Addresses.
        /// </para>
        /// <para>In previous versions this returned the internal array, it now
        /// returns a copy.  Addresses should be immutable, particularly for the
        /// None const!
        /// </para>
        /// </remarks>
        /// -
        /// <returns>An array of byte of length six representing the Bluetooth address.</returns>
        public byte[] ToByteArrayLittleEndian()
        {
            var clone8 = ToByteArray();
            var copy6 = new byte[6];
            Array.Copy(clone8, copy6, copy6.Length);
            return copy6;
        }
        /// <summary>
        /// Returns the value as a byte array,
        /// where the array is in standard order.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>See <see cref="M:InTheHand.Net.BluetoothAddress.CreateFromBigEndian(System.Byte[])"/> for discussion of
        /// different stack#x2019;s storage formats for Bluetooth Addresses.
        /// </para>
        /// <para>In previous versions this returned the internal array, it now
        /// returns a copy.  Addresses should be immutable, particularly for the
        /// None const!
        /// </para>
        /// </remarks>
        /// -
        /// <returns>An array of byte of length six representing the Bluetooth address.</returns>
        public byte[] ToByteArrayBigEndian()
        {
            var arr6 = ToByteArrayLittleEndian();
            Debug.Assert(arr6.Length == 6, "BAD, arr6.Length is: " + arr6.Length);
            Array.Reverse(arr6);
            return arr6;
        }
        #endregion

        #region ToInt64
        /// <summary>
		/// Returns the Bluetooth address as a long integer.
		/// </summary>
        /// -
        /// <returns>An <see cref="T:System.Int64"/>.</returns>
		public long ToInt64()
		{
			return BitConverter.ToInt64(data, 0);
        }
        #endregion

        #region ToUInt64
        /// <summary>
		/// Returns the Bluetooth address as an unsigned long integer.
		/// </summary>
        /// -
        /// <returns>An <see cref="T:System.UInt64"/>.</returns>
        [CLSCompliant(false)]
		public ulong ToUInt64()
        {
            return BitConverter.ToUInt64(data, 0);
        }
        #endregion

        #region Equals
        /// <summary>
		/// Compares two <see cref="BluetoothAddress"/> instances for equality.
		/// </summary>
        /// -
        /// <param name="obj">The <see cref="BluetoothAddress"/>
        /// to compare with the current instance.
        /// </param>
        /// -
        /// <returns><c>true</c> if <paramref name="obj"/>
        /// is a <see cref="BluetoothAddress"/> and equal to the current instance;
        /// otherwise, <c>false</c>.
        /// </returns>
		public override bool Equals(object obj)
		{
			BluetoothAddress bta = obj as BluetoothAddress;
			
			if(bta!=null)
			{
				return (this==bta);	
			}

			return base.Equals (obj);
        }
        #endregion

        #region Get Hash Code
        /// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
		{
			return this.ToInt64().GetHashCode();
        }
        #endregion

        #region Operators
        /// <summary>
		/// Returns an indication whether the values of two specified <see cref="BluetoothAddress"/> objects are equal.<para><b>New in v1.5</b></para>
		/// </summary>
        /// -
        /// <param name="x">A <see cref="BluetoothAddress"/> or <see langword="null"/>.</param>
        /// <param name="y">A <see cref="BluetoothAddress"/> or <see langword="null"/>.</param>
        /// -
        /// <returns><c>true</c> if the values of the two instance are equal;
        /// otherwise, <c>false</c>.
        /// </returns>
		public static bool operator ==(BluetoothAddress x, BluetoothAddress y) 
		{
			if(((object)x == null) && ((object)y == null))
			{
				return true;
			}

			if(((object)x != null) && ((object)y != null))
			{
				if(x.ToInt64()==y.ToInt64())
				{
					return true;
				}
			}

			return false;
        }
        

        /// <summary>
		/// Returns an indication whether the values of two specified <see cref="BluetoothAddress"/> objects are not equal.
		/// </summary>
        /// -
        /// <param name="x">A <see cref="BluetoothAddress"/> or <see langword="null"/>.</param>
        /// <param name="y">A <see cref="BluetoothAddress"/> or <see langword="null"/>.</param>
        /// -
        /// <returns><c>true</c> if the value of the two instance is different;
        /// otherwise, <c>false</c>.
        /// </returns>
		public static bool operator !=(BluetoothAddress x, BluetoothAddress y) 
		{
			return !(x == y);
        }
        #endregion

        #region To String
        /// <summary>
		/// Converts the address to its equivalent string representation.
		/// </summary>
		/// <returns>The string representation of this instance.</returns>
		/// <remarks>The default return format is without a separator character 
        /// - use the <see cref="M:InTheHand.Net.BluetoothAddress.ToString(System.String)"/>
        /// overload for more formatting options.</remarks>
		public override string ToString()
		{
			return this.ToString("N");
		}

		/// <summary>
		/// Returns a <see cref="String"/> representation of the value of this <see cref="BluetoothAddress"/> instance, according to the provided format specifier.
		/// </summary>
		/// <param name="format">A single format specifier that indicates how to format the value of this address.
        /// The format parameter can be "N", "C", or "P".
        /// If format is null or the empty string (""), "N" is used.</param>
		/// <returns>A <see cref="String"/> representation of the value of this <see cref="BluetoothAddress"/>.</returns>
		/// <remarks><list type="table">
		/// <listheader><term>Specifier</term><term>Format of Return Value </term></listheader>
		/// <item><term>N</term><term>12 digits: <para>XXXXXXXXXXXX</para></term></item>
		/// <item><term>C</term><term>12 digits separated by colons: <para>XX:XX:XX:XX:XX:XX</para></term></item>
		/// <item><term>P</term><term>12 digits separated by periods: <para>XX.XX.XX.XX.XX.XX</para></term></item>
		/// </list></remarks>
		public string ToString(string format)
		{
			string separator;

			if(format==null || format==string.Empty)
			{
				separator = string.Empty;
			}
			else
			{

				switch(format.ToUpper(CultureInfo.InvariantCulture))
				{
                    case "8":
					case "N":
						separator = string.Empty;
						break;
					case "C":
						separator = ":";
						break;
					case "P":
						separator = ".";
						break;
					default:
						throw new FormatException("Invalid format specified - must be either \"N\", \"C\", \"P\", \"\" or null.");
				}
			}

			System.Text.StringBuilder result = new System.Text.StringBuilder(18);

            if (format == "8")
            {
                result.Append(data[7].ToString("X2") + separator);
                result.Append(data[6].ToString("X2") + separator);
            }

			result.Append(data[5].ToString("X2") + separator);
			result.Append(data[4].ToString("X2") + separator);
			result.Append(data[3].ToString("X2") + separator);
			result.Append(data[2].ToString("X2") + separator);
			result.Append(data[1].ToString("X2") + separator);
			result.Append(data[0].ToString("X2"));

			return result.ToString();
        }
        #endregion

        #region Static
        /// <summary>
		/// Provides a null Bluetooth address.
		/// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes",
            Justification = "Is now immutable.")]
		public static readonly BluetoothAddress None = new BluetoothAddress();

		/// <summary>
		/// 
		/// </summary>
		internal const int IacFirst = 0x9E8B00;

		/// <summary>
		/// 
		/// </summary>
		internal const int IacLast = 0x9E8B3f;

		/// <summary>
        /// Limited Inquiry Access Code.
		/// </summary>
		public const int Liac = 0x9E8B00;

		/// <summary>
        /// General Inquire Access Code.
        /// The default inquiry code which is used to discover all devices in range.
		/// </summary>
		public const int Giac = 0x9E8B33;

        #endregion

        #region IComparable Members

        int IComparable.CompareTo(object obj)
        {
            BluetoothAddress bta = obj as BluetoothAddress;

            if (bta != null)
            {
                return this.ToInt64().CompareTo(bta.ToInt64());
            }

            return -1;
        }

        #endregion

        #region IFormattable Members
        /// <summary>
        /// Returns a <see cref="String"/> representation of the value of this 
        /// <see cref="BluetoothAddress"/> instance, according to the provided format specifier.
        /// </summary>
        /// -
        /// <param name="format">A single format specifier that indicates how to format the value of this Address.
        /// See <see cref="M:InTheHand.Net.BluetoothAddress.ToString(System.String)"/>
        /// for the possible format strings and their output.
        /// </param>
        /// <param name="formatProvider">Ignored.
        /// </param>
        /// -
        /// <returns>A <see cref="String"/> representation of the value of this
        /// <see cref="BluetoothAddress"/>.
        /// </returns>
        /// -
        /// <remarks>See <see cref="M:InTheHand.Net.BluetoothAddress.ToString(System.String)"/>
        /// for the possible format strings and their output.
        /// </remarks>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            //for now just wrap existing ToString method
            return ToString(format);
        }

        #endregion

        #region IXmlSerializable Members
#if !NETCF
        System.Xml.Schema.XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema()
        {
            return null;
        }

        void System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader reader)
        {
            String text = reader.ReadElementContentAsString();
            BluetoothAddress tmpAddr = BluetoothAddress.Parse(text);
            this.data = tmpAddr.data;
        }

        void System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
        {
            // Serialize the address -- in text format, to avoid any endian or length 
            // issues etc.
            writer.WriteString(this.ToString("N"));
        }
#endif
        #endregion

        #region ISerializable Members
#if !NETCF
        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.LinkDemand,
            SerializationFormatter = true)]
        private BluetoothAddress(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            String text = info.GetString("dataString");
            BluetoothAddress tmpAddr = BluetoothAddress.Parse(text);
            this.data = tmpAddr.data;
        }

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.LinkDemand,
            SerializationFormatter = true)]
        void System.Runtime.Serialization.ISerializable.GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            // Serialize the address -- in text format, to avoid any endian or length 
            // issues etc.
            info.AddValue("dataString", this.ToString("N"));
        }
#endif
        #endregion

        #region Clone
        /// <summary>
        /// Creates a copy of the <see cref="BluetoothAddress"/>.
        /// </summary>
        /// <remarks>Creates a copy including of the internal byte array.
        /// </remarks>
        /// <returns>A copy of the <see cref="BluetoothAddress"/>.
        /// </returns>
        public object Clone()
        {
            BluetoothAddress addr2 = new BluetoothAddress();
            addr2.data = (byte[])this.data.Clone();
            return addr2;
        }
        #endregion
    }
}
