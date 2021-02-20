using System;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Bluetooth.Factory;
using System.Diagnostics;

namespace InTheHand.Net.Sockets
{
    /// <summary>
    /// Listens for connections from L2CAP Bluetooth network clients.
    /// </summary>
    /// -
    /// <remarks>
    /// <para>The <see cref="L2CapListener"/> class provides simple methods 
    /// that listen for and accept incoming connection requests.  New connections 
    /// are returned as <see cref="T:InTheHand.Net.Sockets.L2CapClient"/> instances.
    /// </para>
    /// <para>In the normal case a the listener is initialised with a 
    /// <see cref="T:System.Guid"/> holding the Service Class Id on which it is 
    /// to accept connections, the listener will automatically create a SDP 
    /// Service Record containg that Service Class Id and the port number
    /// (L2CAP Protocol Service Multiplexer) that it has started listening on.
    /// The standard usage is thus as follows.
    /// </para>
    /// <code lang="VB.NET">
    /// Class MyConsts
    ///   Shared ReadOnly MyServiceUuid As Guid _
    ///     = New Guid("{00112233-4455-6677-8899-aabbccddeeff}")
    /// End Class
    /// 
    ///   ...
    ///   Dim lsnr As New L2CapListener(MyConsts.MyServiceUuid)
    ///   lsnr.Start()
    ///   ' Now accept new connections, perhaps using the thread pool to handle each
    ///   Dim conn As New L2CapClient = lsnr.AcceptClient()
    ///   Dim peerStream As Stream = conn.GetStream()
    ///   ...
    /// </code>
    /// <para>One can also pass the L2CapListener a Service Name, or
    /// a custom Service Record (Service Discovery Protocol record).
    /// To create a custom Service Record use 
    /// <see cref="T:InTheHand.Net.Bluetooth.ServiceRecordBuilder"/>.
    /// </para>
    /// <para>There are overloads of the constructor which take a 
    /// <see cref="T:InTheHand.Net.BluetoothEndPoint"/> parameter instead of a 
    /// <see cref="T:System.Guid"/> as the Service Class Id, the Class Id
    /// value should be specified in that case in the endpoint.
    /// If the port value is specified in the endpoint, then the listener will 
    /// attempt to bind to that L2CAP PSM locally.  The address in the endpoint is 
    /// largely ignored as no current stack supports more than one local radio.
    /// </para>
    /// <para>The L2CAP protocol accepts only certain PSM values.  The value is
    /// a 16-bit integer, and the low byte must be odd and the high byte must
    /// be even. So e.g. 0x0001 is valid, but 0x0002 and 0x0101 are invalid.
    /// The range below 0x1001 is reserved for standards allocations.
    /// See the L2CAP Specification for more information, L2CAP section 4.2
    /// (and SDP section 5.1.5) in the version 2.1 specification.
    /// </para>
    /// </remarks>
    public class L2CapListener
    {
        readonly IBluetoothListener m_impl; // HACK ?should be IL2CapListener?

        #region Real Constructor
        private L2CapListener(BluetoothFactory factory)
        {
            if (factory != null) {
                Trace.Assert(false, "Specific factory!");
            }
            // HACK
            if (PlatformVerification.IsMonoRuntime) {
#if NETCF
                throw new RankException("IsMonoRuntime on NETCF!!");
#elif BlueZ
                m_impl = new InTheHand.Net.Bluetooth.BlueZ.BluezL2CapListener(null);
#endif
            } else {
                m_impl = InTheHand.Net.Bluetooth.Widcomm.WidcommL2CapListener.Create();
            }
            //m_impl = factory.DoGetBluetoothListener();
            //m_impl.ToString(); // A check for null pointer!
        }

        private static BluetoothFactory DefaultBluetoothFactory
        {
            //get { return BluetoothFactory.Factory; } HACK
            get { return null; }
        }
        #endregion

        #region Public Contructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:InTheHand.Net.Sockets.L2CapListener"/> class
        /// that listens on the specified service identifier.
        /// </summary>
        /// -
        /// <param name="service">The Bluetooth service to listen on.
        /// Either one of the values on <see cref="T:InTheHand.Net.Bluetooth.BluetoothService"/>,
        /// or your custom UUID stored in a <see cref="T:System.Guid"/>.
        /// See the <see cref="L2CapListener"/> documentation for more information 
        /// on the usage of this argument.
        /// </param>
        public L2CapListener(Guid service)
            : this(DefaultBluetoothFactory, service)
        {
        }

        internal L2CapListener(BluetoothFactory factory, Guid service)
            : this(factory)
        {
            m_impl.Construct(service);
        }

        //
        /// <summary>
        /// Initializes a new instance of the <see cref="L2CapListener"/> class
        /// with the specified local endpoint.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>The simpler constructor <see cref="M:InTheHand.Net.Sockets.L2CapListener.#ctor(System.Guid)"/>
        /// taking just a <see cref="T:System.Guid">System.Guid</see> is used 
        /// in most cases instead of this one.
        /// </para>
        /// </remarks>
        /// -
        /// <param name="localEP">A <see cref="BluetoothEndPoint"/> that represents 
        /// the local endpoint to which to bind the listener.
        /// Either one of the values on <see cref="T:InTheHand.Net.Bluetooth.BluetoothService"/>,
        /// or your custom UUID stored in a <see cref="T:System.Guid"/>.
        /// See the <see cref="L2CapListener"/> documentation for more information 
        /// on the usage of this argument.
        /// </param>
        public L2CapListener(BluetoothEndPoint localEP)
            : this(DefaultBluetoothFactory, localEP)
        {
        }

        internal L2CapListener(BluetoothFactory factory, BluetoothEndPoint localEP)
            : this(factory)
        {
            if (localEP == null) {
                throw new ArgumentNullException("localEP");
            }
            m_impl.Construct(localEP);
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
        /// Starts listening for incoming connection requests with a maximum
        /// number of pending connection.
        /// </summary>
        /// -
        /// <param name="backlog">The maximum length of the pending connections
        /// queue.
        /// </param>
        public void Start(int backlog)
        {
            m_impl.Start(backlog);
        }
        #endregion

        #region Stop
        /// <summary>
        /// Closes the listener.
        /// </summary>
        public void Stop()
        {
            m_impl.Stop();
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
        /// property of the endpoint will contain the port number (L2CAP PSM) 
        /// that the listener is listening on.
        /// On some platforms, the <see cref="P:InTheHand.Net.BluetoothEndPoint.Address"/>
        /// is similarly set, or is <see cref="F:InTheHand.Net.BluetoothAddress.None">BluetoothAddress.None</see> 
        /// if not known.
        /// The endpoint&#x2019;s <see cref="P:InTheHand.Net.BluetoothEndPoint.Service"/>
        /// is never set.
        /// </para>
        /// </remarks>
        public BluetoothEndPoint LocalEndPoint
        {
            get { return m_impl.LocalEndPoint; }
        }
        #endregion

        #region Accept
        #region Async Client
        /// <summary>
        /// Begins an asynchronous operation to accept an incoming connection attempt.
        /// </summary>
        /// -
        /// <param name="callback">An AsyncCallback delegate that references
        /// the method to invoke when the operation is complete.
        /// </param>
        /// <param name="state">A user-defined object containing information 
        /// about the accept operation. This object is passed to the callback
        /// delegate when the operation is complete.
        /// </param>
        /// -
        /// <returns>An <see cref="T:System.IAsyncResult"/> that represents the 
        /// asynchronous accept, which could still be pending.
        /// </returns>
        public IAsyncResult BeginAcceptClient(AsyncCallback callback, object state)
        {
            return m_impl.BeginAcceptBluetoothClient(callback, state);
        }

        /// <summary>
        /// Asynchronously accepts an incoming connection attempt and creates
        /// a new <see cref="T:InTheHand.Net.Sockets.L2CapClient"/> to handle remote host communication.
        /// </summary>
        /// -
        /// <param name="asyncResult">An <see cref="IAsyncResult"/> returned
        /// by a call to the <see cref="BeginAcceptClient"/> method.
        /// </param>
        /// -
        /// <returns>A <see cref="T:InTheHand.Net.Sockets.L2CapClient"/>.
        /// </returns>
        public L2CapClient EndAcceptClient(IAsyncResult asyncResult)
        {
            return new L2CapClient(m_impl.EndAcceptBluetoothClient(asyncResult));
        }

#if FX4
        public System.Threading.Tasks.Task<L2CapClient> AcceptClientAsync(object state)
        {
            return System.Threading.Tasks.Task.Factory.FromAsync<L2CapClient>(
                BeginAcceptClient, EndAcceptClient,
                state);
        }
#endif
        #endregion

        /// <summary>
        /// Accepts a pending connection request.
        /// </summary>
        /// -
        /// <remarks>AcceptClient is a blocking method that returns a
        /// <see cref="T:InTheHand.Net.Sockets.L2CapClient"/> that you can use to send and receive data.
        /// Use the <see cref="Pending"/> method to determine if connection
        /// requests are available in the incoming connection queue if you want
        /// to avoid blocking.
        /// <para>Use the <see cref="L2CapClient.GetStream"/> method to obtain
        /// the underlying <see cref="System.IO.Stream"/> of the returned
        /// <see cref="T:InTheHand.Net.Sockets.L2CapClient"/>.
        /// The <see cref="System.IO.Stream"/> will provide you with methods for
        /// sending and receiving with the remote host.
        /// When you are through with the <see cref="T:InTheHand.Net.Sockets.L2CapClient"/>, be sure
        /// to call its <see cref="L2CapClient.Close"/> method.
        /// </para>
        /// </remarks>
        /// -
        /// <returns>A <see cref="T:InTheHand.Net.Sockets.L2CapClient"/> used to send and receive data.</returns>
        /// -
        /// <exception cref="T:System.InvalidOperationException">Listener is stopped.</exception>
        public L2CapClient AcceptClient()
        {
            IBluetoothClient cli0 = m_impl.AcceptBluetoothClient();
            var cli = new L2CapClient(cli0);
            return cli;
        }
        #endregion

        #region Pending
        /// <summary>
        /// Determines if there is a connection pending.
        /// </summary>
        /// -
        /// <returns>true if there is a connection pending; otherwise, false.
        /// </returns>
        public bool Pending()
        {
            return m_impl.Pending();
        }
        #endregion

        #region Service Class
        // /// <summary>
        // /// Get or set the Service Class flags that this service adds to the host 
        // /// device&#x2019;s Class Of Device field.
        // /// </summary>
        // /// -
        // /// <remarks>
        // /// <para>The Class of Device value contains a Device part which describes 
        // /// the primary service that the device provides, and a Service part which 
        // /// is a set of flags indicating all the service types that the device supports, 
        // /// e.g. <see cref="F:InTheHand.Net.Bluetooth.ServiceClass.ObjectTransfer"/>,
        // /// <see cref="F:InTheHand.Net.Bluetooth.ServiceClass.Telephony"/>,
        // /// <see cref="F:InTheHand.Net.Bluetooth.ServiceClass.Audio"/> etc.
        // /// This property supports setting those flags; bits set in this value will be 
        // /// <strong>added</strong> to the host device&#x2019;s CoD Service Class bits when the listener
        // /// is active.  For Win32 see <see href="http://msdn.microsoft.com/en-us/library/aa362940(VS.85).aspx">MSDN &#x2014; BTH_SET_SERVICE Structure</see>
        // /// </para>
        // /// <para><note>Supported on Win32, but not supported on WindowsMobile/WinCE 
        // /// as there's no native API for it.  The WindowCE section of MSDN mentions the
        // /// Registry value <c>COD</c> at key <c>HKEY_LOCAL_MACHINE\Software\Microsoft\Bluetooth\sys</c>. 
        // /// However my (Jam) has value 0x920100 there but advertises a CoD of 0x100114, 
        // /// so its not clear how the values relate to each other.
        // /// </note>
        // /// </para>
        // /// </remarks>
        // public ServiceClass ServiceClass
        // {
        //     get { return m_impl.ServiceClass; }
        //     set { m_impl.ServiceClass = value; }
        // }

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

        #region Service Record
        /// <summary>
        /// Returns the SDP Service Record for this service.
        /// </summary>
        /// -
        /// <remarks>
        /// <note>Returns <see langword="null"/> if the listener is not 
        /// <see cref="M:InTheHand.Net.Sockets.L2CapListener.Start"/>ed
        /// (and an record wasn&#x2019;t supplied at initialization).
        /// </note>
        /// </remarks>
        public ServiceRecord ServiceRecord
        {
            get { return m_impl.ServiceRecord; }
        }
        #endregion

    }
}
