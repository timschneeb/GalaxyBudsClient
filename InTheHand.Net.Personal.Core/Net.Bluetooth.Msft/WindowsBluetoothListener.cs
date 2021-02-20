// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.Msft.WindowsBluetoothListener
// 
// Copyright (c) 2003-2011 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Diagnostics;
using System.Net.Sockets;
using InTheHand.Net.Bluetooth.Factory;
using InTheHand.Net.Sockets;


namespace InTheHand.Net.Bluetooth.Msft
{
	/// <summary>
	/// Listens for connections from Bluetooth network clients.
	/// </summary>
	/// <remarks>The <see cref="WindowsBluetoothListener"/> class provides simple methods that listen for and accept incoming connection requests in blocking synchronous mode.
	/// You can use either a <see cref="BluetoothClient"/> or a <see cref="Socket"/> to connect with a <see cref="WindowsBluetoothListener"/></remarks>
	class WindowsBluetoothListener : IBluetoothListener
	{
        readonly BluetoothFactory _fcty;
        private bool active;
		private BluetoothEndPoint serverEP;
        private ISocketOptionHelper m_optionHelper;


		private IntPtr serviceHandle;
        private ServiceRecord m_serviceRecord;
        private bool m_manualServiceRecord;
        private ServiceClass codService;
        private byte[] m_activeServiceRecordBytes; // As passed to WSASetService(REGISTER).

        #region Constructor
#if NETCF
        static WindowsBluetoothListener()
        {
            InTheHand.Net.PlatformVerification.ThrowException();
        }
#endif

        internal WindowsBluetoothListener(BluetoothFactory fcty)
        {
            Debug.Assert(fcty != null, "ArgNull");
            _fcty = fcty;
        }

        protected virtual Socket CreateSocket()
        {
            return new Socket(BluetoothAddressFamily, SocketType.Stream, BluetoothProtocolType.RFComm);
        }

        protected virtual AddressFamily BluetoothAddressFamily
        {
            get { return AddressFamily32.Bluetooth; }
        }

        protected virtual ISocketOptionHelper CreateSocketOptionHelper(Socket socket)
        {
            return new SocketBluetoothClient.MsftSocketOptionHelper(socket);
        }

        /// <overloads>
        /// Initializes a new instance of the <see cref="WindowsBluetoothListener"/> class.
        /// </overloads>
        /// ----
        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsBluetoothListener"/> class
        /// to listen on the specified service identifier.
        /// </summary>
        /// <param name="service">The Bluetooth service to listen for.</param>
        /// <remarks>
        /// <para>
        /// An SDP record is published on successful <see cref="M:InTheHand.Net.Sockets.WindowsBluetoothListener.Start"/>
        /// to advertise the server.
        /// A generic record is created, containing the essential <c>ServiceClassIdList</c>
        /// and <c>ProtocolDescriptorList</c> attributes.  The specified service identifier is
        /// inserted into the former, and the RFCOMM Channel number that the server is
        /// listening on is inserted into the latter.  See the Bluetooth SDP specification
        /// for details on the use and format of SDP records.
        /// </para><para>
        /// If a SDP record with more elements is required, then use
        /// one of the other constructors that takes an SDP record e.g. 
        /// <see cref="M:InTheHand.Net.Sockets.WindowsBluetoothListener.#ctor(System.Guid,InTheHand.Net.Bluetooth.ServiceRecord)"/>,
        /// or when passing it as a byte array 
        /// <see cref="M:InTheHand.Net.Sockets.WindowsBluetoothListener.#ctor(System.Guid,System.Byte[],System.Int32)"/>.
        /// The format of the generic record used here is shown there also.
        /// </para><para>
        /// Call the <see cref="M:InTheHand.Net.Sockets.WindowsBluetoothListener.Start"/> 
        /// method to begin listening for incoming connection attempts.
        /// </para>
        /// </remarks>
        public void Construct(Guid service)
        {
            InitServiceRecord(service);
            this.serverEP = new BluetoothEndPoint(BluetoothAddress.None, service);
            serverSocket = CreateSocket();
            m_optionHelper = CreateSocketOptionHelper(serverSocket);
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsBluetoothListener"/> class
        /// that listens for incoming connection attempts on the specified local Bluetooth address and service identifier. 
        /// </summary>
        /// <param name="localAddress">A <see cref="BluetoothAddress"/> that represents the local Bluetooth radio address.</param>
        /// <param name="service">The Bluetooth service on which to listen for incoming connection attempts.</param>
        /// <remarks>
        /// <para>
        /// An SDP record is published on successful <see cref="M:InTheHand.Net.Sockets.WindowsBluetoothListener.Start"/>
        /// to advertise the server.
        /// A generic record is created, containing the essential <c>ServiceClassIdList</c>
        /// and <c>ProtocolDescriptorList</c> attributes.  The specified service identifier is
        /// inserted into the former, and the RFCOMM Channel number that the server is
        /// listening on is inserted into the latter.  See the Bluetooth SDP specification
        /// for details on the use and format of SDP records.
        /// </para><para>
        /// If a SDP record with more elements is required, then use
        /// one of the other constructors that takes an SDP record e.g. 
        /// <see cref="M:InTheHand.Net.Sockets.WindowsBluetoothListener.#ctor(InTheHand.Net.BluetoothAddress,System.Guid,InTheHand.Net.Bluetooth.ServiceRecord)"/>,
        /// or when passing it as a byte array, e.g. 
        /// <see cref="M:InTheHand.Net.Sockets.WindowsBluetoothListener.#ctor(InTheHand.Net.BluetoothAddress,System.Guid,System.Byte[],System.Int32)"/>.
        /// The format of the generic record used here is shown there also.
        /// </para><para>
        /// Call the <see cref="M:InTheHand.Net.Sockets.WindowsBluetoothListener.Start"/> 
        /// method to begin listening for incoming connection attempts.
        /// </para>
        /// </remarks>
        public void Construct(BluetoothAddress localAddress, Guid service)
        {
            if (localAddress == null) {
                throw new ArgumentNullException("localAddress");
            }
            if (service == Guid.Empty) {
                throw new ArgumentNullException("service");
            }

            InitServiceRecord(service);
            this.serverEP = new BluetoothEndPoint(localAddress, service);
            serverSocket = CreateSocket();
            m_optionHelper = CreateSocketOptionHelper(serverSocket);
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsBluetoothListener"/> class
        /// with the specified local endpoint.
		/// </summary>
        /// <param name="localEP">A <see cref="BluetoothEndPoint"/> that represents the local endpoint to which to bind the listener <see cref="Socket"/>.</param>
        /// <remarks>
        /// <para>
        /// An SDP record is published on successful <see cref="M:InTheHand.Net.Sockets.WindowsBluetoothListener.Start"/>
        /// to advertise the server.
        /// A generic record is created, containing the essential <c>ServiceClassIdList</c>
        /// and <c>ProtocolDescriptorList</c> attributes.  The specified service identifier is
        /// inserted into the former, and the RFCOMM Channel number that the server is
        /// listening on is inserted into the latter.  See the Bluetooth SDP specification
        /// for details on the use and format of SDP records.
        /// </para><para>
        /// If a SDP record with more elements is required, then use
        /// one of the other constructors that takes an SDP record e.g. 
        /// <see cref="M:InTheHand.Net.Sockets.WindowsBluetoothListener.#ctor(InTheHand.Net.BluetoothEndPoint,InTheHand.Net.Bluetooth.ServiceRecord)"/>,
        /// or when passing it as a byte array
        /// <see cref="M:InTheHand.Net.Sockets.WindowsBluetoothListener.#ctor(InTheHand.Net.BluetoothEndPoint,System.Byte[],System.Int32)"/>.
        /// The format of the generic record used here is shown there also.
        /// </para><para>
        /// Call the <see cref="M:InTheHand.Net.Sockets.WindowsBluetoothListener.Start"/> 
        /// method to begin listening for incoming connection attempts.
        /// </para>
        /// </remarks>
        public void Construct(BluetoothEndPoint localEP)
		{
            if (localEP == null)
            {
                throw new ArgumentNullException("localEP");
            }

            InitServiceRecord(localEP.Service);
            this.serverEP = localEP;
            serverSocket = CreateSocket();
            m_optionHelper = CreateSocketOptionHelper(serverSocket);
        }
        //----------------
        
        /// <summary>
		/// Initializes a new instance of the <see cref="WindowsBluetoothListener"/> class
        /// to listen on the specified service identifier, 
        /// publishing the specified SDP record.
        /// </summary>
		/// <param name="service">The Bluetooth service to listen for.</param>
		/// <param name="sdpRecord">Prepared SDP Record to publish.</param>
		/// <param name="channelOffset">
        /// The index in the <paramref name="sdpRecord"/> byte array where the RFCOMM Channel Number that the
        /// server is listening on is to be placed.
        /// However the supplied record is now parsed into an <see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/>
        /// instance, and the channel offset is not used.
        /// </param>
        /// <remarks>
        /// <note>
        /// The constructors taking the SDP record explicitly (as a byte array) should
        /// only be used if
        /// a specialized SDP record is required. For instance when using one of the
        /// standard profiles.  Otherwise use one of the other constructors 
        /// e.g. <see 
        /// cref="M:InTheHand.Net.Sockets.WindowsBluetoothListener.#ctor(System.Guid)"/>
        /// which create a generic SDP Record from the specified service identifier.
        /// </note>
        /// <para>Instead of passing a byte array containing a hand-built record,
        /// the record can also be built using the <see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/>
        /// and <see cref="T:InTheHand.Net.Bluetooth.ServiceElement"/> classes, and
        /// passed to the respective constuctor, e.g.
        /// <see cref="M:InTheHand.Net.Sockets.WindowsBluetoothListener.#ctor(System.Guid,InTheHand.Net.Bluetooth.ServiceRecord)"/>
        /// </para>
        /// <para>Any useful SDP record will include 
        /// a <c>ProtocolDescriptor</c> element containing
        /// the RFCOMM Channel number that the server is listening on,
        /// and a <c>ServiceClassId</c> element containing the service UUIDs.
        /// The record supplied in the <paramref name="sdpRecord"/> parameter
        /// should contain those elements.  On successful <see 
        /// cref="M:InTheHand.Net.Sockets.WindowsBluetoothListener.Start"/>,
        /// the RFCOMM Channel number that the protocol stack has assigned to the
        /// server is retrieved, and copied into the service record before it is
        /// published.  The <paramref name="channelOffset"/> indicates the location
        /// of the respective byte in the <paramref name="sdpRecord"/> byte array.
        /// </para>
        /// <para>
        /// An example SDP record is as follows.  This is actually the format of the 
        /// generic record used in the other constructors.  For another example see
        /// the code in the <c>ObexListener</c> class.
        /// <code>
        /// // The asterisks note where the Service UUID and the Channel number are
        /// // to be filled in.
        /// byte[] record = new byte[] {
        ///   //Element Sequence:
        ///   0x35,0x27,
        ///     //UInt16: 0x0001  -- ServiceClassIdList
        ///     0x09,0x00,0x01,
        ///     //Element Sequence:
        ///     0x35,0x11,
        ///     //  UUID128: 00000000-0000-0000-0000-000000000000 -- * Service UUID
        ///         0x1c,
        ///           0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00,
        ///           0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00,
        ///     //
        ///     //UInt16: 0x0004  -- ProtocolDescriptorList
        ///     0x09,0x00,0x04,
        ///     //Element Sequence:
        ///     0x35,0x0c,
        ///     //  Element Sequence:
        ///         0x35,0x03,
        ///     //      UUID16: 0x0100  -- L2CAP
        ///             0x19,0x01,0x00,
        ///     //  Element Sequence:
        ///         0x35,0x05,
        ///     //      UUID16: 0x0003  -- RFCOMM
        ///             0x19,0x00,0x03,
        ///     //      UInt8: 0x00     -- * Channel Number
        ///             0x08,0x00
        /// };
        /// </code>
        /// For that record the <c>channelOffset</c> is 40.
        /// </para>
        /// </remarks>
		public void Construct(Guid service, byte[] sdpRecord, int channelOffset)
        {
            Construct(service);
            InitServiceRecord(sdpRecord, channelOffset);
		}
        
        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsBluetoothListener"/> class
        /// that listens for incoming connection attempts on the specified local Bluetooth address and service identifier,
        /// publishing the specified SDP record.
        /// </summary>
        /// <param name="localAddress">A <see cref="BluetoothAddress"/> that represents the local Bluetooth radio address.</param>
        /// <param name="service">The Bluetooth service to listen for.</param>
        /// <param name="sdpRecord">Prepared SDP Record to publish</param>
        /// <param name="channelOffset">
        /// The index in the <paramref name="sdpRecord"/> byte array where the RFCOMM Channel Number that the
        /// server is listening on is to be placed.
        /// However the supplied record is now parsed into an <see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/>
        /// instance, and the channel offset is not used.
        /// </param>
        /// <remarks>
        /// <note>
        /// The constructors taking the SDP record explicitly (as a byte array) should
        /// only be used if
        /// a specialized SDP record is required. For instance when using one of the
        /// standard profiles.  Otherwise use one of the other constructors 
        /// e.g. <see 
        /// cref="M:InTheHand.Net.Sockets.WindowsBluetoothListener.#ctor(InTheHand.Net.BluetoothAddress,System.Guid)"/>
        /// which create a generic SDP Record from the specified service identifier.
        /// </note>
        /// <para>Instead of passing a byte array containing a hand-built record,
        /// the record can also be built using the <see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/>
        /// and <see cref="T:InTheHand.Net.Bluetooth.ServiceElement"/> classes, and
        /// passed to the respective constuctor, e.g.
        /// <see cref="M:InTheHand.Net.Sockets.WindowsBluetoothListener.#ctor(InTheHand.Net.BluetoothAddress,System.Guid,InTheHand.Net.Bluetooth.ServiceRecord)"/>
        /// </para>
        /// <para>Any useful SDP record will include 
        /// a <c>ProtocolDescriptor</c> element containing
        /// the RFCOMM Channel number that the server is listening on,
        /// and a <c>ServiceClassId</c> element containing the service UUIDs.
        /// The record supplied in the <paramref name="sdpRecord"/> parameter
        /// should contain those elements.  On successful <see 
        /// cref="M:InTheHand.Net.Sockets.WindowsBluetoothListener.Start"/>,
        /// the RFCOMM Channel number that the protocol stack has assigned to the
        /// server is retrieved, and copied into the service record before it is
        /// published.  The <paramref name="channelOffset"/> indicates the location
        /// of the respective byte in the <paramref name="sdpRecord"/> byte array.
        /// </para>
        /// <para>
        /// An example SDP record is as follows.  This is actually the format of the 
        /// generic record used in the other constructors.  For another example see
        /// the code in the <c>ObexListener</c> class.
        /// <code>
        /// // The asterisks note where the Service UUID and the Channel number are
        /// // to be filled in.
        /// byte[] record = new byte[] {
        ///   //Element Sequence:
        ///   0x35,0x27,
        ///     //UInt16: 0x0001  -- ServiceClassIdList
        ///     0x09,0x00,0x01,
        ///     //Element Sequence:
        ///     0x35,0x11,
        ///     //  UUID128: 00000000-0000-0000-0000-000000000000 -- * Service UUID
        ///         0x1c,
        ///           0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00,
        ///           0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00,
        ///     //
        ///     //UInt16: 0x0004  -- ProtocolDescriptorList
        ///     0x09,0x00,0x04,
        ///     //Element Sequence:
        ///     0x35,0x0c,
        ///     //  Element Sequence:
        ///         0x35,0x03,
        ///     //      UUID16: 0x0100  -- L2CAP
        ///             0x19,0x01,0x00,
        ///     //  Element Sequence:
        ///         0x35,0x05,
        ///     //      UUID16: 0x0003  -- RFCOMM
        ///             0x19,0x00,0x03,
        ///     //      UInt8: 0x00     -- * Channel Number
        ///             0x08,0x00
        /// };
        /// </code>
        /// For that record the <c>channelOffset</c> is 40.
        /// </para>
        /// </remarks>
        public void Construct(BluetoothAddress localAddress, Guid service, byte[] sdpRecord, int channelOffset)
        {
            Construct(localAddress, service);
            InitServiceRecord(sdpRecord, channelOffset);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsBluetoothListener"/> class
        /// with the specified local endpoint,
        /// publishing the specified SDP record.
        /// </summary>
        /// <param name="localEP">A <see cref="BluetoothEndPoint"/> that represents the local endpoint to which to bind the listener <see cref="Socket"/>.</param>
        /// <param name="sdpRecord">Prepared SDP Record to publish</param>
        /// <param name="channelOffset">
        /// The index in the <paramref name="sdpRecord"/> byte array where the RFCOMM Channel Number that the
        /// server is listening on is to be placed.
        /// However the supplied record is now parsed into an <see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/>
        /// instance, and the channel offset is not used.
        /// </param>
        /// <remarks>
        /// <note>
        /// The constructors taking the SDP record explicitly (as a byte array) should
        /// only be used if
        /// a specialized SDP record is required. For instance when using one of the
        /// standard profiles.  Otherwise use one of the other constructors 
        /// e.g. <see 
        /// cref="M:InTheHand.Net.Sockets.WindowsBluetoothListener.#ctor(InTheHand.Net.BluetoothEndPoint)"/>
        /// which create a generic SDP Record from the specified service identifier.
        /// </note>
        /// <para>Instead of passing a byte array containing a hand-built record,
        /// the record can also be built using the <see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/>
        /// and <see cref="T:InTheHand.Net.Bluetooth.ServiceElement"/> classes, and
        /// passed to the respective constuctor, e.g.
        /// <see cref="M:InTheHand.Net.Sockets.WindowsBluetoothListener.#ctor(InTheHand.Net.BluetoothEndPoint,InTheHand.Net.Bluetooth.ServiceRecord)"/>
        /// </para>
        /// <para>Any useful SDP record will include 
        /// a <c>ProtocolDescriptor</c> element containing
        /// the RFCOMM Channel number that the server is listening on,
        /// and a <c>ServiceClassId</c> element containing the service UUIDs.
        /// The record supplied in the <paramref name="sdpRecord"/> parameter
        /// should contain those elements.  On successful <see 
        /// cref="M:InTheHand.Net.Sockets.WindowsBluetoothListener.Start"/>,
        /// the RFCOMM Channel number that the protocol stack has assigned to the
        /// server is retrieved, and copied into the service record before it is
        /// published.  The <paramref name="channelOffset"/> indicates the location
        /// of the respective byte in the <paramref name="sdpRecord"/> byte array.
        /// </para>
        /// <para>
        /// An example SDP record is as follows.  This is actually the format of the 
        /// generic record used in the other constructors.  For another example see
        /// the code in the <c>ObexListener</c> class.
        /// <code>
        /// // The asterisks note where the Service UUID and the Channel number are
        /// // to be filled in.
        /// byte[] record = new byte[] {
        ///   //Element Sequence:
        ///   0x35,0x27,
        ///     //UInt16: 0x0001  -- ServiceClassIdList
        ///     0x09,0x00,0x01,
        ///     //Element Sequence:
        ///     0x35,0x11,
        ///     //  UUID128: 00000000-0000-0000-0000-000000000000 -- * Service UUID
        ///         0x1c,
        ///           0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00,
        ///           0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00,
        ///     //
        ///     //UInt16: 0x0004  -- ProtocolDescriptorList
        ///     0x09,0x00,0x04,
        ///     //Element Sequence:
        ///     0x35,0x0c,
        ///     //  Element Sequence:
        ///         0x35,0x03,
        ///     //      UUID16: 0x0100  -- L2CAP
        ///             0x19,0x01,0x00,
        ///     //  Element Sequence:
        ///         0x35,0x05,
        ///     //      UUID16: 0x0003  -- RFCOMM
        ///             0x19,0x00,0x03,
        ///     //      UInt8: 0x00     -- * Channel Number
        ///             0x08,0x00
        /// };
        /// </code>
        /// For that record the <c>channelOffset</c> is 40.
        /// </para>
        /// </remarks>
        public void Construct(BluetoothEndPoint localEP, byte[] sdpRecord, int channelOffset)
        {
            Construct(localEP);
            InitServiceRecord(sdpRecord, channelOffset);
        }
        //----------------

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsBluetoothListener"/> class
        /// to listen on the specified service identifier, 
        /// publishing the specified SDP record.
        /// </summary>
        /// -
        /// <param name="service">The Bluetooth service to listen for.</param>
        /// <param name="sdpRecord">Prepared SDP Record to publish.</param>
        /// -
        /// <remarks>
        /// <note>
        /// The constructors taking the SDP record explicitly should
        /// only be used if
        /// a specialized SDP record is required. For instance when using one of the
        /// standard profiles.  Otherwise use one of the other constructors 
        /// e.g. <see 
        /// cref="M:InTheHand.Net.Sockets.WindowsBluetoothListener.#ctor(System.Guid)"/>
        /// which create a generic SDP Record from the specified service identifier.
        /// </note>
        /// <para>Any useful SDP record will include 
        /// a <c>ProtocolDescriptor</c> element containing
        /// the RFCOMM Channel number that the server is listening on,
        /// and a <c>ServiceClassId</c> element containing the service UUIDs.
        /// The record supplied in the <paramref name="sdpRecord"/> parameter
        /// should contain those elements.  On successful <see 
        /// cref="M:InTheHand.Net.Sockets.WindowsBluetoothListener.Start"/>,
        /// the RFCOMM Channel number that the protocol stack has assigned to the
        /// server is retrieved, and copied into the service record before it is
        /// published.
        /// </para>
        /// <para>
        /// An example SDP record is as follows.  This is actually the format of the 
        /// generic record used in the other constructors.  For another example see
        /// the code in the <c>ObexListener</c> class.
        /// <code lang="C#">
        /// private static ServiceRecord CreateBasicRfcommRecord(Guid serviceClassUuid)
        /// {
        ///     ServiceElement pdl = ServiceRecordHelper.CreateRfcommProtocolDescriptorList();
        ///     ServiceElement classList = new ServiceElement(ElementType.ElementSequence,
        ///         new ServiceElement(ElementType.Uuid128, serviceClassUuid));
        ///     ServiceRecord record = new ServiceRecord(
        ///         new ServiceAttribute(
        ///             InTheHand.Net.Bluetooth.AttributeIds.UniversalAttributeId.ServiceClassIdList,
        ///             classList),
        ///         new ServiceAttribute(
        ///             InTheHand.Net.Bluetooth.AttributeIds.UniversalAttributeId.ProtocolDescriptorList,
        ///             pdl));
        ///     return record;
        /// }
        /// </code>
        /// </para>
        /// </remarks>
        public void Construct(Guid service, ServiceRecord sdpRecord)
        {
            Construct(service);
            InitServiceRecord(sdpRecord);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsBluetoothListener"/> class
        /// that listens for incoming connection attempts on the specified local Bluetooth address and service identifier,
        /// publishing the specified SDP record.
        /// </summary>
        /// -
        /// <param name="localAddress">A <see cref="BluetoothAddress"/> that represents the local Bluetooth radio address.</param>
        /// <param name="service">The Bluetooth service to listen for.</param>
        /// <param name="sdpRecord">Prepared SDP Record to publish</param>
        /// -
        /// <remarks>
        /// <note>
        /// The constructors taking the SDP record explicitly should
        /// only be used if
        /// a specialized SDP record is required. For instance when using one of the
        /// standard profiles.  Otherwise use one of the other constructors 
        /// e.g. <see 
        /// cref="M:InTheHand.Net.Sockets.WindowsBluetoothListener.#ctor(InTheHand.Net.BluetoothAddress,System.Guid)"/>
        /// which create a generic SDP Record from the specified service identifier.
        /// </note>
        /// <para>Any useful SDP record will include 
        /// a <c>ProtocolDescriptor</c> element containing
        /// the RFCOMM Channel number that the server is listening on,
        /// and a <c>ServiceClassId</c> element containing the service UUIDs.
        /// The record supplied in the <paramref name="sdpRecord"/> parameter
        /// should contain those elements.  On successful <see 
        /// cref="M:InTheHand.Net.Sockets.WindowsBluetoothListener.Start"/>,
        /// the RFCOMM Channel number that the protocol stack has assigned to the
        /// server is retrieved, and copied into the service record before it is
        /// published.
        /// </para>
        /// <para>
        /// An example SDP record is as follows.  This is actually the format of the 
        /// generic record used in the other constructors.  For another example see
        /// the code in the <c>ObexListener</c> class.
        /// <code lang="C#">
        /// private static ServiceRecord CreateBasicRfcommRecord(Guid serviceClassUuid)
        /// {
        ///     ServiceElement pdl = ServiceRecordHelper.CreateRfcommProtocolDescriptorList();
        ///     ServiceElement classList = new ServiceElement(ElementType.ElementSequence,
        ///         new ServiceElement(ElementType.Uuid128, serviceClassUuid));
        ///     ServiceRecord record = new ServiceRecord(
        ///         new ServiceAttribute(
        ///             InTheHand.Net.Bluetooth.AttributeIds.UniversalAttributeId.ServiceClassIdList,
        ///             classList),
        ///         new ServiceAttribute(
        ///             InTheHand.Net.Bluetooth.AttributeIds.UniversalAttributeId.ProtocolDescriptorList,
        ///             pdl));
        ///     return record;
        /// }
        /// </code>
        /// </para>
        /// </remarks>
        public void Construct(BluetoothAddress localAddress, Guid service, ServiceRecord sdpRecord)
        {
            Construct(localAddress, service);
            InitServiceRecord(sdpRecord);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsBluetoothListener"/> class
        /// with the specified local endpoint,
        /// publishing the specified SDP record.
        /// </summary>
        /// <param name="localEP">A <see cref="BluetoothEndPoint"/> that represents the local endpoint to which to bind the listener <see cref="Socket"/>.</param>
        /// <param name="sdpRecord">Prepared SDP Record to publish</param>
        /// -
        /// <remarks>
        /// <note>
        /// The constructors taking the SDP record explicitly (as a byte array) should
        /// only be used if
        /// a specialized SDP record is required. For instance when using one of the
        /// standard profiles.  Otherwise use one of the other constructors 
        /// e.g. <see 
        /// cref="M:InTheHand.Net.Sockets.WindowsBluetoothListener.#ctor(InTheHand.Net.BluetoothEndPoint)"/>
        /// which create a generic SDP Record from the specified service identifier.
        /// </note>
        /// <para>Any useful SDP record will include 
        /// a <c>ProtocolDescriptor</c> element containing
        /// the RFCOMM Channel number that the server is listening on,
        /// and a <c>ServiceClassId</c> element containing the service UUIDs.
        /// The record supplied in the <paramref name="sdpRecord"/> parameter
        /// should contain those elements.  On successful <see 
        /// cref="M:InTheHand.Net.Sockets.WindowsBluetoothListener.Start"/>,
        /// the RFCOMM Channel number that the protocol stack has assigned to the
        /// server is retrieved, and copied into the service record before it is
        /// published.
        /// </para>
        /// <para>
        /// An example SDP record is as follows.  This is actually the format of the 
        /// generic record used in the other constructors.  For another example see
        /// the code in the <c>ObexListener</c> class.
        /// <code lang="C#">
        /// private static ServiceRecord CreateBasicRfcommRecord(Guid serviceClassUuid)
        /// {
        ///     ServiceElement pdl = ServiceRecordHelper.CreateRfcommProtocolDescriptorList();
        ///     ServiceElement classList = new ServiceElement(ElementType.ElementSequence,
        ///         new ServiceElement(ElementType.Uuid128, serviceClassUuid));
        ///     ServiceRecord record = new ServiceRecord(
        ///         new ServiceAttribute(
        ///             InTheHand.Net.Bluetooth.AttributeIds.UniversalAttributeId.ServiceClassIdList,
        ///             classList),
        ///         new ServiceAttribute(
        ///             InTheHand.Net.Bluetooth.AttributeIds.UniversalAttributeId.ProtocolDescriptorList,
        ///             pdl));
        ///     return record;
        /// }
        /// </code>
        /// </para>
        /// </remarks>
        public void Construct(BluetoothEndPoint localEP, ServiceRecord sdpRecord)
        {
            Construct(localEP);
            InitServiceRecord(sdpRecord);
        }
        #endregion

        #region Local EndPoint
        /// <summary>
		///  Gets the underlying <see cref="BluetoothEndPoint"/> of the current <see cref="WindowsBluetoothListener"/>.  
		/// </summary>
		public BluetoothEndPoint LocalEndPoint
		{
			get
			{
                if (!active)
                {
                    return serverEP;
                }
                return (BluetoothEndPoint)serverSocket.LocalEndPoint;

			}
		}
		#endregion

        #region Service Class
        /// <summary>
        /// Get or set the Service Class flags that this service adds to the host 
        /// device&#x2019;s Class Of Device field.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>The Class of Device value contains a Device part which describes 
        /// the primary service that the device provides, and a Service part which 
        /// is a set of flags indicating all the service types that the device supports, 
        /// e.g. <see cref="F:InTheHand.Net.Bluetooth.ServiceClass.ObjectTransfer"/>,
        /// <see cref="F:InTheHand.Net.Bluetooth.ServiceClass.Telephony"/>,
        /// <see cref="F:InTheHand.Net.Bluetooth.ServiceClass.Audio"/> etc.
        /// This property supports setting those flags; bits set in this value will be 
        /// added to the host device&#x2019;s CoD Service Class bits when the listener
        /// is active.
        /// </para>
        /// <para><note>Supported on Win32, but not supported on WindowsMobile/WinCE 
        /// as there's no native API for it.  The WindowCE section of MSDN mentions the
        /// Registry value <c>COD</c> at key <c>HKEY_LOCAL_MACHINE\Software\Microsoft\Bluetooth\sys</c>. 
        /// However my (Jam) has value 0x920100 there but advertises a CoD of 0x100114, 
        /// so its not clear how the values relate to each other.
        /// </note>
        /// </para>
        /// </remarks>
        public ServiceClass ServiceClass
        {
            get
            {
                return codService;
            }
            set
            {
                codService = value;
            }
        }

        private String m_serviceName;

        /// <summary>
        /// Get or set the ServiceName the server will use in its SDP Record.
        /// </summary>
        /// -
        /// <value>A string representing the value to be used for the Service Name
        /// SDP Attribute.  Will be <see langword="null"/> if not specfied.
        /// </value>
        /// -
        /// <exception cref="T:System.InvalidOperationException">
        /// The listener is already started.
        /// <para>- or -</para>
        /// A custom Service Record was given at initialization time.  In that case 
        /// the ServiceName attribute should be added to that record.
        /// </exception>
        public String ServiceName
        {
            get { return m_serviceName; }
            set
            {
                if (this.active) {
                    throw new InvalidOperationException("Can not change ServiceName when started.");
                }
                if (m_manualServiceRecord) {
                    throw new InvalidOperationException("ServiceName may not be specified when a custom Service Record is being used.");
                }
                m_serviceName = value;
                InitServiceRecord(this.serverEP.Service);
            }
        }
	
        #endregion

        #region Server

        private Socket serverSocket;

        /// <summary>
		/// Gets the underlying network <see cref="Socket"/>.
		/// </summary>
		/// <value>The underlying <see cref="Socket"/>.</value>
		/// <remarks><see cref="WindowsBluetoothListener"/> creates a <see cref="Socket"/> to listen for incoming client connection requests.
		/// Classes deriving from <see cref="WindowsBluetoothListener"/> can use this property to get this <see cref="Socket"/>.
		/// Use the underlying <see cref="Socket"/> returned by the <see cref="Server"/> property if you require access beyond that which <see cref="WindowsBluetoothListener"/> provides.
		/// <para>Note <see cref="Server"/> property only returns the <see cref="Socket"/> used to listen for incoming client connection requests.
		/// Use the <see cref="AcceptSocket"/> method to accept a pending connection request and obtain a <see cref="Socket"/> for sending and receiving data.
		/// You can also use the <see cref="AcceptBluetoothClient"/> method to accept a pending connection request and obtain a <see cref="BluetoothClient"/> for sending and receiving data.</para></remarks>
		public Socket Server
		{
			get
			{
				return serverSocket;
			}
		}
		#endregion

		#region Start
        /// <summary>
        /// Starts listening for incoming connection requests.
        /// </summary>
        public void Start()
        {
            this.Start(int.MaxValue);
        }
		/// <summary>
        /// Starts listening for incoming connection requests with a maximum number of pending connection.
		/// </summary>
        /// <param name="backlog">The maximum length of the pending connections queue.</param>
        public void Start(int backlog)
		{
            if ((backlog > int.MaxValue) || (backlog < 0))
            {
                throw new ArgumentOutOfRangeException("backlog");
            }
            if (serverSocket == null)
            {
                throw new InvalidOperationException("The socket handle is not valid.");
            }
            if (!active)
            {
                serverEP = PrepareBindEndPoint((BluetoothEndPoint)serverEP.Clone());
                serverSocket.Bind(serverEP);

                //Console.WriteLine("WinBtLsnr BEFORE listen(), lep: " + serverSocket.LocalEndPoint);
                serverSocket.Listen(backlog);
                active = true;

                // (Do this after Listen as BlueZ doesn't assign the port until then).
                byte channelNumber = (byte)((BluetoothEndPoint)serverSocket.LocalEndPoint).Port;
                //Console.WriteLine("WinBtLsnr: lep: " + serverSocket.LocalEndPoint);
                SetService(channelNumber);
            }
		}

        protected virtual BluetoothEndPoint PrepareBindEndPoint(BluetoothEndPoint serverEP)
        {
            return serverEP;
        }

        private void SetService(byte channelNumber)
        {
            byte[] rawRecord = GetServiceRecordBytes(channelNumber);
            SetService(rawRecord, codService);
        }
		#endregion

        #region Stop
        /// <summary>
        /// Stops the socket from monitoring connections.
        /// </summary>
        public void Stop()
        {
            if (serverSocket != null) {
                try {
                    RemoveService();
                } finally {
                    serverSocket.Close();
                    serverSocket = null;
                }
            }

            active = false;
            serverSocket = CreateSocket();
        }

        protected virtual void RemoveService()
        {
            if (serviceHandle != IntPtr.Zero) {
                RemoveService(serviceHandle, m_activeServiceRecordBytes);
                serviceHandle = IntPtr.Zero;
            }
        }
        #endregion

		#region Accept

        #region Async Socket
        /// <summary>
		/// Begins an asynchronous operation to accept an incoming connection attempt.
		/// </summary>
		/// <param name="callback">An <see cref="AsyncCallback"/> delegate that references the method to invoke when the operation is complete.</param>
		/// <param name="state">A user-defined object containing information about the accept operation.
		/// This object is passed to the callback delegate when the operation is complete.</param>
		/// <returns>An <see cref="IAsyncResult"/> that references the asynchronous creation of the <see cref="Socket"/>.</returns>
        /// <exception cref="ObjectDisposedException">The <see cref="Socket"/> has been closed.</exception>
		public IAsyncResult BeginAcceptSocket(AsyncCallback callback, object state)
		{
            if (!active)
            {
                throw new InvalidOperationException("Not listening. You must call the Start() method before calling this method.");
            }
            return serverSocket.BeginAccept(callback, state);
		}

		/// <summary>
		/// Asynchronously accepts an incoming connection attempt and creates a new <see cref="Socket"/> to handle remote host communication.
		/// </summary>
		/// <param name="asyncResult">An <see cref="IAsyncResult"/> returned by a call to the <see cref="BeginAcceptSocket"/> method.</param>
		/// <returns>A <see cref="Socket"/>.</returns>
		public Socket EndAcceptSocket(IAsyncResult asyncResult)
		{
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			return serverSocket.EndAccept(asyncResult);
        }
        #endregion

        #region Async Client
        /// <summary>
		/// Begins an asynchronous operation to accept an incoming connection attempt.
		/// </summary>
		/// <param name="callback"></param>
		/// <param name="state"></param>
		/// <returns></returns>
		public IAsyncResult BeginAcceptBluetoothClient(AsyncCallback callback, object state)
		{
            if (!active)
            {
                throw new InvalidOperationException("Not listening. You must call the Start() method before calling this method.");
            }

			return serverSocket.BeginAccept(callback, state);
		}

		/// <summary>
		/// Asynchronously accepts an incoming connection attempt and creates a new <see cref="BluetoothClient"/> to handle remote host communication.
		/// </summary>
		/// <param name="asyncResult">An <see cref="IAsyncResult"/> returned by a call to the <see cref="BeginAcceptBluetoothClient"/> method.</param>
		/// <returns>A <see cref="BluetoothClient"/>.</returns>
		public IBluetoothClient EndAcceptBluetoothClient(IAsyncResult asyncResult)
		{
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}

            Socket s = serverSocket.EndAccept(asyncResult);
            return MakeIBluetoothClient(s);
        }
        #endregion

        /// <summary>
		/// Creates a new socket for a connection.
		/// </summary>
		/// <remarks>AcceptSocket is a blocking method that returns a <see cref="Socket"/> that you can use to send and receive data.
		/// If you want to avoid blocking, use the <see cref="Pending"/> method to determine if connection requests are available in the incoming connection queue.
		/// <para>The <see cref="Socket"/> returned is initialized with the address and channel number of the remote device.
		/// You can use any of the Send and Receive methods available in the <see cref="Socket"/> class to communicate with the remote device.
		/// When you are finished using the <see cref="Socket"/>, be sure to call its <see cref="Socket.Close()"/> method.
		/// If your application is relatively simple, consider using the <see cref="AcceptBluetoothClient"/> method rather than the AcceptSocket method.
		/// <see cref="BluetoothClient"/> provides you with simple methods for sending and receiving data over a network in blocking synchronous mode.</para></remarks>
		/// <returns>A <see cref="Socket"/> used to send and receive data.</returns>
		/// <exception cref="InvalidOperationException">Listener is stopped.</exception>
		public Socket AcceptSocket()
		{
            if (!active)
            {
                throw new InvalidOperationException("Not listening. You must call the Start() method before calling this method.");
            }

			return serverSocket.Accept();
		}

		/// <summary>
		/// Creates a client object for a connection when the specified service or endpoint is detected by the listener component.
		/// </summary>
		/// <remarks>AcceptTcpClient is a blocking method that returns a <see cref="BluetoothClient"/> that you can use to send and receive data.
		/// Use the <see cref="Pending"/> method to determine if connection requests are available in the incoming connection queue if you want to avoid blocking.
		/// <para>Use the <see cref="BluetoothClient.GetStream"/> method to obtain the underlying <see cref="NetworkStream"/> of the returned <see cref="BluetoothClient"/>.
		/// The <see cref="NetworkStream"/> will provide you with methods for sending and receiving with the remote host.
		/// When you are through with the <see cref="BluetoothClient"/>, be sure to call its <see cref="BluetoothClient.Close"/> method.
		/// If you want greater flexibility than a <see cref="BluetoothClient"/> offers, consider using <see cref="AcceptSocket"/>.</para></remarks>
		/// <returns>A <see cref="BluetoothClient"/> component.</returns>
		/// <exception cref="InvalidOperationException">Listener is stopped.</exception>
        public IBluetoothClient AcceptBluetoothClient()
        {
            Socket s = AcceptSocket();
            return MakeIBluetoothClient(s);
        }

        protected virtual IBluetoothClient MakeIBluetoothClient(Socket s)
        {
            return _fcty.DoGetBluetoothClient(s);
        }
		#endregion

		#region Pending
		/// <summary>
		/// Determines if there is a connection pending.
		/// </summary>
		/// <returns>true if there is a connection pending; otherwise, false.</returns>
		public bool Pending()
		{
            if (!active)
            {
                throw new InvalidOperationException("Not listening. You must call the Start() method before calling this method.");
            }

			return serverSocket.Poll(0, SelectMode.SelectRead);
		}
		#endregion

        #region Service Record
        /// <summary>
        /// Returns the SDP Service Record for this service.
        /// </summary>
        /// <remarks>
        /// <note>Returns <see langword="null"/> if the listener is not 
        /// <see cref="M:InTheHand.Net.Sockets.WindowsBluetoothListener.Start"/>ed
        /// (and an record wasn&#x2019;t supplied at initialization).
        /// </note>
        /// </remarks>
        public ServiceRecord ServiceRecord
        {
            get
            {
                return m_serviceRecord;
            }
        }

        private void InitServiceRecord(Guid serviceClassUuid)
        {
            ServiceRecord record = CreateBasicRfcommRecord(serviceClassUuid, m_serviceName);
            m_serviceRecord = record;
        }

        private static ServiceRecord CreateBasicRfcommRecord(Guid serviceClassUuid, String svcName)
        {
            ServiceRecordBuilder bldr = new ServiceRecordBuilder();
            System.Diagnostics.Debug.Assert(bldr.ProtocolType == BluetoothProtocolDescriptorType.Rfcomm);
            bldr.AddServiceClass(serviceClassUuid);
            if (svcName != null) {
                bldr.ServiceName = svcName;
            }
            return bldr.ServiceRecord;
        }

        private void InitServiceRecord(ServiceRecord sdpRecord)
        {
            if (sdpRecord == null) {
                throw new ArgumentNullException("sdpRecord");
            }
            if (ServiceRecordHelper.GetRfcommChannelNumber(sdpRecord) == -1) {
                throw new ArgumentException("The ServiceRecord must contain a RFCOMM-style ProtocolDescriptorList.");
            }
            m_serviceRecord = sdpRecord;
            m_manualServiceRecord = true;
        }

        private void InitServiceRecord(byte[] sdpRecord, int channelOffset)
        {
            if (sdpRecord.Length == 0) { throw new ArgumentException("sdpRecord must not be empty."); }
            if (channelOffset >= sdpRecord.Length) { throw new ArgumentOutOfRangeException("channelOffset"); }
            //
            // Parse into a ServiceRecord, and discard the array and offset!
            m_serviceRecord = ServiceRecord.CreateServiceRecordFromBytes(sdpRecord);
            m_manualServiceRecord = true;
        }

        // Called at registration time
        private byte[] GetServiceRecordBytes(byte channelNumber)
        {
            ServiceRecord record = m_serviceRecord;
            ServiceRecordHelper.SetRfcommChannelNumber(record, channelNumber);
            m_activeServiceRecordBytes = record.ToByteArray();
            System.Diagnostics.Debug.Assert(m_activeServiceRecordBytes != null);
            return m_activeServiceRecordBytes;
        }
        #endregion

        #region Service
        private static void RemoveService(IntPtr handle, byte[] sdpRecord)
        {
            MicrosoftSdpService.RemoveService(handle, sdpRecord);
        }

        protected virtual void SetService(byte[] sdpRecord, ServiceClass cod)
        {
            serviceHandle = MicrosoftSdpService.SetService(sdpRecord, cod);
        }
        #endregion


        #region Authenticate
        /// <summary>
        /// Gets or sets the authentication state of the current connect or behaviour to use when connection is established.
        /// </summary>
        /// <remarks>
        /// For disconnected sockets, specifies that authentication is required in order for a connect or accept operation to complete successfully.
        /// Setting this option actively initiates authentication during connection establishment, if the two Bluetooth devices were not previously authenticated.
        /// The user interface for passkey exchange, if necessary, is provided by the operating system outside the application context.
        /// For outgoing connections that require authentication, the connect operation fails with WSAEACCES if authentication is not successful.
        /// In response, the application may prompt the user to authenticate the two Bluetooth devices before connection.
        /// For incoming connections, the connection is rejected if authentication cannot be established and returns a WSAEHOSTDOWN error.
        /// </remarks>
        public bool Authenticate
        {
            get { return m_optionHelper.Authenticate; }
            set { m_optionHelper.Authenticate = value; }
        }
        #endregion

        #region Encrypt
        /// <summary>
        /// On unconnected sockets, enforces encryption to establish a connection.
        /// Encryption is only available for authenticated connections.
        /// For incoming connections, a connection for which encryption cannot be established is automatically rejected and returns WSAEHOSTDOWN as the error.
        /// For outgoing connections, the connect function fails with WSAEACCES if encryption cannot be established.
        /// In response, the application may prompt the user to authenticate the two Bluetooth devices before connection.
        /// </summary>
        public bool Encrypt
        {
            get { return m_optionHelper.Encrypt; }
            set { m_optionHelper.Encrypt = value; }
        }
        #endregion

        #region Set PIN
        /// <summary>
        /// Set or change the PIN to be used with a specific remote device.
        /// </summary>
        /// <param name="device">Address of Bluetooth device.</param>
        /// <param name="pin">PIN string consisting of 1 to 16 ASCII characters.</param>
        /// <remarks>Assigning null (Nothing in VB) or an empty String will revoke the PIN.</remarks>
        public void SetPin(BluetoothAddress device, string pin)
        {
            m_optionHelper.SetPin(device, pin);
        }

        public void SetPin(string pin)
        {
#if WinXP
            throw new NotSupportedException("Must supply the remote address on Win32.");
#else
            m_optionHelper.SetPin(null, pin);
#endif
        }
        #endregion

	}
}
