// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Sockets.BluetoothClient
// 
// Copyright (c) 2003-2008 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using InTheHand.Net.Bluetooth;
using Microsoft.Win32;
using System.Diagnostics;
using InTheHand.Net.Bluetooth.Factory;
#if !V1
using List_IBluetoothDeviceInfo = System.Collections.Generic.List<InTheHand.Net.Bluetooth.Factory.IBluetoothDeviceInfo>;
using System.Diagnostics.CodeAnalysis;
#else
using List_IBluetoothDeviceInfo = System.Collections.ArrayList;
#endif

namespace InTheHand.Net.Sockets
{
    /// <summary>
    /// Provides client connections for Bluetooth RFCOMM network services.
    /// </summary>
    /// <remarks>
    /// <note>This class currently only supports devices which use the Microsoft 
    /// and Widcomm Bluetooth stacks, devices which use the other stacks will 
    /// not work.
    /// </note>
    /// <!--This para is in both the class remarks and in Connect(BtEndPoint)-->
    /// <para>When connecting
    /// normally an endpoint with an Address and a Service Class Id 
    /// is specified, then the system will automatically lookup the SDP 
    /// record on the remote device for that service class and connect to 
    /// the port number (RFCOMM Channel Number) specified there.
    /// If instead a port value is provided in the endpoint then the SDP 
    /// lookup will be skipped and  the system will connect to the specified 
    /// port directly.
    /// </para>
    /// <para>Note: Do not attempt to connect with service
    /// <see cref="F:InTheHand.Net.Bluetooth.BluetoothService.RFCommProtocol">BluetoothService.RFCommProtocol</see>
    /// this class always uses RFCOMM, instead the Service Class Id of the 
    /// particular service to which you want to connect must be specified,
    /// perhaps
    /// <see cref="F:InTheHand.Net.Bluetooth.BluetoothService.SerialPort">BluetoothService.SerialPort</see>,
    /// <see cref="F:InTheHand.Net.Bluetooth.BluetoothService.ObexObjectPush">BluetoothService.ObexObjectPush</see>,
    /// or the unique UUID/<see cref="T:System.Guid"/> that you are using in
    /// your custom server application.
    /// </para>
    /// </remarks>
    [System.Diagnostics.DebuggerDisplay("impl={m_impl}")]
    public class BluetoothClient : IDisposable
    {
        readonly IBluetoothClient m_impl;

#if TEST_EARLY && ! V1
        [Obsolete("(need to remove this one)")]
        public ISdpDiscoveryRecordsBuffer WidcommHack__GetServiceRecordsUnparsed(BluetoothAddress address, Guid serviceGuid)
        {
            WidcommBtInterface btIf = ((WidcommBluetoothClient)m_impl).m_btIf___HACK;
            IAsyncResult ar = btIf.BeginServiceDiscovery(address, serviceGuid, null, null);
            ISdpDiscoveryRecordsBuffer recs = btIf.EndServiceDiscovery(ar);
            return recs;
        }

        [Obsolete("(need to remove this one)")]
        public BOND_RETURN_CODE WidcommHack__SetPinX(BluetoothAddress address, string pin)
        {
            return ((WidcommBluetoothSecurity)WidcommBluetoothFactory.Factory.GetBluetoothSecurity()).Bond_(address, pin);
        }
#endif


        #region Constructor
#if NETCF
        static BluetoothClient()
        {
            InTheHand.Net.PlatformVerification.ThrowException();
        }
#endif

        internal BluetoothClient(IBluetoothClient impl)
        {
            m_impl = impl;
        }

        /// <summary>
        /// Creates a new instance of <see cref="BluetoothClient"/>.
        /// </summary>
        public BluetoothClient()
            : this(BluetoothFactory.Factory)
        {
        }

        internal BluetoothClient(BluetoothFactory factory)
            : this(factory.DoGetBluetoothClient())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BluetoothClient"/> class and binds it to the specified local endpoint.
        /// </summary>
        /// <param name="localEP">The <see cref="BluetoothEndPoint"/> to which you bind the Bluetooth Socket.
        /// Only necessary on multi-radio system where you want to select the local radio to use.</param>
        public BluetoothClient(BluetoothEndPoint localEP)
            : this(BluetoothFactory.Factory, localEP)
        {
        }

        internal BluetoothClient(BluetoothFactory factory, BluetoothEndPoint localEP)
            : this(factory.DoGetBluetoothClient(localEP))
        {
        }

        #endregion


        #region InquiryAccessCode
        /// <summary>
        /// Get or set the Device Discovery Inquiry Access Code.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>This is supported only the Microsoft stack on WindowsMobile/etc.
        /// It is not supported on any other platforms.
        /// </para>
        /// <para>The default value is
        /// <see cref="F:InTheHand.Net.BluetoothAddress.Giac">GIAC</see> (0x9E8B33).
        /// See also constant 
        /// <see cref="F:InTheHand.Net.BluetoothAddress.Liac">LIAC</see> (0x9E8B00).
        /// The valid range is 0x9E8B00 through 0x9E8B3f.
        /// </para>
        /// </remarks>
        /// -
        /// <value>An <see cref="T:System.Int32"/> containing the Access Code
        /// to be used for Inquiry.
        /// </value>
        public int InquiryAccessCode
        {
            [DebuggerStepThrough]
            get { return m_impl.InquiryAccessCode; }
            [DebuggerStepThrough]
            set { m_impl.InquiryAccessCode = value; }
        }
        #endregion

        #region Query Length

        /// <summary>
        /// Amount of time allowed to perform the query.
        /// </summary>
        /// <remarks>On Windows CE the actual value used is expressed in units of 1.28 seconds, so will be the nearest match for the value supplied.
        /// The default value is 10 seconds. The maximum is 60 seconds.</remarks>
        public TimeSpan InquiryLength
        {
            [DebuggerStepThrough]
            get { return m_impl.InquiryLength; }
            [DebuggerStepThrough]
            set { m_impl.InquiryLength = value; }
        }
        #endregion

        #region Discover Devices
        /// <summary>
        /// Discovers accessible Bluetooth devices, both remembered and in-range,
        /// and returns their names and addresses.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>This is equivalent to calling
        /// <see cref="M:InTheHand.Net.Sockets.BluetoothClient.DiscoverDevices(System.Int32,System.Boolean,System.Boolean,System.Boolean,System.Boolean)"
        /// />(255, true, true, true)
        /// </para>
        /// </remarks>
        /// -
        /// <returns>An array of BluetoothDeviceInfo objects describing the devices discovered.</returns>
        public BluetoothDeviceInfo[] DiscoverDevices()
        {
            return DiscoverDevices(255, true, true, true);
        }

        /// <summary>
        /// Discovers accessible Bluetooth devices, both remembered and in-range,
        /// and returns their names and addresses.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>This is equivalent to calling
        /// <see cref="M:InTheHand.Net.Sockets.BluetoothClient.DiscoverDevices(System.Int32,System.Boolean,System.Boolean,System.Boolean,System.Boolean)"
        /// />(maxDevices, true, true, true)
        /// </para>
        /// </remarks>
        /// -
        /// <param name="maxDevices">The number of in-range devices to find before the inquiry may be stopped early.
        /// The result can contain more than this number of devices.
        /// </param>
        /// -
        /// <returns>An array of BluetoothDeviceInfo objects describing the devices discovered.</returns>
        public BluetoothDeviceInfo[] DiscoverDevices(int maxDevices)
        {
            return DiscoverDevices(maxDevices, true, true, true);
        }

        /// <summary>
        /// Discovers accessible Bluetooth devices, optionally remembered and in-range,
        /// and returns their names and addresses.
        /// </summary>
        /// -
        /// <param name="maxDevices">The number of in-range devices to find before the inquiry may be stopped early.
        /// The result can contain more than this number of devices.
        /// </param>
        /// <param name="authenticated">True to return previously authenticated/paired devices.</param>
        /// <param name="remembered">True to return remembered devices.</param>
        /// <param name="unknown">True to return previously unknown devices.</param>
        /// -
        /// <returns>An array of BluetoothDeviceInfo objects describing the devices discovered.</returns>
        public BluetoothDeviceInfo[] DiscoverDevices(int maxDevices, bool authenticated, bool remembered, bool unknown)
        {
            return BluetoothDeviceInfo.Wrap(m_impl.DiscoverDevices(maxDevices, authenticated, remembered, unknown, false));
        }

        /// <summary>
        /// Discovers accessible Bluetooth devices, optionally remembered and in-range or just in-range,
        /// and returns their names and addresses.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>The <paramref name="discoverableOnly"/> parameter is not supported 
        /// on the Microsoft stack on WinXP as the stack there returns the remembered and Device-Inquiry-results already 
        /// merged, it is however supported on Windows 7.
        /// It is supported on WM/CE and on Widcomm (both platforms).
        /// Note when that flag is set the other related flag values are ignored.
        /// </para>
        /// <para>To remove devices from the list of remembered/authenticated
        /// devices use <see cref="M:InTheHand.Net.Bluetooth.BluetoothSecurity.RemoveDevice(InTheHand.Net.BluetoothAddress)">BluetoothSecurity.RemoveDevice</see>
        /// </para>
        /// </remarks>
        /// -
        /// <param name="maxDevices">The number of in-range devices to find before the inquiry may be stopped early.
        /// The result can contain more than this number of devices.
        /// </param>
        /// <param name="authenticated">True to return previously authenticated/paired devices.</param>
        /// <param name="remembered">True to return remembered devices.</param>
        /// <param name="unknown">True to return previously unknown devices.</param>
        /// <param name="discoverableOnly">True to return only the devices that 
        /// are in range, and in discoverable mode.  See the remarks section.</param>
        /// -
        /// <returns>An array of BluetoothDeviceInfo objects describing the devices discovered.</returns>
        [DebuggerStepThrough]
        public BluetoothDeviceInfo[] DiscoverDevices(int maxDevices,
            bool authenticated, bool remembered, bool unknown, bool discoverableOnly)
        {
            return BluetoothDeviceInfo.Wrap(m_impl.DiscoverDevices(maxDevices, authenticated, remembered, unknown, discoverableOnly));
        }

        /// <summary>
        /// Discovers Bluetooth devices that are in range and are in &#x2018;discoverable mode&#x2019;
        /// </summary>
        /// -
        /// <remarks>
        /// <para>This is equivalent to calling
        /// <see cref="M:InTheHand.Net.Sockets.BluetoothClient.DiscoverDevices(System.Int32,System.Boolean,System.Boolean,System.Boolean,System.Boolean)"
        /// />(255, false, false, false, true)
        /// </para>
        /// </remarks>
        /// -
        /// <returns>An array of BluetoothDeviceInfo objects describing the devices discovered.</returns>
        public BluetoothDeviceInfo[] DiscoverDevicesInRange()
        {
            return DiscoverDevices(255, false, false, false, true);
        }

        /// <summary>
        /// An asynchronous version of <see cref="M:InTheHand.Net.Sockets.BluetoothClient.DiscoverDevices(System.Int32,System.Boolean,System.Boolean,System.Boolean,System.Boolean)"/>
        /// </summary>
        /// -
        /// <param name="maxDevices">See <see cref="M:InTheHand.Net.Sockets.BluetoothClient.DiscoverDevices(System.Int32,System.Boolean,System.Boolean,System.Boolean,System.Boolean)"/>.
        /// </param>
        /// <param name="authenticated">See <see cref="M:InTheHand.Net.Sockets.BluetoothClient.DiscoverDevices(System.Int32,System.Boolean,System.Boolean,System.Boolean,System.Boolean)"/>.
        /// </param>
        /// <param name="remembered">See <see cref="M:InTheHand.Net.Sockets.BluetoothClient.DiscoverDevices(System.Int32,System.Boolean,System.Boolean,System.Boolean,System.Boolean)"/>.
        /// </param>
        /// <param name="unknown">See <see cref="M:InTheHand.Net.Sockets.BluetoothClient.DiscoverDevices(System.Int32,System.Boolean,System.Boolean,System.Boolean,System.Boolean)"/>.
        /// </param>
        /// <param name="discoverableOnly">See <see cref="M:InTheHand.Net.Sockets.BluetoothClient.DiscoverDevices(System.Int32,System.Boolean,System.Boolean,System.Boolean,System.Boolean)"/>.
        /// </param>
        /// <param name="callback">An optional asynchronous callback, to be called 
        /// when the discovery is complete.
        /// </param>
        /// <param name="state">A user-provided object that distinguishes this 
        /// particular asynchronous discovery request from other requests.
        /// </param>
        /// -
        /// <returns>An <see cref="T:System.IAsyncResult"/> that represents the 
        /// asynchronous discovery, which could still be pending.
        /// </returns>
        [DebuggerStepThrough]
        public IAsyncResult BeginDiscoverDevices(int maxDevices,
            bool authenticated, bool remembered, bool unknown, bool discoverableOnly,
            AsyncCallback callback, object state)
        {
#if !V1
            return m_impl.BeginDiscoverDevices(
                maxDevices, authenticated, remembered, unknown, discoverableOnly,
                callback, state);
#else
            throw new NotSupportedException();
#endif
        }

        /// <summary>
        /// Ends an asynchronous Service Record lookup query.
        /// </summary>
        /// -
        /// <param name="asyncResult">An <see cref="T:System.IAsyncResult"/> returned
        /// by <see cref="M:InTheHand.Net.Sockets.BluetoothClient.BeginDiscoverDevices(System.Int32,System.Boolean,System.Boolean,System.Boolean,System.Boolean,System.AsyncCallback,System.Object)"/>.
        /// </param>
        /// -
        /// <returns>See <see cref="M:InTheHand.Net.Sockets.BluetoothClient.DiscoverDevices(System.Int32,System.Boolean,System.Boolean,System.Boolean,System.Boolean)"/>.
        /// </returns>
        public BluetoothDeviceInfo[] EndDiscoverDevices(IAsyncResult asyncResult)
        {
#if !V1
            return BluetoothDeviceInfo.Wrap(m_impl.EndDiscoverDevices(asyncResult));
#else
            throw new NotSupportedException();
#endif
        }

        //--------
        /// <exclude/>
        public delegate void LiveDiscoveryCallback(IBluetoothDeviceInfo p1, object p2);

        internal IAsyncResult BeginDiscoverDevices(int maxDevices,
            bool authenticated, bool remembered, bool unknown, bool discoverableOnly,
            AsyncCallback callback, object state,
            LiveDiscoveryCallback handler, object liveDiscoState)
        {
            return m_impl.BeginDiscoverDevices(
                maxDevices, authenticated, remembered, unknown, discoverableOnly,
                callback, state,
                handler, liveDiscoState);
        }
        #endregion


        #region Available
        /// <summary>
        /// Gets the amount of data that has been received from the network and is available to be read.
        /// </summary>
        /// <value>The number of bytes of data received from the network and available to be read.</value>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="Socket"/> has been closed.</exception>
        public int Available
        {
            [DebuggerStepThrough]
            get { return m_impl.Available; }
        }
        #endregion

        #region Client

        /// <summary>
        /// Gets or sets the underlying <see cref="Socket"/>.
        /// </summary>
        /// -
        /// <value>The underlying network <see cref="Socket"/>.</value>
        /// -
        /// <remarks>
        /// <note>The property is only supported on Microsoft Bluetooth stack platforms.
        /// </note>
        /// </remarks>
        public Socket Client
        {
            [DebuggerStepThrough]
            get { return m_impl.Client; }
            [DebuggerStepThrough]
            set { m_impl.Client = value; }
        }

        #endregion

        #region Connect
        /// <summary>
        /// Connects a client to a specified endpoint.
        /// </summary>
        /// -
        /// <param name="remoteEP">A <see cref="BluetoothEndPoint"/> that represents the server on the remote device.</param>
        /// -
        /// <remarks>
        /// <!--This para is in both the class remarks and in Connect(BtEndPoint)-->
        /// <para>Normally an endpoint with an Address and a Service Class Id 
        /// is specified, then the system will automatically lookup the SDP 
        /// record on the remote device for that service class and connect to 
        /// the port number (RFCOMM Channel Number) specified there.
        /// If instead a port value is provided in the endpoint then the SDP 
        /// lookup will be skipped and  the system will connect to the specified 
        /// port directly.
        /// </para>
        /// <para>Note: Do not attempt to connect with service
        /// <see cref="F:InTheHand.Net.Bluetooth.BluetoothService.RFCommProtocol">BluetoothService.RFCommProtocol</see>.
        /// See the <see cref="T:InTheHand.Net.Sockets.BluetoothClient">class</see> remarks for more information.
        /// </para> 
        /// </remarks>
        public void Connect(BluetoothEndPoint remoteEP)
        {
            if (remoteEP == null) {
                throw new ArgumentNullException("remoteEP");
            }
            m_impl.Connect(remoteEP);
        }
        /// <summary>
        /// Connects the client to a remote Bluetooth host using the specified Bluetooth address and service identifier. 
        /// </summary>
        /// -
        /// <remarks>
        /// <!--This para is in both the class remarks and in Connect(BtEndPoint)-->
        /// <para>The system will automatically lookup the SDP 
        /// record on the remote device for that service class and connect to 
        /// the port number (RFCOMM Channel Number) specified there.
        /// </para>
        /// <para>Note: Do not attempt to connect with service
        /// <see cref="F:InTheHand.Net.Bluetooth.BluetoothService.RFCommProtocol">BluetoothService.RFCommProtocol</see>.
        /// See the <see cref="T:InTheHand.Net.Sockets.BluetoothClient">class</see> remarks for more information.
        /// </para> 
        /// </remarks>
        /// -
        /// <param name="address">The <see cref="BluetoothAddress"/> of the remote host.
        /// </param>
        /// <param name="service">The Service Class Id of the service on the remote host.
        /// The standard Bluetooth Service Classes are provided on class 
        /// <see cref="T:InTheHand.Net.Bluetooth.BluetoothService"/>.
        /// </param>
        public void Connect(BluetoothAddress address, Guid service)
        {
            if (address == null) {
                throw new ArgumentNullException("address");
            }
            if (service == Guid.Empty) {
                throw new ArgumentNullException("service");
            }
            BluetoothEndPoint point = new BluetoothEndPoint(address, service);
            this.Connect(point);
        }

        #region Begin Connect
        /// <summary>
        /// Begins an asynchronous request for a remote host connection.
        /// The remote host is specified by a <see cref="BluetoothAddress"/> and a service identifier (Guid). 
        /// </summary>
        /// -
        /// <remarks>
        /// <para>See the <see cref="M:InTheHand.Net.Sockets.BluetoothClient.Connect(InTheHand.Net.BluetoothAddress,System.Guid)"/>
        /// method for information on the usage of the values in the endpoint.
        /// </para>
        /// </remarks>
        /// -
        /// <param name="address">The <see cref="BluetoothAddress"/> of the remote host.
        /// </param>
        /// <param name="service">The Service Class Id of the service on the remote host.
        /// The standard Bluetooth Service Classes are provided on class 
        /// <see cref="T:InTheHand.Net.Bluetooth.BluetoothService"/>
        /// </param>
        /// <param name="requestCallback">An <see cref="T:System.AsyncCallback"/> delegate that 
        /// references the method to invoke when the operation is complete.
        /// </param>
        /// <param name="state">A user-defined object that contains information 
        /// about the connect operation. This object is passed to the <paramref name="requestCallback"/> 
        /// delegate when the operation is complete.
        /// </param>
        /// -
        /// <returns>An <see cref="T:System.IAsyncResult"/> that represents the 
        /// asynchronous connect, which could still be pending.
        /// </returns>
        public IAsyncResult BeginConnect(BluetoothAddress address, Guid service, AsyncCallback requestCallback, object state)
        {
            if (address == null) {
                throw new ArgumentNullException("address");
            }
            if (service == Guid.Empty) {
                throw new ArgumentNullException("service");
            }
            return BeginConnect(new BluetoothEndPoint(address, service), requestCallback, state);
        }

        /// <summary>
        /// Begins an asynchronous request for a remote host connection.
        /// The remote server is specified by a <see cref="BluetoothEndPoint"/>. 
        /// </summary>
        /// -
        /// <param name="remoteEP">A <see cref="BluetoothEndPoint"/> that 
        /// represents the server on the remote device.
        /// See the <see cref="M:InTheHand.Net.Sockets.BluetoothClient.Connect(InTheHand.Net.BluetoothEndPoint)"/>
        /// method for information on the usage of the values in the endpoint.
        /// </param>
        /// <param name="requestCallback">An <see cref="T:System.AsyncCallback"/> delegate that 
        /// references the method to invoke when the operation is complete.
        /// </param>
        /// <param name="state">A user-defined object that contains information 
        /// about the connect operation. This object is passed to the <paramref name="requestCallback"/> 
        /// delegate when the operation is complete.
        /// </param>
        /// -
        /// <remarks>
        /// <para>See the <see cref="M:InTheHand.Net.Sockets.BluetoothClient.Connect(InTheHand.Net.BluetoothEndPoint)"/>
        /// method for information on the usage of the values in the endpoint.
        /// </para>
        /// </remarks>
        /// -
        /// <returns>An <see cref="T:System.IAsyncResult"/> that represents the 
        /// asynchronous connect, which could still be pending.
        /// </returns>
        public IAsyncResult BeginConnect(BluetoothEndPoint remoteEP, AsyncCallback requestCallback, object state)
        {
            if (remoteEP == null) {
                throw new ArgumentNullException("remoteEP");
            }
            return m_impl.BeginConnect(remoteEP, requestCallback, state);
        }
        #endregion

        #region End Connect
        /// <summary>
        /// Asynchronously accepts an incoming connection attempt.
        /// </summary>
        /// <param name="asyncResult">An <see cref="IAsyncResult"/> object returned by a call to 
        /// <see cref="M:InTheHand.Net.Sockets.BluetoothClient.BeginConnect(InTheHand.Net.BluetoothEndPoint,System.AsyncCallback,System.Object)"/>
        /// / <see cref="M:InTheHand.Net.Sockets.BluetoothClient.BeginConnect(InTheHand.Net.BluetoothAddress,System.Guid,System.AsyncCallback,System.Object)"/>.
        /// </param>
        [DebuggerStepThrough]
        public void EndConnect(IAsyncResult asyncResult)
        {
            m_impl.EndConnect(asyncResult);
        }

#if FX4
        public System.Threading.Tasks.Task ConnectAsync(BluetoothEndPoint remoteEP, object state)
        {
            return System.Threading.Tasks.Task.Factory.FromAsync(
                BeginConnect, EndConnect,
                remoteEP, state);
        }
#endif
        #endregion

        #endregion

        #region Connected
        /// <summary>
        /// Gets a value indicating whether the underlying <see cref="Socket"/> for a <see cref="BluetoothClient"/> is connected to a remote host.
        /// </summary>
        /// <value>true if the <see cref="Client"/> socket was connected to a remote resource as of the most recent operation; otherwise, false.</value>
        public bool Connected
        {
            [DebuggerStepThrough]
            get { return m_impl.Connected; }
        }
        #endregion

        #region Close
        /// <summary>
        /// Closes the <see cref="BluetoothClient"/> and the underlying connection.
        /// </summary>
        /// -
        /// <remarks>The two XxxxxClient classes produced by Microsoft (TcpClient, 
        /// and IrDAClient in the NETCF) have had various documented behaviours and various
        /// actual behaviours for close/dispose/finalize on the various platforms. :-(
        /// The current TcpClient implementation on is that 
        /// Close/Dispose closes the connection by closing the underlying socket and/or
        /// NetworkStream, and finalization doesn't close either.  This is the behaviour
        /// we use for the here (for <see cref="T:InTheHand.Net.Sockets.BluetoothClient"/>,
        /// <see cref="T:InTheHand.Net.Sockets.IrDAClient"/>).  (The documentation in MSDN for 
        /// <see cref="T:System.Net.Sockets.TcpClient"/> is still wrong by-the-way,
        /// see <see href="https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=158480">
        /// Microsoft feedback #158480</see>).
        /// </remarks>
        [DebuggerStepThrough]
        public void Close()
        {
            Dispose();
        }
        #endregion

        #region Get Stream

        /// <summary>
        /// Gets the underlying stream of data.
        /// </summary>
        /// <returns>The underlying <see cref="NetworkStream"/>.</returns>
        /// <remarks><see cref="GetStream"/> returns a <see cref="NetworkStream"/> that you can use to send and receive data.
        /// The <see cref="NetworkStream"/> class inherits from the <see cref="Stream"/> class, which provides a rich collection of methods and properties used to facilitate network communications.
        /// <para>You must call the <see cref="Connect(InTheHand.Net.BluetoothEndPoint)"/> / <see cref="M:InTheHand.Net.Sockets.BluetoothClient.Connect(InTheHand.Net.BluetoothAddress,System.Guid)"/>
        /// method first, or the <see cref="GetStream"/> method will throw an <see cref="T:System.InvalidOperationException"/>.
        /// After you have obtained the <see cref="NetworkStream"/>, call the <see cref="NetworkStream.Write"/> method to send data to the remote host.
        /// Call the <see cref="NetworkStream.Read"/> method to receive data arriving from the remote host.
        /// Both of these methods block until the specified operation is performed.
        /// You can avoid blocking on a read operation by checking the <see cref="NetworkStream.DataAvailable"/> property.
        /// A true value means that data has arrived from the remote host and is available for reading.
        /// In this case, <see cref="NetworkStream.Read"/> is guaranteed to complete immediately.
        /// If the remote host has shutdown its connection, <see cref="NetworkStream.Read"/> will immediately return with zero bytes.</para></remarks>
        /// <exception cref="T:System.InvalidOperationException">The <see cref="BluetoothClient"/> is not connected to a remote host.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="BluetoothClient"/> has been closed.</exception>
        [DebuggerStepThrough]
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public NetworkStream GetStream()
        {
            return m_impl.GetStream();
        }

#if TEST_EARLY
        [Obsolete("Now that we've wrapped NetworkStream there's no need for this property.")]
        public Stream GetStream2()
        {
            return m_impl.GetStream2();
        }
#endif
        #endregion

        #region LingerState
        /// <summary>
        /// Gets or sets a value that specifies whether the client will delay closing 
        /// in an attempt to send all pending data.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>See <see cref="P:System.Net.Sockets.Socket.LingerState">Socket.LingerState</see>.
        /// </para>
        /// <para>In Widcomm, linger <c>false</c> (disabled) is not supported.
        /// </para>
        /// </remarks>
        /// -
        /// <value>A <see cref="T:System.Net.Sockets.LingerOption"/> that specifies 
        /// how to linger while closing a socket.
        /// </value>
        public System.Net.Sockets.LingerOption LingerState
        {
            [DebuggerStepThrough]
            get { return m_impl.LingerState; }
            [DebuggerStepThrough]
            set { m_impl.LingerState = value; }
        }
        #endregion

        #region Authenticate
        /// <summary>
        /// Sets whether an authenticated connection is required.
        /// </summary>
        /// <remarks>
        /// <para>Supported mostly on the Microsoft stack (desktop and WM/CE).
        /// </para>
        /// For disconnected sockets, specifies that authentication is required in order for a connect or accept operation to complete successfully.
        /// Setting this option actively initiates authentication during connection establishment, if the two Bluetooth devices were not previously authenticated.
        /// The user interface for passkey exchange, if necessary, is provided by the operating system outside the application context.
        /// For outgoing connections that require authentication, the connect operation fails (on Win32, with WSAEACCES) if authentication is not successful.
        /// In response, the application may prompt the user to authenticate the two Bluetooth devices before connection.
        /// For incoming connections, the connection is rejected if authentication cannot be established and fails (on Win32, returning a WSAEHOSTDOWN error).
        /// <para>MSDN. Desktop: <see href="http://msdn.microsoft.com/en-us/library/aa362911(VS.85).aspx" />
        /// , WM: <see href="http://msdn.microsoft.com/en-us/library/aa915899.aspx" />
        /// </para>
        /// </remarks>
        public bool Authenticate
        {
            get { return m_impl.Authenticate; }
            set { m_impl.Authenticate = value; }
        }
        #endregion

        #region Encrypt
        /// <summary>
        /// Sets whether an encrypted connection is required.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>Supported mostly on the Microsoft stack (desktop and WM/CE).
        /// </para>
        /// On unconnected sockets, enforces encryption to establish a connection.
        /// Encryption is only available for authenticated connections.
        /// For incoming connections, a connection for which encryption cannot be established is automatically rejected (and on Win32, returns WSAEHOSTDOWN as the error).
        /// For outgoing connections, the Connect function fails (on Win32, with WSAEACCES) if encryption cannot be established.
        /// In response, the application may prompt the user to authenticate the two Bluetooth devices before connection.
        /// <note>For Windows Mobile/CE:
        /// On a connected socket, this command will toggle encryption for all sessions sharing the same Baseband connection. You should use it ONLY if you know what you are doing (for example, yours is the only application); otherwise, the link presumed more secure by another application may become unencrypted.
        /// </note>
        /// <para>MSDN. Desktop: <see href="http://msdn.microsoft.com/en-us/library/aa362911(VS.85).aspx" />
        /// , WM: <see href="http://msdn.microsoft.com/en-us/library/aa915899.aspx" />
        /// </para>
        /// </remarks>
        public bool Encrypt
        {
            [DebuggerStepThrough]
            get { return m_impl.Encrypt; }
            [DebuggerStepThrough]
            set { m_impl.Encrypt = value; }
        }
        #endregion


        #region Link Key
        /// <summary>
        /// Returns link key associated with peer Bluetooth device.
        /// </summary>
        public Guid LinkKey
        {
            [DebuggerStepThrough]
            get { return m_impl.LinkKey; }
        }
        #endregion

        #region Link Policy
        /// <summary>
        /// Returns the Link Policy of the current connection.
        /// </summary>
        public LinkPolicy LinkPolicy
        {
            get { return m_impl.LinkPolicy; }
        }
        #endregion


        #region Set PIN
        /// <summary>
        /// Sets the PIN associated with the remote device.
        /// </summary>
        /// <param name="pin">PIN which must be composed of 1 to 16 ASCII characters.</param>
        /// <remarks>
        /// <para>Is not supported on all platforms.
        /// For instance see the Widcomm documentation 
        /// </para>
        /// <para>Assigning null (Nothing in VB) or an empty String will revoke the PIN.
        /// </para>
        /// <para>In version 2.3 could only be called when connected.
        /// </para>
        /// </remarks>
        [DebuggerStepThrough]
        public void SetPin(string pin)
        {
            m_impl.SetPin(pin);
        }

        /// <summary>
        /// Set or change the PIN to be used with a specific remote device.
        /// </summary>
        /// <param name="device">Address of Bluetooth device.</param>
        /// <param name="pin">PIN string consisting of 1 to 16 ASCII characters.</param>
        /// <remarks>
        /// <para>Is not supported on all platforms.
        /// For instance see the Widcomm documentation 
        /// </para>
        /// <para>Assigning null (Nothing in VB) or an empty String will revoke the PIN.
        /// </para>
        /// </remarks>
        [DebuggerStepThrough]
        public void SetPin(BluetoothAddress device, string pin)
        {
            m_impl.SetPin(device, pin);
        }
        #endregion


        #region Remote Machine Name
        /// <summary>
        /// Get the remote endpoint.
        /// </summary>
        /// -
        /// <value>
        /// The <see cref="T:InTheHand.Net.BluetoothEndPoint"/> with which the 
        /// <see cref="T:InTheHand.Net.Sockets.BluetoothClient"/> is communicating.
        /// </value>
        /// -
        /// <remarks>
        /// <para>Note it can't be guaranteed that the <see cref="P:InTheHand.Net.BluetoothEndPoint.Service"/>
        /// and <see cref="P:InTheHand.Net.BluetoothEndPoint.Port"/> parts
        /// of the returned endpoint are valid; and this will affect the
        /// <see cref="M:InTheHand.Net.BluetoothEndPoint.ToString"/> output.
        /// In particular, on MSFT, the <see cref="P:InTheHand.Net.Sockets.BluetoothClient.RemoteEndPoint"/>
        /// for a client connection seems to have no <see cref="P:InTheHand.Net.BluetoothEndPoint.Port"/>
        /// and a garbage <see cref="P:InTheHand.Net.BluetoothEndPoint.Service"/>,
        /// so we would display garbage there in <see cref="M:InTheHand.Net.BluetoothEndPoint.ToString"/>.
        /// An in-bound/server connection however does have a valid Port.
        /// (There the endpoints are returned from the native socket).
        /// On the other hand on Widcomm, Bluetopia and on BlueSoleil the
        /// opposite is the case: for a client the Port is known but it isn't
        /// for a server, and the <see cref="P:InTheHand.Net.BluetoothEndPoint.Service"/>
        /// is blank in both cases.
        /// </para>
        /// </remarks>
        public BluetoothEndPoint RemoteEndPoint
        {
            [DebuggerStepThrough]
            get { return m_impl.RemoteEndPoint; }
        }

        /// <summary>
        /// Gets the name of the remote device.
        /// </summary>
        public string RemoteMachineName
        {
            [DebuggerStepThrough]
            get { return m_impl.RemoteMachineName; }
        }

        /// <summary>
        /// Gets the name of the specified remote device.
        /// </summary>
        /// <param name="a">Address of remote device.</param>
        /// <returns>Friendly name of specified device.</returns>
        [DebuggerStepThrough]
        public string GetRemoteMachineName(BluetoothAddress a)
        {
            return m_impl.GetRemoteMachineName(a);
        }

        /// <summary>
        /// Gets the name of a device by a specified socket.
        /// </summary>
        /// <param name="s"> A <see cref="Socket"/>.</param>
        /// <returns>Returns a string value of the computer or device name.</returns>
        public static string GetRemoteMachineName(Socket s)
        {
#if WinCE
            return InTheHand.Net.Bluetooth.Msft.SocketBluetoothClient.GetRemoteMachineName(s);
#else
            var bdi = new BluetoothDeviceInfo(((BluetoothEndPoint)s.RemoteEndPoint).Address);
            return bdi.DeviceName;
#endif
        }
        #endregion

        #region IDisposable Members

        /// <summary>
        /// Closes the <see cref="BluetoothClient"/> and the underlying connection.
        /// </summary>
        /// -
        /// <seealso cref="M:InTheHand.Net.Sockets.BluetoothClient.Close"/>
        [DebuggerStepThrough]
        public void Dispose()
        {
            m_impl.Dispose();
        }
        #endregion

        [Obsolete("Use the four Boolean method.")] // Is used by UnitTests
        internal static List_IBluetoothDeviceInfo DiscoverDevicesMerge(
            bool authenticated, bool remembered, bool unknown,
            List_IBluetoothDeviceInfo knownDevices,
            List_IBluetoothDeviceInfo discoverableDevices, DateTime discoTime)
        {
            return DiscoverDevicesMerge(authenticated, remembered, unknown,
                knownDevices, discoverableDevices,
                false, discoTime);
        }

        internal static List_IBluetoothDeviceInfo DiscoverDevicesMerge(
            bool authenticated, bool remembered, bool unknown,
            List_IBluetoothDeviceInfo knownDevices,
            List_IBluetoothDeviceInfo discoverableDevices,
            bool discoverableOnly, DateTime discoTime)
        {
            // Check args
            if (unknown || discoverableOnly) {
                if (discoverableDevices == null)
                    throw new ArgumentNullException("discoverableDevices");
            } else {
                bool nunit = TestUtilities.IsUnderTestHarness(); // Don't warn then.
                Debug.Assert(nunit || discoverableDevices == null, "No need to run SLOW Inquiry when not wanting 'unknown'.");
            }
            if (knownDevices == null)
                throw new ArgumentNullException("knownDevices");
            AssertNoDuplicates(knownDevices, "knownDevices");
            AssertNoDuplicates(discoverableDevices, "discoverableDevices");
            //
            bool addFromKnown_General;
            if (discoverableOnly) {
                addFromKnown_General = false;
            } else {
                addFromKnown_General = authenticated || remembered;
                if (!addFromKnown_General && !unknown)
                    return new List_IBluetoothDeviceInfo();
            }
            List_IBluetoothDeviceInfo merged;
            if (unknown || discoverableOnly)
                merged = new List_IBluetoothDeviceInfo(discoverableDevices);
            else
                merged = new List_IBluetoothDeviceInfo();
            //
            foreach (IBluetoothDeviceInfo cur in knownDevices) {
                bool debug_contains = false;
                //TODO #if DEBUG
                foreach (IBluetoothDeviceInfo curMerged in merged) {
                    if (BluetoothDeviceInfo.EqualsIBDI(curMerged, cur)) {
                        debug_contains = true;
                        break;
                    }
                }
                //#endif
                //--
                int idx = BluetoothDeviceInfo.ListIndexOf(merged, cur);
                if (idx != -1) {
                    // The device is both in the inquiry result and the remembered list.
                    // Does the called want "already known" devices?  If so, update 
                    // to the correct flags, otherwise remove it.
                    Debug.Assert(debug_contains);
                    if (addFromKnown_General || discoverableOnly) {
                        ((IBluetoothDeviceInfo)merged[idx]).Merge(cur);
                        // (The casts are for the NETCFv1 build).
                    } else {
                        merged.RemoveAt(idx);
                    }
                } else {
                    // The device is not in inquiry result, do we add it from the known list?
                    Debug.Assert(!debug_contains);
                    bool addFromKnown_Specific
                        = (remembered && cur.Remembered)
                        || (authenticated && cur.Authenticated);
                    if (addFromKnown_General && addFromKnown_Specific) {
                        merged.Add(cur);
                    }
                }
            }//for
            AssertNoDuplicates(merged, "merged");
            return merged;
        }

        [Conditional("DEBUG")]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.String.Format(System.IFormatProvider,System.String,System.Object[])")]
        private static void AssertNoDuplicates(
            List_IBluetoothDeviceInfo deviceList, string location)
        {
            // (The casts are for the NETCFv1 build).
            if (deviceList == null)
                return;
            for (int i = 0; i < deviceList.Count; ++i) {
                IBluetoothDeviceInfo cur = (IBluetoothDeviceInfo)deviceList[i];
                BluetoothAddress curAddr = cur.DeviceAddress;
                for (int j = i + 1; j < deviceList.Count; ++j) {
                    if (((IBluetoothDeviceInfo)deviceList[j]).DeviceAddress == curAddr) {
                        string msg = $"'{location}' duplicate #{i}==#{j}: '{cur}' '{deviceList[j]}'";
                        Debug.Fail(msg);
                    }
                }
            }
        }

    }//class--BluetoothClient

}
