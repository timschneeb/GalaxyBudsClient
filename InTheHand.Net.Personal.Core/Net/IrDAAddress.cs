// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.IrDAAddress
// 
// Copyright (c) 2003-2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics.CodeAnalysis;

namespace InTheHand.Net
{
	/// <summary>
	/// Represents an IrDA device address.
	/// </summary>
	public sealed class IrDAAddress : IComparable, IFormattable 
	{
        const int AddressBytesLength = 4;
        
        // This should be read-only but arrays can't be. :-(
        // Currently there are no Properties that change the content of this type
        // so currently it is non-mutable.  If we wanted a mutable type, then we
        // have to be careful of the 'const' "None" value, as a user could change
        // it, and then other uses would *not* be None...
        byte[] data;

        //--------------------------------------------------------------
        //--------------------------------------------------------------
        #region Constructor
        internal IrDAAddress()
		{
			data = new byte[4];
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IrDAAddress"/> class with the specified address.
		/// </summary>
		/// <param name="address">Address as 4 byte array.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="address"/> was null.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="address"/> was not a 4 byte array.</exception>
		public IrDAAddress(byte[] address)
		{
            if (address == null) {
                throw new ArgumentNullException("address");
            }
			if(address.Length!=4)
			{
				throw new ArgumentException("Address bytes array must be four bytes in size.");
            }
            data = (byte[])address.Clone();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IrDAAddress"/> class with the specified address.
		/// </summary>
		/// <param name="address"><see cref="Int32"/> representation of the address.</param>
		public IrDAAddress(int address)
		{
			data = new byte[4];
			BitConverter.GetBytes(address).CopyTo(data,0);
        }
        #endregion

        /// <summary>
		/// Returns the IrDA address as an integer.
		/// </summary>
        /// -
        /// <returns>An <see cref="T:System.Int64"/>.</returns>
		public int ToInt32()
		{
			return BitConverter.ToInt32(data, 0);
		}

		/// <summary>
		/// Returns the internal byte array.
		/// </summary>
        /// -
        /// <returns>An array of <see cref="T:System.Byte"/>.</returns>
		public byte[] ToByteArray()
		{
            return (byte[])data.Clone();
        }

        #region Parse
        /// <summary>
        /// Converts the string representation of an address to it's <see cref="IrDAAddress"/> equivalent.
        /// A return value indicates whether the operation succeeded.
        /// </summary>
        /// <param name="s">A string containing an address to convert.</param>
        /// <param name="result">When this method returns, contains the <see cref="IrDAAddress"/> equivalent to the address contained in s, if the conversion succeeded, or null (Nothing in Visual Basic) if the conversion failed.
        /// The conversion fails if the s parameter is null or is not of the correct format.</param>
        /// <returns>true if s is a valid IrDA address; otherwise, false.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "TrParse pattern.")]
        public static bool TryParse(string s, out IrDAAddress result)
        {
            try
            {
                result = Parse(s);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
		/// <summary>
		/// Converts the string representation of an IrDA address to a new <see cref="IrDAAddress"/> instance.
		/// </summary>
        /// <param name="irdaString">A string containing an address to convert.</param>
		/// <returns>New <see cref="IrDAAddress"/> instance.</returns>
		/// <remarks>Address must be specified in hex format optionally separated by the colon or period character e.g. 00000000, 00:00:00:00 or 00.00.00.00.</remarks>
        /// <exception cref="T:System.ArgumentNullException">irdaString is null.</exception>
        /// <exception cref="T:System.FormatException">irdaString is not a valid IrDA address.</exception>
        public static IrDAAddress Parse(string irdaString)
		{
            if (irdaString == null)
			{
				throw new ArgumentNullException("irdaString");
			}

			IrDAAddress ia;

            if (irdaString.IndexOf(":", StringComparison.Ordinal) > -1)
			{
				//assume address in colon separated hex format 00:00:00:00
                //check length
                if (irdaString.Length != 11)
                {
                    throw new FormatException("irdaString is not a valid IrDA address.");
                }

				byte[] iabytes = new byte[AddressBytesLength];
				//split on colons
                string[] sbytes = irdaString.Split(':');
				for(int ibyte = 0; ibyte < iabytes.Length; ibyte++)
				{
					//parse hex byte in reverse order
					iabytes[ibyte] = byte.Parse(sbytes[3 - ibyte],System.Globalization.NumberStyles.HexNumber);
				}
                ia = new IrDAAddress(iabytes);
			}
            else if (irdaString.IndexOf(".", StringComparison.Ordinal) > -1)
			{
				//assume address in uri hex format 00.00.00.00
                //check length
                if (irdaString.Length != 11)
                {
                    throw new FormatException("irdaString is not a valid IrDA address.");
                }

                byte[] iabytes = new byte[AddressBytesLength];
                //split on colons
                string[] sbytes = irdaString.Split('.');
				for(int ibyte = 0; ibyte < iabytes.Length; ibyte++)
				{
					//parse hex byte in reverse order
					iabytes[ibyte] = byte.Parse(sbytes[3 - ibyte],System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
				}
                ia = new IrDAAddress(iabytes);
            }
			else
			{
				//assume specified as integer
                //check length
                if ((irdaString.Length < 4) | (irdaString.Length > 8))
                {
                    throw new FormatException("irdaString is not a valid IrDA address.");
                }

                ia = new IrDAAddress(int.Parse(irdaString, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture));
			}

			return ia;
        }
        #endregion

        #region To String
        /// <summary>
		/// Converts the address to its equivalent string representation.
		/// </summary>
		/// <returns>The string representation of this instance.</returns>
		public override string ToString()
		{
			return this.ToString("N");
		}

		/// <summary>
		/// Returns a <see cref="String"/> representation of the value of this <see cref="IrDAAddress"/> instance, according to the provided format specifier.
		/// </summary>
		/// <param name="format">A single format specifier that indicates how to format the value of this Guid. The format parameter can be "N", "C" or "P". If format is null or the empty string (""), "N" is used.</param>
		/// <returns>A <see cref="String"/> representation of the value of this <see cref="BluetoothAddress"/>.</returns>
		/// <remarks><list type="table">
		/// <listheader><term>Specifier</term><term>Format of Return Value </term></listheader>
		/// <item><term>N</term><term>8 digits: <para>XXXXXXXX</para></term></item>
		/// <item><term>C</term><term>8 digits separated by colons: <para>XX:XX:XX:XX</para></term></item>
		/// <item><term>P</term><term>8 digits separated by periods: <para>XX.XX.XX.XX</para></term></item>
		/// </list></remarks>
		public string ToString(string format)
		{
			string separator;

            if (format == null || format.Length == 0)
			{
				separator = string.Empty;
			}
			else
			{

                // FxCop notes: "Warning, Avoid unnecessary string creation" due to this.
                // It is compiled as a series of if/else statments...
				switch(format.ToUpper())
				{
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
			
			result.Append(data[3].ToString("X2") + separator);
			result.Append(data[2].ToString("X2") + separator);
			result.Append(data[1].ToString("X2") + separator);
			result.Append(data[0].ToString("X2"));

			return result.ToString();
        }
        #endregion

        /// <summary>
		/// Compares two <see cref="IrDAAddress"/> instances for equality.
		/// </summary>
        /// -
        /// <param name="obj">The <see cref="IrDAAddress"/>
        /// to compare with the current instance.
        /// </param>
        /// -
        /// <returns><c>true</c> if <paramref name="obj"/>
        /// is a <see cref="IrDAAddress"/> and equal to the current instance;
        /// otherwise, <c>false</c>.
        /// </returns>
		public override bool Equals(object obj)
		{
			IrDAAddress ira = obj as IrDAAddress;
			
			if(ira!=null)
			{
				return (this==ira);	
			}

			return base.Equals (obj);
			
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
		{
			return this.ToInt32();
		}


		/// <summary>
		/// Returns an indication whether the values of two specified <see cref="IrDAAddress"/> objects are equal.
		/// </summary>
        /// -
        /// <param name="x">A <see cref="IrDAAddress"/> or <see langword="null"/>.</param>
        /// <param name="y">A <see cref="IrDAAddress"/> or <see langword="null"/>.</param>
        /// -
        /// <returns><c>true</c> if the values of the two instance are equal;
        /// otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(IrDAAddress x, IrDAAddress y) 
		{
			if(((object)x == null) && ((object)y == null))
			{
				return true;
			}

			if(((object)x != null) && ((object)y != null))
			{
				if(x.ToInt32()==y.ToInt32())
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Returns an indication whether the values of two specified <see cref="IrDAAddress"/> objects are not equal.
		/// </summary>
        /// -
        /// <param name="x">A <see cref="IrDAAddress"/> or <see langword="null"/>.</param>
        /// <param name="y">A <see cref="IrDAAddress"/> or <see langword="null"/>.</param>
        /// -
        /// <returns><c>true</c> if the value of the two instance is different;
        /// otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(IrDAAddress x, IrDAAddress y) 
		{
			return !(x == y);
        }

        #region None
        /// <summary>
		/// Provides a null IrDA address.
        /// </summary>
#if CODE_ANALYSIS
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Is now immutable.")]
#endif
        public static readonly IrDAAddress None = new IrDAAddress();
        #endregion

        #region IComparable Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        int IComparable.CompareTo(object obj)
        {
            IrDAAddress irda = obj as IrDAAddress;

            if (irda != null)
            {
                return this.ToInt32().CompareTo(irda.ToInt32());
            }

            return -1;
        }

        #endregion

        #region IFormattable Members
        /// <summary>
        /// Returns a <see cref="String"/> representation of the value of this <see cref="IrDAAddress"/> instance, according to the provided format specifier.
        /// </summary>
        /// <param name="format">A single format specifier that indicates how to format the value of this Guid. The format parameter can be "N", "C" or "P". If format is null or the empty string (""), "N" is used.</param>
        /// <param name="formatProvider">Ignored.</param>
        /// -
        /// <returns>A <see cref="String"/> representation of the value of this <see cref="IrDAAddress"/>.</returns>
        /// -
        /// <remarks>See <see cref="M:InTheHand.Net.IrDAAddress.ToString(System.String)"/>
        /// for the possible format strings and their output.
        /// </remarks>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            //for now just wrap existing ToString method
            return ToString(format);
        }

        #endregion
		
	}
}
