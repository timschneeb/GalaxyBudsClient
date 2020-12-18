// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.IrDAEndPoint
// 
// Copyright (c) 2003-2006 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Net;
using System.Net.Sockets;

namespace InTheHand.Net
{
	/// <summary>
	/// Represents an end point for an infrared connection.
	/// </summary>
    /// <seealso cref="T:System.Net.IrDAEndPoint"/>
#if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId="EndPoint")]
#endif
    public class IrDAEndPoint : EndPoint
	{
        private IrDAAddress id;
		private string service;

        #region Constructor
        /// <summary>
		/// Initializes a new instance of the <see cref="IrDAEndPoint"/> class.
		/// </summary>
		/// <param name="irdaDeviceID">The device identifier.</param>
        /// <param name="serviceName">The Service Name to connect to/listen on eg "<c>OBEX</c>".
        /// In the very uncommon case where a connection is to be made to
        /// / a server is to listen on 
        /// a specific LSAP-SEL (port number), then use 
        /// the form "<c>LSAP-SELn</c>", where n is an integer.
        /// </param>
        [Obsolete("Use the constructor which accepts an IrDAAddress.", false)]
		public IrDAEndPoint(byte[] irdaDeviceID, string serviceName)
		{
            if (irdaDeviceID == null) {
                throw new ArgumentNullException("irdaDeviceID");
            }
            if (serviceName == null) {
                throw new ArgumentNullException("serviceName");
            }
            //
			this.id = new IrDAAddress(irdaDeviceID);
			this.service = serviceName;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IrDAEndPoint"/> class.
		/// </summary>
		/// <param name="irdaDeviceAddress">The device address.</param>
        /// <param name="serviceName">The Service Name to connect to/listen on eg "<c>OBEX</c>".
        /// In the very uncommon case where a connection is to be made to
        /// / a server is to listen on 
        /// a specific LSAP-SEL (port number), then use 
        /// the form "<c>LSAP-SELn</c>", where n is an integer.
        /// </param>
        public IrDAEndPoint(IrDAAddress irdaDeviceAddress, string serviceName)
		{
            if (irdaDeviceAddress == null) {
                throw new ArgumentNullException("irdaDeviceAddress");
            }
            if (serviceName == null) {
                throw new ArgumentNullException("serviceName");
            }
            //
            this.id = irdaDeviceAddress;
			this.service = serviceName;
        }
        #endregion


        #region Address
        /// <summary>
		/// Gets or sets an address for the device.
		/// </summary>
		public IrDAAddress Address
		{
			get
			{
				return this.id;
			}
			set
			{
				if(value==null)
				{
					throw new ArgumentNullException("value");
				}
				this.id = value;
			}
		}

		/// <summary>
		/// Gets or sets an identifier for the device.
		/// </summary>
        /// <exception cref="T:System.ArgumentNullException">
        /// The specified byte array is null (<c>Nothing</c> in Visual Basic).
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The specified byte array is not four bytes long.
        /// </exception>
        [Obsolete("Use the Address property to access the device Address.", false)]
		public byte[] DeviceID
		{
			get
			{
				return this.id.ToByteArray();
			}
			set
			{
				if(value==null)
				{
					throw new ArgumentNullException("value");
				}
				if(value.Length!=4)
				{
                    throw ExceptionFactory.ArgumentOutOfRangeException("value", "DeviceID must be 4 bytes");
               }
				this.id = new IrDAAddress(value);
			}
        }
        #endregion

        #region Service Name
        /// <summary>
		/// Gets or sets the name of the service.
		/// </summary>
		public string ServiceName
		{
			get
			{
				return this.service;
			}
			set
			{
				if(value == null)
				{
					throw new ArgumentNullException("value");
				}

				this.service = value;
			}
        }
        #endregion

        #region AddressFamily
        /// <summary>
		/// Gets the address family to which the endpoint belongs.
		/// </summary>
		public override AddressFamily AddressFamily
		{
			get
			{
				return AddressFamily.Irda;
			}
        }
        #endregion


        #region Serialize & Create
        /// <inheritdoc/>
		public override SocketAddress Serialize()
		{
			SocketAddress sa = new SocketAddress(AddressFamily.Irda, 32);
			
			byte[] b = this.id.ToByteArray();
			for(int ibyte = 0; ibyte < 4; ibyte++)
			{
				sa[ibyte+2] = b[ibyte];
			}
			
			byte[] buffer = System.Text.Encoding.ASCII.GetBytes(this.service);
            const int MaxServiceNameBytes = 24;
            if (buffer.Length > MaxServiceNameBytes) {
                throw new InvalidOperationException("ServiceName has a maximum length of 24 bytes.");
            }
			for(int iservice = 0; iservice < buffer.Length; iservice++)
			{
				sa[iservice + 6] = buffer[iservice];
			}

			// Ensure null-terminated
            if(sa[30] != 0 || sa[31] != 0){
                throw new InvalidOperationException("ServiceName too long for SocketAddress.");
            }

			return sa;
        }

        /// <inheritdoc/>
		public override EndPoint Create(SocketAddress socketAddress)
		{
            if (socketAddress == null) {
                throw new ArgumentNullException("socketAddress");
            }
            //
			byte[] id = new byte[4];
			for(int ibyte = 0; ibyte < 4; ibyte++)
			{
				id[ibyte] = socketAddress[ibyte+2];
			}
			
			byte[] buffer = new byte[24];
			for(int iservice = 0; iservice < buffer.Length; iservice++)
			{
				buffer[iservice] = socketAddress[iservice + 6];
			}
			string name = System.Text.Encoding.ASCII.GetString(buffer, 0, 24);
			if(name.IndexOf('\0') > -1)
			{
				name = name.Substring(0, name.IndexOf('\0'));
			}

            return new IrDAEndPoint(new IrDAAddress(id), name);
        }
        #endregion

        #region Equals
        /// <summary>
		/// Compares two <see cref="IrDAEndPoint"/> instances for equality.
		/// </summary>
        /// -
        /// <param name="obj">The <see cref="BluetoothEndPoint"/>
        /// to compare with the current instance.
        /// </param>
        /// -
        /// <returns><c>true</c> if <paramref name="obj"/>
        /// is a <see cref="IrDAEndPoint"/> and equal to the current instance;
        /// otherwise, <c>false</c>.
        /// </returns>
		public override bool Equals(object obj)
		{
			IrDAEndPoint irep = obj as IrDAEndPoint;
			
			if(irep!=null)
			{
				return (this.Address.Equals(irep.Address) && this.ServiceName.Equals(irep.ServiceName));	
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
            // TODO (Include ServiceName in the GetHashCode)
			return this.Address.GetHashCode();
        }
        #endregion

        #region To String
        /// <summary>
        /// Returns the string representation of the IrDAEndPoint.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The string is in format <c>&lt;DeviceAddress&gt;:&lt;ServiceName&gt;</c>
        /// </para>
        /// An example is:
        /// <code lang="none">"04E20304:OBEX"</code>
        /// </remarks>
        /// <returns>The string representation of the IrDAEndPoint.</returns>
        public override string ToString()
        {
            return Address.ToString() + ":" + this.ServiceName;
        }
        #endregion
	}
}
