// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Sockets.BluetoothListener
// 
// Copyright (c) 2003-2008 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using InTheHand.Net.Bluetooth;
using System.Diagnostics;
using InTheHand.Net.Bluetooth.Factory;


namespace InTheHand.Net.Sockets
{
	/// <summary>
	/// Listens for connections from Bluetooth RFCOMM network clients.
	/// </summary>
    /// <remarks>
    /// <para>The <see cref="BluetoothListener"/> class provides simple methods 
    /// that listen for and accept incoming connection requests.  New connections 
    /// are returned as <see cref="BluetoothClient"/> instances 
    /// (on Microsoft Bluetooth stack platforms alone a new <see cref="Socket"/> 
    /// instance can be returned for new connections).
    /// </para>
    /// <para>In the normal case a the listener is initialised with a 
    /// <see cref="T:System.Guid"/> holding the Service Class Id on which it is 
    /// to accept connections, the listener will automatically create a SDP 
    /// Service Record containg that Service Class Id and the port number
    /// (RFCOMM Service Channel Number) that it has started listening on.
    /// The standard usage is thus as follows.
    /// </para>
    /// <code lang="VB.NET">
    /// Class MyConsts
    ///   Shared ReadOnly MyServiceUuid As Guid _
    ///     = New Guid("{00112233-4455-6677-8899-aabbccddeeff}")
    /// End Class
    /// 
    ///   ...
    ///   Dim lsnr As New BluetoothListener(MyConsts.MyServiceUuid)
    ///   lsnr.Start()
    ///   ' Now accept new connections, perhaps using the thread pool to handle each
    ///   Dim conn As New BluetoothClient = lsnr.AcceptBluetoothClient()
    ///   Dim peerStream As Stream = conn.GetStream()
    ///   ...
    /// </code>
    /// <para>One can also pass the BluetoothListener a Service Name (v2.4), 
    /// a custom Service Record (Service Discovery Protocol record), and/or 
    /// set Class of Service bit(s). To create a custom Service Record use 
    /// <see cref="T:InTheHand.Net.Bluetooth.ServiceRecordBuilder"/>.
    /// </para>
    /// <para>There are overloads of the constructor which take a 
    /// <see cref="T:InTheHand.Net.BluetoothEndPoint"/> parameter instead of a 
    /// <see cref="T:System.Guid"/> as the Service Class Id, the Class Id
    /// value should be specified in that case in the endpoint.
    /// If the port value is specified in the endpoint, then the listener will 
    /// attempt to bind to that port locally.  The address in the endpoint is 
    /// largely ignored as no current stack supports more than one local radio.
    /// </para>
    /// <para>As of version 3.4 we catch an exception if it occurs on the new 
    /// port set-up and it is stored. That error will be returned to any subsequent 
    /// Accept; that is we assume that the error affects the listener completely 
    /// and so make no attempt to start a new port and all subsequent Accept 
    /// complete with the original error.
    /// </para>
    /// <para>In the Bluetopia case previously the 'one port at a time' error
    /// was unhandled and occurred on a background thread and therefore killed
    /// the application.  Now it is caught and returned to the next Accept.
    /// Even better the first Accept successfully returns back to the caller.
    /// So BluetoothListener is now usable to that extent: one connection can
    /// be accepted.  After that it needs to be discarded and a new server created.
    /// </para>
    /// </remarks>
	public class BluetoothListener
	{
        readonly IBluetoothListener m_impl;

        #region Constructor
        private BluetoothListener(BluetoothFactory factory)
        {
            m_impl = factory.DoGetBluetoothListener();
            m_impl.ToString(); // A check for null pointer!
        }

        private static BluetoothFactory DefaultBluetoothFactory
        {
            get { return BluetoothFactory.Factory; }
        }

        /// <overloads>
        /// Initializes a new instance of the <see cref="BluetoothListener"/> class.
        /// </overloads>
        /// ----
        /// <summary>
        /// Initializes a new instance of the <see cref="BluetoothListener"/> class
        /// to listen on the specified service identifier.
        /// </summary>
        /// <param name="service">The Bluetooth service to listen for.</param>
        /// <remarks>
        /// <para>
        /// An SDP record is published on successful <see cref="M:InTheHand.Net.Sockets.BluetoothListener.Start"/>
        /// to advertise the server.
        /// A generic record is created, containing the essential <c>ServiceClassIdList</c>
        /// and <c>ProtocolDescriptorList</c> attributes.  The specified service identifier is
        /// inserted into the former, and the RFCOMM Channel number that the server is
        /// listening on is inserted into the latter.  See the Bluetooth SDP specification
        /// for details on the use and format of SDP records.
        /// </para><para>
        /// If a SDP record with more elements is required, then use
        /// one of the other constructors that takes an SDP record e.g. 
        /// <see cref="M:InTheHand.Net.Sockets.BluetoothListener.#ctor(System.Guid,InTheHand.Net.Bluetooth.ServiceRecord)"/>,
        /// or when passing it as a byte array 
        /// <see cref="M:InTheHand.Net.Sockets.BluetoothListener.#ctor(System.Guid,System.Byte[],System.Int32)"/>.
        /// The format of the generic record used here is shown there also.
        /// </para><para>
        /// Call the <see cref="M:InTheHand.Net.Sockets.BluetoothListener.Start"/> 
        /// method to begin listening for incoming connection attempts.
        /// </para>
        /// </remarks>
        public BluetoothListener(Guid service)
            : this(DefaultBluetoothFactory, service)
        {
        }

        internal BluetoothListener(BluetoothFactory factory, Guid service)
            : this(factory)
        {
            m_impl.Construct(service);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BluetoothListener"/> class
        /// that listens for incoming connection attempts on the specified local Bluetooth address and service identifier. 
        /// </summary>
        /// <param name="localaddr">A <see cref="BluetoothAddress"/> that represents the local Bluetooth radio address.</param>
        /// <param name="service">The Bluetooth service on which to listen for incoming connection attempts.</param>
        /// <remarks>
        /// <para>
        /// An SDP record is published on successful <see cref="M:InTheHand.Net.Sockets.BluetoothListener.Start"/>
        /// to advertise the server.
        /// A generic record is created, containing the essential <c>ServiceClassIdList</c>
        /// and <c>ProtocolDescriptorList</c> attributes.  The specified service identifier is
        /// inserted into the former, and the RFCOMM Channel number that the server is
        /// listening on is inserted into the latter.  See the Bluetooth SDP specification
        /// for details on the use and format of SDP records.
        /// </para><para>
        /// If a SDP record with more elements is required, then use
        /// one of the other constructors that takes an SDP record e.g. 
        /// <see cref="M:InTheHand.Net.Sockets.BluetoothListener.#ctor(InTheHand.Net.BluetoothAddress,System.Guid,InTheHand.Net.Bluetooth.ServiceRecord)"/>,
        /// or when passing it as a byte array, e.g. 
        /// <see cref="M:InTheHand.Net.Sockets.BluetoothListener.#ctor(InTheHand.Net.BluetoothAddress,System.Guid,System.Byte[],System.Int32)"/>.
        /// The format of the generic record used here is shown there also.
        /// </para><para>
        /// Call the <see cref="M:InTheHand.Net.Sockets.BluetoothListener.Start"/> 
        /// method to begin listening for incoming connection attempts.
        /// </para>
        /// </remarks>
        public BluetoothListener(BluetoothAddress localaddr, Guid service)
            : this(DefaultBluetoothFactory, localaddr, service)
        {
        }

        internal BluetoothListener(BluetoothFactory factory, BluetoothAddress localaddr, Guid service)
            : this(factory)
        {
            if (localaddr == null) {
                throw new ArgumentNullException("localaddr");
            }
            if (service == Guid.Empty) {
                throw new ArgumentNullException("service");
            }
            m_impl.Construct(localaddr, service);
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="BluetoothListener"/> class
        /// with the specified local endpoint.
		/// </summary>
        /// -
        /// <param name="localEP">A <see cref="BluetoothEndPoint"/> that represents 
        /// the local endpoint to which to bind the listener.
        /// See the <see cref="BluetoothListener"/> documentation for more information 
        /// on the usage of this argument.
        /// </param>
        /// -
        /// <remarks>
        /// <para>
        /// An SDP record is published on successful <see cref="M:InTheHand.Net.Sockets.BluetoothListener.Start"/>
        /// to advertise the server.
        /// A generic record is created, containing the essential <c>ServiceClassIdList</c>
        /// and <c>ProtocolDescriptorList</c> attributes.  The specified service identifier is
        /// inserted into the former, and the RFCOMM Channel number that the server is
        /// listening on is inserted into the latter.  See the Bluetooth SDP specification
        /// for details on the use and format of SDP records.
        /// </para><para>
        /// If a SDP record with more elements is required, then use
        /// one of the other constructors that takes an SDP record e.g. 
        /// <see cref="M:InTheHand.Net.Sockets.BluetoothListener.#ctor(InTheHand.Net.BluetoothEndPoint,InTheHand.Net.Bluetooth.ServiceRecord)"/>,
        /// or when passing it as a byte array
        /// <see cref="M:InTheHand.Net.Sockets.BluetoothListener.#ctor(InTheHand.Net.BluetoothEndPoint,System.Byte[],System.Int32)"/>.
        /// The format of the generic record used here is shown there also.
        /// </para><para>
        /// Call the <see cref="M:InTheHand.Net.Sockets.BluetoothListener.Start"/> 
        /// method to begin listening for incoming connection attempts.
        /// </para>
        /// </remarks>
        public BluetoothListener(BluetoothEndPoint localEP)
            : this(DefaultBluetoothFactory, localEP)
        {
        }

        internal BluetoothListener(BluetoothFactory factory, BluetoothEndPoint localEP)
            : this(factory)
        {
            if (localEP == null)
            {
                throw new ArgumentNullException("localEP");
            }
            m_impl.Construct(localEP);
        }
        //----------------
        
        /// <summary>
		/// Initializes a new instance of the <see cref="BluetoothListener"/> class
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
        /// cref="M:InTheHand.Net.Sockets.BluetoothListener.#ctor(System.Guid)"/>
        /// which create a generic SDP Record from the specified service identifier.
        /// </note>
        /// <para>Instead of passing a byte array containing a hand-built record,
        /// the record can also be built using the <see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/>
        /// and <see cref="T:InTheHand.Net.Bluetooth.ServiceElement"/> classes, and
        /// passed to the respective constuctor, e.g.
        /// <see cref="M:InTheHand.Net.Sockets.BluetoothListener.#ctor(System.Guid,InTheHand.Net.Bluetooth.ServiceRecord)"/>
        /// </para>
        /// <para>Any useful SDP record will include 
        /// a <c>ProtocolDescriptor</c> element containing
        /// the RFCOMM Channel number that the server is listening on,
        /// and a <c>ServiceClassId</c> element containing the service UUIDs.
        /// The record supplied in the <paramref name="sdpRecord"/> parameter
        /// should contain those elements.  On successful <see 
        /// cref="M:InTheHand.Net.Sockets.BluetoothListener.Start"/>,
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
        public BluetoothListener(Guid service, byte[] sdpRecord, int channelOffset)
            : this(DefaultBluetoothFactory, service, sdpRecord, channelOffset)
        {
        }

        internal BluetoothListener(BluetoothFactory factory, Guid service, byte[] sdpRecord, int channelOffset)
            : this(factory)
        {
            m_impl.Construct(service, sdpRecord, channelOffset);
		}
        
        /// <summary>
        /// Initializes a new instance of the <see cref="BluetoothListener"/> class
        /// that listens for incoming connection attempts on the specified local Bluetooth address and service identifier,
        /// publishing the specified SDP record.
        /// </summary>
        /// <param name="localaddr">A <see cref="BluetoothAddress"/> that represents the local Bluetooth radio address.</param>
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
        /// cref="M:InTheHand.Net.Sockets.BluetoothListener.#ctor(InTheHand.Net.BluetoothAddress,System.Guid)"/>
        /// which create a generic SDP Record from the specified service identifier.
        /// </note>
        /// <para>Instead of passing a byte array containing a hand-built record,
        /// the record can also be built using the <see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/>
        /// and <see cref="T:InTheHand.Net.Bluetooth.ServiceElement"/> classes, and
        /// passed to the respective constuctor, e.g.
        /// <see cref="M:InTheHand.Net.Sockets.BluetoothListener.#ctor(InTheHand.Net.BluetoothAddress,System.Guid,InTheHand.Net.Bluetooth.ServiceRecord)"/>
        /// </para>
        /// <para>Any useful SDP record will include 
        /// a <c>ProtocolDescriptor</c> element containing
        /// the RFCOMM Channel number that the server is listening on,
        /// and a <c>ServiceClassId</c> element containing the service UUIDs.
        /// The record supplied in the <paramref name="sdpRecord"/> parameter
        /// should contain those elements.  On successful <see 
        /// cref="M:InTheHand.Net.Sockets.BluetoothListener.Start"/>,
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
        public BluetoothListener(BluetoothAddress localaddr, Guid service, byte[] sdpRecord, int channelOffset)
            : this(DefaultBluetoothFactory, localaddr, service, sdpRecord, channelOffset)
        {
        }

        internal BluetoothListener(BluetoothFactory factory, BluetoothAddress localaddr, Guid service, byte[] sdpRecord, int channelOffset)
            : this(factory)
        {
            m_impl.Construct(localaddr, service, sdpRecord, channelOffset);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BluetoothListener"/> class
        /// with the specified local endpoint,
        /// publishing the specified SDP record.
        /// </summary>
        /// -
        /// <param name="localEP">A <see cref="BluetoothEndPoint"/> that represents 
        /// the local endpoint to which to bind the listener.
        /// See the <see cref="BluetoothListener"/> documentation for more information 
        /// on the usage of this argument.
        /// </param>
        /// <param name="sdpRecord">Prepared SDP Record to publish</param>
        /// <param name="channelOffset">
        /// The index in the <paramref name="sdpRecord"/> byte array where the RFCOMM Channel Number that the
        /// server is listening on is to be placed.
        /// However the supplied record is now parsed into an <see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/>
        /// instance, and the channel offset is not used.
        /// </param>
        /// -
        /// <remarks>
        /// <note>
        /// The constructors taking the SDP record explicitly (as a byte array) should
        /// only be used if
        /// a specialized SDP record is required. For instance when using one of the
        /// standard profiles.  Otherwise use one of the other constructors 
        /// e.g. <see 
        /// cref="M:InTheHand.Net.Sockets.BluetoothListener.#ctor(InTheHand.Net.BluetoothEndPoint)"/>
        /// which create a generic SDP Record from the specified service identifier.
        /// </note>
        /// <para>Instead of passing a byte array containing a hand-built record,
        /// the record can also be built using the <see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/>
        /// and <see cref="T:InTheHand.Net.Bluetooth.ServiceElement"/> classes, and
        /// passed to the respective constuctor, e.g.
        /// <see cref="M:InTheHand.Net.Sockets.BluetoothListener.#ctor(InTheHand.Net.BluetoothEndPoint,InTheHand.Net.Bluetooth.ServiceRecord)"/>
        /// </para>
        /// <para>Any useful SDP record will include 
        /// a <c>ProtocolDescriptor</c> element containing
        /// the RFCOMM Channel number that the server is listening on,
        /// and a <c>ServiceClassId</c> element containing the service UUIDs.
        /// The record supplied in the <paramref name="sdpRecord"/> parameter
        /// should contain those elements.  On successful <see 
        /// cref="M:InTheHand.Net.Sockets.BluetoothListener.Start"/>,
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
        public BluetoothListener(BluetoothEndPoint localEP, byte[] sdpRecord, int channelOffset)
            : this(DefaultBluetoothFactory, localEP, sdpRecord, channelOffset)
        {
        }

        internal BluetoothListener(BluetoothFactory factory, BluetoothEndPoint localEP, byte[] sdpRecord, int channelOffset)
            : this(factory)
        {
            m_impl.Construct(localEP, sdpRecord, channelOffset);
        }
        //----------------

        /// <summary>
        /// Initializes a new instance of the <see cref="BluetoothListener"/> class
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
        /// cref="M:InTheHand.Net.Sockets.BluetoothListener.#ctor(System.Guid)"/>
        /// which create a generic SDP Record from the specified service identifier.
        /// </note>
        /// <para>Any useful SDP record will include 
        /// a <c>ProtocolDescriptor</c> element containing
        /// the RFCOMM Channel number that the server is listening on,
        /// and a <c>ServiceClassId</c> element containing the service UUIDs.
        /// The record supplied in the <paramref name="sdpRecord"/> parameter
        /// should contain those elements.  On successful <see 
        /// cref="M:InTheHand.Net.Sockets.BluetoothListener.Start"/>,
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
        public BluetoothListener(Guid service, ServiceRecord sdpRecord)
            : this(DefaultBluetoothFactory, service, sdpRecord)
        {
        }

        internal BluetoothListener(BluetoothFactory factory, Guid service, ServiceRecord sdpRecord)
            : this(factory)
        {
            m_impl.Construct(service, sdpRecord);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BluetoothListener"/> class
        /// that listens for incoming connection attempts on the specified local Bluetooth address and service identifier,
        /// publishing the specified SDP record.
        /// </summary>
        /// -
        /// <param name="localaddr">A <see cref="BluetoothAddress"/> that represents the local Bluetooth radio address.</param>
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
        /// cref="M:InTheHand.Net.Sockets.BluetoothListener.#ctor(InTheHand.Net.BluetoothAddress,System.Guid)"/>
        /// which create a generic SDP Record from the specified service identifier.
        /// </note>
        /// <para>Any useful SDP record will include 
        /// a <c>ProtocolDescriptor</c> element containing
        /// the RFCOMM Channel number that the server is listening on,
        /// and a <c>ServiceClassId</c> element containing the service UUIDs.
        /// The record supplied in the <paramref name="sdpRecord"/> parameter
        /// should contain those elements.  On successful <see 
        /// cref="M:InTheHand.Net.Sockets.BluetoothListener.Start"/>,
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
        public BluetoothListener(BluetoothAddress localaddr, Guid service, ServiceRecord sdpRecord)
            : this(DefaultBluetoothFactory, localaddr, service, sdpRecord)
        {
        }

        internal BluetoothListener(BluetoothFactory factory, BluetoothAddress localaddr, Guid service, ServiceRecord sdpRecord)
            : this(factory)
        {
            m_impl.Construct(localaddr, service, sdpRecord);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BluetoothListener"/> class
        /// with the specified local endpoint,
        /// publishing the specified SDP record.
        /// </summary>
        /// -
        /// <param name="localEP">A <see cref="BluetoothEndPoint"/> that represents 
        /// the local endpoint to which to bind the listener.
        /// See the <see cref="BluetoothListener"/> documentation for more information 
        /// on the usage of this argument.
        /// </param>
        /// <param name="sdpRecord">Prepared SDP Record to publish</param>
        /// -
        /// <remarks>
        /// <note>
        /// The constructors taking the SDP record explicitly (as a byte array) should
        /// only be used if
        /// a specialized SDP record is required. For instance when using one of the
        /// standard profiles.  Otherwise use one of the other constructors 
        /// e.g. <see 
        /// cref="M:InTheHand.Net.Sockets.BluetoothListener.#ctor(InTheHand.Net.BluetoothEndPoint)"/>
        /// which create a generic SDP Record from the specified service identifier.
        /// </note>
        /// <para>Any useful SDP record will include 
        /// a <c>ProtocolDescriptor</c> element containing
        /// the RFCOMM Channel number that the server is listening on,
        /// and a <c>ServiceClassId</c> element containing the service UUIDs.
        /// The record supplied in the <paramref name="sdpRecord"/> parameter
        /// should contain those elements.  On successful <see 
        /// cref="M:InTheHand.Net.Sockets.BluetoothListener.Start"/>,
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
        public BluetoothListener(BluetoothEndPoint localEP, ServiceRecord sdpRecord)
            : this(DefaultBluetoothFactory, localEP, sdpRecord)
        {
        }

        internal BluetoothListener(BluetoothFactory factory, BluetoothEndPoint localEP, ServiceRecord sdpRecord)
            : this(factory)
        {
            m_impl.Construct(localEP, sdpRecord);
        }
        #endregion

        #region Local EndPoint
        /// <summary>
        /// Gets the local endpoint.
        /// </summary>
        /// -
        /// <value>The <see cref="T:InTheHand.Net.BluetoothEndPoint"/>
        /// that the listener is using for communications.
        /// </value>
        /// -
        /// <remarks>
        /// <para>The <see cref="P:InTheHand.Net.BluetoothEndPoint.Port"/> 
        /// property of the endpoint will contain the port number (RFCOMM Channel 
        /// Number) that the listener is listening on.
        /// On some platforms, the <see cref="P:InTheHand.Net.BluetoothEndPoint.Address"/>
        /// is similarly set, or is <see cref="F:InTheHand.Net.BluetoothAddress.None"/> 
        /// if not known.  The endpoint&#x2019;s <see cref="P:InTheHand.Net.BluetoothEndPoint.Service"/>
        /// is never set.
        /// </para>
        /// </remarks>
		public BluetoothEndPoint LocalEndPoint
		{
            get { return m_impl.LocalEndPoint; }
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
        /// <strong>added</strong> to the host device&#x2019;s CoD Service Class bits when the listener
        /// is active.  For Win32 see <see href="http://msdn.microsoft.com/en-us/library/aa362940(VS.85).aspx">MSDN &#x2014; BTH_SET_SERVICE Structure</see>
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
            get { return m_impl.ServiceClass; }
            set { m_impl.ServiceClass = value; }
        }

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
            get { return m_impl.ServiceName; }
            set { m_impl.ServiceName = value; }
        }
	
        #endregion

        #region Server

        /// <summary>
		/// Gets the underlying network <see cref="Socket"/>.
		/// </summary>
        /// -
		/// <value>The underlying network <see cref="Socket"/>.</value>
        /// -
		/// <remarks>
        /// <note>The property is only supported on Microsoft Bluetooth stack platforms.
        /// </note>
        /// <para><see cref="BluetoothListener"/> creates a <see cref="Socket"/> to listen for incoming client connection requests.
		/// Classes deriving from <see cref="BluetoothListener"/> can use this property to get this <see cref="Socket"/>.
		/// Use the underlying <see cref="Socket"/> returned by the <see cref="Server"/> property if you require access beyond that which <see cref="BluetoothListener"/> provides.
        /// </para>
		/// <para>Note <see cref="Server"/> property only returns the <see cref="Socket"/> used to listen for incoming client connection requests.
		/// Use the <see cref="AcceptSocket"/> method to accept a pending connection request and obtain a <see cref="Socket"/> for sending and receiving data.
		/// You can also use the <see cref="AcceptBluetoothClient"/> method to accept a pending connection request and obtain a <see cref="BluetoothClient"/> for sending and receiving data.
        /// </para>
        /// </remarks>
		public Socket Server
		{
            get { return m_impl.Server; }
		}
		#endregion

		#region Start
        /// <summary>
        /// Starts listening for incoming connection requests.
        /// </summary>
        public void Start()
        {
            m_impl.Start();
        }
		/// <summary>
        /// Starts listening for incoming connection requests with a maximum number of pending connection.
		/// </summary>
        /// <param name="backlog">The maximum length of the pending connections queue.</param>
        public void Start(int backlog)
		{
            m_impl.Start(backlog);
        }
		#endregion

		#region Stop
		/// <summary>
		/// Stops the socket from monitoring connections.
		/// </summary>
		public void Stop()
		{
            m_impl.Stop();
		}
		#endregion

		#region Accept

        #region Async Socket
        /// <summary>
		/// Begins an asynchronous operation to accept an incoming connection attempt.
		/// </summary>
        /// -
        /// <remarks>
        /// <note>The method is only supported on Microsoft Bluetooth stack platforms.
        /// </note>
        /// </remarks>
        /// -
		/// <param name="callback">An <see cref="AsyncCallback"/> delegate that references the method to invoke when the operation is complete.</param>
		/// <param name="state">A user-defined object containing information about the accept operation.
		/// This object is passed to the callback delegate when the operation is complete.</param>
        /// -
        /// <returns>An <see cref="T:System.IAsyncResult"/> that represents the 
        /// asynchronous accept, which could still be pending.
        /// </returns>
        /// -
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="Socket"/> has been closed.</exception>
		public IAsyncResult BeginAcceptSocket(AsyncCallback callback, object state)
		{
            return m_impl.BeginAcceptSocket(callback, state);
        }

		/// <summary>
		/// Asynchronously accepts an incoming connection attempt and creates a new <see cref="Socket"/> to handle remote host communication.
		/// </summary>
		/// <param name="asyncResult">An <see cref="IAsyncResult"/> returned by a call to the <see cref="BeginAcceptSocket"/> method.</param>
		/// <returns>A <see cref="Socket"/>.</returns>
		public Socket EndAcceptSocket(IAsyncResult asyncResult)
		{
            return m_impl.EndAcceptSocket(asyncResult);
        }

#if FX4
        public System.Threading.Tasks.Task<Socket> AcceptSocketAsync(object state)
        {
            return System.Threading.Tasks.Task.Factory.FromAsync<Socket>(
                BeginAcceptSocket, EndAcceptSocket,
                state);
        }
#endif
        #endregion

        #region Async Client
        /// <summary>
		/// Begins an asynchronous operation to accept an incoming connection attempt.
		/// </summary>
        /// -
        /// <param name="callback">An <see cref="AsyncCallback"/> delegate that references the method to invoke when the operation is complete.</param>
        /// <param name="state">A user-defined object containing information about the accept operation.
        /// This object is passed to the callback delegate when the operation is complete.</param>
        /// -
        /// <returns>An <see cref="T:System.IAsyncResult"/> that represents the 
        /// asynchronous accept, which could still be pending.
        /// </returns>
        public IAsyncResult BeginAcceptBluetoothClient(AsyncCallback callback, object state)
		{
            return m_impl.BeginAcceptBluetoothClient(callback, state);
		}

		/// <summary>
		/// Asynchronously accepts an incoming connection attempt and creates a new <see cref="BluetoothClient"/> to handle remote host communication.
		/// </summary>
		/// <param name="asyncResult">An <see cref="IAsyncResult"/> returned by a call to the <see cref="BeginAcceptBluetoothClient"/> method.</param>
		/// <returns>A <see cref="BluetoothClient"/>.</returns>
		public BluetoothClient EndAcceptBluetoothClient(IAsyncResult asyncResult)
		{
            return new BluetoothClient(m_impl.EndAcceptBluetoothClient(asyncResult));
        }

#if FX4
        public System.Threading.Tasks.Task<BluetoothClient> AcceptBluetoothClientAsync(object state)
        {
            return System.Threading.Tasks.Task.Factory.FromAsync<BluetoothClient>(
                BeginAcceptBluetoothClient, EndAcceptBluetoothClient,
                state);
        }
#endif
        #endregion

        /// <summary>
		/// Creates a new socket for a connection.
		/// </summary>
        /// -
        /// <remarks>
        /// <note>The method is only supported on Microsoft Bluetooth stack platforms.
        /// </note>
        /// <para>AcceptSocket is a blocking method that returns a <see cref="Socket"/> that you can use to send and receive data.
		/// If you want to avoid blocking, use the <see cref="Pending"/> method to determine if connection requests are available in the incoming connection queue.
        /// </para>
		/// <para>The <see cref="Socket"/> returned is initialized with the address and channel number of the remote device.
		/// You can use any of the Send and Receive methods available in the <see cref="Socket"/> class to communicate with the remote device.
		/// When you are finished using the <see cref="Socket"/>, be sure to call its <see cref="Socket.Close()"/> method.
		/// If your application is relatively simple, consider using the <see cref="AcceptBluetoothClient"/> method rather than the AcceptSocket method.
		/// <see cref="BluetoothClient"/> provides you with simple methods for sending and receiving data over a network in blocking synchronous mode.</para></remarks>
		/// <returns>A <see cref="Socket"/> used to send and receive data.</returns>
		/// <exception cref="T:System.InvalidOperationException">Listener is stopped.</exception>
		public Socket AcceptSocket()
		{
            return m_impl.AcceptSocket();
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
		/// <exception cref="T:System.InvalidOperationException">Listener is stopped.</exception>
		public BluetoothClient AcceptBluetoothClient()
		{
            IBluetoothClient cli0 = m_impl.AcceptBluetoothClient();
            BluetoothClient cli = new BluetoothClient(cli0);
            return cli;
        }
		#endregion

		#region Pending
		/// <summary>
		/// Determines if there is a connection pending.
		/// </summary>
		/// <returns>true if there is a connection pending; otherwise, false.</returns>
		public bool Pending()
		{
            return m_impl.Pending();
        }
		#endregion

        #region Service Record
        /// <summary>
        /// Returns the SDP Service Record for this service.
        /// </summary>
        /// <remarks>
        /// <note>Returns <see langword="null"/> if the listener is not 
        /// <see cref="M:InTheHand.Net.Sockets.BluetoothListener.Start"/>ed
        /// (and an record wasn&#x2019;t supplied at initialization).
        /// </note>
        /// </remarks>
        public ServiceRecord ServiceRecord
        {
            get { return m_impl.ServiceRecord; }
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
            get { return m_impl.Authenticate; }
            set { m_impl.Authenticate = value; }
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
            get { return m_impl.Encrypt; }
            set { m_impl.Encrypt = value; }
        }
        #endregion

        #region Set PIN
        ///// <summary>
        ///// Sets the PIN associated with the currently connected device.
        ///// </summary>
        ///// <param name="pin">PIN which must be composed of 1 to 16 ASCII characters.</param>
        ///// <remarks>Assigning null (Nothing in VB) or an empty String will revoke the PIN.</remarks>
        //TODO ????public void SetPin(string pin)
        //{
        //    m_impl.SetPin(pin);
        //}

        /// <summary>
        /// Set or change the PIN to be used with a specific remote device.
        /// </summary>
        /// <param name="device">Address of Bluetooth device.</param>
        /// <param name="pin">PIN string consisting of 1 to 16 ASCII characters.</param>
        /// <remarks>Assigning null (Nothing in VB) or an empty String will revoke the PIN.</remarks>
        public void SetPin(BluetoothAddress device, string pin)
        {
            m_impl.SetPin(device, pin);
        }
        #endregion

        #region Host To Network Order
        internal static Guid HostToNetworkOrder(Guid hostGuid)
		{
			byte[] guidBytes = hostGuid.ToByteArray();
			
			BitConverter.GetBytes(IPAddress.HostToNetworkOrder(BitConverter.ToInt32(guidBytes, 0))).CopyTo(guidBytes, 0);
			BitConverter.GetBytes(IPAddress.HostToNetworkOrder(BitConverter.ToInt16(guidBytes, 4))).CopyTo(guidBytes, 4);
			BitConverter.GetBytes(IPAddress.HostToNetworkOrder(BitConverter.ToInt16(guidBytes, 6))).CopyTo(guidBytes, 6);
			
			return new Guid(guidBytes);
		}
		#endregion

	}
}
