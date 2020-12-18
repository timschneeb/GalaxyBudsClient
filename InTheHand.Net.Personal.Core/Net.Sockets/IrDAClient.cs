// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Sockets.IrDAClient
// 
// Copyright (c) 2003-2006 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

#if !NO_IRDA
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using InTheHand.Net.IrDA;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;


namespace InTheHand.Net.Sockets
{

    /// <summary>
    /// Makes connections to services on peer IrDA devices.
    /// </summary>
    /// -
    /// <remarks>
    /// <para>Makes connections to services on peer IrDA devices.  It allows 
    /// discovery of all devices in range, then a connection can be made to 
    /// the required service on the chosen peer.  Or, given just the 
    /// service name a connection will be made to an arbitrary peer.  This is 
    /// most useful when it is expected that there will be only one device in 
    /// range—as is often the case.</para>
    /// <para>It can be used with both the full and Compact frameworks, and can 
    /// be used as a replacement for the latter's built-in version simply by 
    /// changing the referenced namespace and assembly.
    /// It also has features extra 
    /// to those in the CF's version.  For instance, following the 
    /// pattern of <see cref="T:System.Net.Sockets.TcpClient"/> in framework 
    /// version 2, it provides access to the underlying 
    /// <see cref="T:System.Net.Sockets.Socket"/> via a <c>Client</c> 
    /// property.  This is particularly useful as it allows setting socket 
    /// options, for instance IrCOMM Cooked mode with option <see 
    /// cref="F:InTheHand.Net.Sockets.IrDASocketOptionName.NineWireMode"/>.
    /// </para>
    /// <para>There a number of well-known services, a few are listed here.
    /// <list type="bullet">
    /// <listheader><term>Service description</term>
    ///     <description>Service Name, Protocol type</description></listheader>
    /// <item><term>OBEX file transfer</term>
    ///     <description>OBEX:IrXfer, (TinyTP)</description></item>
    /// <item><term>OBEX general</term>
    ///     <description>OBEX, (TinyTP)</description></item>
    /// <item><term>Printing</term>
    ///     <description>IrLPT, IrLMP mode</description></item>
    /// <item><term>IrCOMM e.g. to modems</term>
    ///     <description>IrDA:IrCOMM, IrCOMM 9-Wire/Cooked mode</description></item>
    /// </list>
    /// The modes required by the last two are set by socket option, as noted 
    /// for IrCOMM above.
    /// </para>
    /// <para>
    /// Of course the library also includes specific OBEX protocol support, both 
    /// client and server, see <see cref="T:InTheHand.Net.ObexWebRequest"/> etc.
    /// </para>
    /// </remarks>
    /// -
    /// <example>Example code to connect to a IrCOMM service would be as 
    /// follows.
    /// <code lang="VB.NET">
    /// Public Shared Sub Main()
    ///   Dim cli As New IrDAClient
    ///   ' Set IrCOMM Cooked/9-wire mode.
    ///   cli.Client.SetSocketOption(IrDASocketOptionLevel.IrLmp, _
    ///     IrDASocketOptionName.NineWireMode, _
    ///     1)  ' equivalent to 'True'
    ///   ' Connect
    ///   cli.Connect("IrDA:IrCOMM")
    ///   ' Connected, now send and receive e.g. by using the 
    ///   ' NetworkStream returned by cli.GetStream
    ///   ...
    /// End Sub
    /// </code>
    /// </example>
    /// -
    /// <seealso cref="N:InTheHand.Net.Sockets"/>
    /// <seealso cref="T:System.Net.Sockets.IrDAClient"/>
    public class IrDAClient : IDisposable
	{
        private bool cleanedUp = false;

        #region Constructor
        /// <overloads>
        /// Initializes a new instance of the <see cref="IrDAClient"/> class,
        /// and optionally connects to a peer device.
        /// </overloads>
        /// ----
        /// <summary>
		/// Initializes a new instance of the <see cref="IrDAClient"/> class.
		/// </summary>
        /// <remarks>
        /// <para>
        /// It then allows discovery of all devices in range using <see 
        /// cref="M:InTheHand.Net.Sockets.IrDAClient.DiscoverDevices"/>, then a 
        /// connection can be made to the  required service on the chosen peer using <see 
        /// cref="M:InTheHand.Net.Sockets.IrDAClient.Connect(InTheHand.Net.IrDAEndPoint)"/>.
        /// Or, given just the  service name a connection will be made to an arbitrary 
        /// peer, using <see cref="M:InTheHand.Net.Sockets.IrDAClient.Connect(System.String)"/>.  This is 
        /// most useful when it is expected that there will be only one device in 
        /// range &#x2014; as is often the case.</para>
        /// </remarks>
		public IrDAClient()
		{
            try
            {
                clientSocket = new Socket(AddressFamily.Irda, SocketType.Stream, ProtocolType.IP);
            }
            catch (SocketException)
            {
                //added for platforms where the legacy CE specific address family constant is used
                clientSocket = new Socket(AddressFamily32.Irda, SocketType.Stream, ProtocolType.IP);
            }
		}


        /// <summary>
        /// Initializes a new instance of the <see cref="T:InTheHand.Net.Sockets.IrDAClient"/> 
        /// class and connects to the specified service name.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>This is 
        /// equivalent to calling the default constructor followed by 
        /// <see cref="M:InTheHand.Net.Sockets.IrDAClient.Connect(System.String)"/>.      
        /// </para>
        /// <para>
        /// As noted the connection will be made to an arbitrary peer.  This is 
        /// most useful when it is expected that there will be only one device in 
        /// range &#x2014; as is often the case.  If a connection is to be made to
        /// a particular remote peer, then use the 
        /// <see cref="M:InTheHand.Net.Sockets.IrDAClient.#ctor(InTheHand.Net.IrDAEndPoint)"/>
        /// overload.
        /// </para>
        /// <para>
        /// Infrared connections are made by specifying a Service Name, which can 
        /// be any value provided the participating devices refer the same name.
        /// </para>
        /// <para>
        /// See <see cref="M:InTheHand.Net.Sockets.IrDAClient.Connect(System.String)"/> 
        /// for the errors that can occur.
        /// </para>
        /// </remarks>
        /// -
        /// <param name="service">
        /// A <see cref="T:System.String"/> containing the service name to connect to.
        /// </param>
        public IrDAClient(string service)
            : this()
		{
			this.Connect(service);
		}


        /// <summary>
        /// Initializes a new instance of the <see cref="T:InTheHand.Net.Sockets.IrDAClient"/> 
        /// class and connects to the specified endpoint.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is 
        /// equivalent to calling the default constructor followed by 
        /// <see cref="M:InTheHand.Net.Sockets.IrDAClient.Connect(InTheHand.Net.IrDAEndPoint)"/>.
        /// </para>
        /// <para>
        /// The endpoint specifies both the peer device and service name 
        /// to connect to.  If only one device is expected to be in range, or 
        /// an arbitrary peer device is suitable, then one can use 
        /// <see cref="M:InTheHand.Net.Sockets.IrDAClient.#ctor(System.String)"/> instead.
        /// </para>
        /// </remarks>
        /// <param name="remoteEP">
        /// An <see cref="IrDAEndPoint"/> initialised with the address of the peer
        /// device and the service name to connect to.
        /// </param>
		public IrDAClient(IrDAEndPoint remoteEP) : this()
		{
			this.Connect(remoteEP);
		}

		internal IrDAClient(Socket acceptedSocket)
		{
            this.Client = acceptedSocket;
            active = true;
        }
        #endregion

        #region Active
        private bool active = false;

        /// <summary>
        /// Gets or set a value that indicates whether a connection has been made. 
        /// </summary>
        protected bool Active
        {
            get
            {
                return active;
            }
            set
            {
                active = value;
            }
        }
        #endregion

        #region Available
        /// <summary>
		/// The number of bytes of data received from the network and available to be read.
		/// </summary>
		public int Available
		{
			get
			{
                EnsureNotDisposed();
                return clientSocket.Available;
			}
        }
        #endregion

        #region Client

        private Socket clientSocket;


        /// <summary>
		/// Gets or sets the underlying <see cref="T:System.Net.Sockets.Socket"/>.
		/// </summary>
        /// <remarks>
        /// This is particularly useful as it allows setting socket 
        /// options, for instance IrCOMM Cooked mode, ie
        /// <see cref="F:InTheHand.Net.Sockets.IrDASocketOptionName.NineWireMode"/>.
        /// </remarks>
        /// <example>Example code to connect to a IrCOMM service would be as 
        /// follows, note the use of the Client property.
        /// <code lang="VB.NET">
        /// Public Shared Sub Main()
        ///    Dim cli As New IrDAClient
        ///    ' Set IrCOMM Cooked/9-wire mode.
        ///    cli.Client.SetSocketOption( _
        ///      IrDASocketOptionLevel.IrLmp, _
        ///      IrDASocketOptionName.NineWireMode, _
        ///      1)  ' representing true
        ///    ' Connect
        ///    cli.Connect("IrDA:IrCOMM")
        ///    ' Connected, now send and receive e.g. by using the 
        ///    ' NetworkStream returned by cli.GetStream
        ///    ...
        /// End Sub
        /// </code>
        /// </example>
        public Socket Client
		{
            [System.Diagnostics.DebuggerStepThrough]
			get
			{
                return clientSocket;
			}
			set
			{
                this.clientSocket = value;
			}
        }
        #endregion

        #region Connected
        /// <summary>
		/// Gets a value indicating whether the underlying <see cref="Socket"/> for an <see cref="IrDAClient"/> is connected to a remote host.
		/// </summary>
		public bool Connected
		{
			get
			{
                if (clientSocket == null) {
                    return false;
                }
				return clientSocket.Connected;
			}
        }
        #endregion

        #region Discover Devices
        /// <summary>
		/// Obtains information about available devices.
		/// </summary>
        /// -
        /// <remarks>
		/// <para>Returns a maximum of 8 devices, for more flexibility use the other overloads.</para>
        /// </remarks>
        /// -
        /// <returns>The discovered devices as an array of <see cref="T:InTheHand.Net.Sockets.IrDADeviceInfo"/>.</returns>
		public IrDADeviceInfo[] DiscoverDevices()
		{
            EnsureNotDisposed();
            return DiscoverDevices(8, clientSocket);
		}

		/// <summary>
		/// Obtains information about a specified number of devices.
		/// </summary>
        /// -
		/// <param name="maxDevices">The maximum number of devices to get information about.</param>
        /// -
        /// <returns>The discovered devices as an array of <see cref="T:InTheHand.Net.Sockets.IrDADeviceInfo"/>.</returns>
		public IrDADeviceInfo[] DiscoverDevices(int maxDevices)
		{
            EnsureNotDisposed();
            return DiscoverDevices(maxDevices, clientSocket);
		}

		/// <summary>
		/// Obtains information about available devices using a socket.
		/// </summary>
        /// -
		/// <param name="maxDevices">The maximum number of devices to get information about.</param>
		/// <param name="irdaSocket">A <see cref="T:System.Net.Sockets.Socket"/>
        /// to be uses to run the discovery process.
        /// It should have been created for the IrDA protocol</param>
        /// -
        /// <returns>The discovered devices as an array of <see cref="T:InTheHand.Net.Sockets.IrDADeviceInfo"/>.</returns>
		public static IrDADeviceInfo[] DiscoverDevices(int maxDevices, Socket irdaSocket)
		{
            if (irdaSocket == null) {
                throw new ArgumentNullException("irdaSocket");
            }
            const int MaxItemsInHugeBuffer = (Int32.MaxValue - 4) / 32;
            if (maxDevices > MaxItemsInHugeBuffer || maxDevices < 0) {
                throw new ArgumentOutOfRangeException("maxDevices");
            }
            //
			byte[] buffer = irdaSocket.GetSocketOption(IrDASocketOptionLevel.IrLmp, IrDASocketOptionName.EnumDevice, 4 + (maxDevices * 32));
            return ParseDeviceList(buffer);
		}

        // A separate public method to allow unit-testing.
        /// <exclude/>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
        public static IrDADeviceInfo[] ParseDeviceList(byte[] buffer)
        {
            if (buffer == null) {
                throw new ArgumentNullException("buffer");
            }
            const int BytesInAnInt32 = 4;
            if (buffer.Length < BytesInAnInt32) {
                throw new ArgumentException("DEVICE_LIST buffer must be at least four bytes long.");
            }
            //
			int count = BitConverter.ToInt32(buffer, 0);

			//create array for results
			IrDADeviceInfo[] idia = new IrDADeviceInfo[count];
            const int devInfoLen = 29;

            //We could check that the buffer is big enough to suit its 'count' value.
            //If this ever happened currently we would just fail with IndexOutOfRangeException
            //int expectedLength = BytesInAnInt32 + count * devInfoLen;
            //if (expectedLength > buffer.Length) {
            //    throw new ArgumentException("Buffer is smaller than the count of items requires.");
            //}
            
			for(int iDev = 0; iDev < count; iDev++)
			{
				byte[] id = new byte[4];
				Buffer.BlockCopy(buffer, 4 + (iDev*devInfoLen), id, 0, 4);
				IrDAAddress devid = new IrDAAddress(id);

                //hints
                IrDAHints hints = (IrDAHints)BitConverter.ToInt16(buffer, 30 + (iDev * devInfoLen));
                //charset
                IrDACharacterSet charset = (IrDACharacterSet)buffer[32 + (iDev * devInfoLen)];
                
                //name
                Encoding e = null;
                switch (charset)
                {
                    case IrDACharacterSet.ASCII:
                        e = Encoding.ASCII;
                        break;
                    case IrDACharacterSet.Unicode:
                        e = Encoding.Unicode;
                        break;
                    default:
                        e = Encoding.GetEncoding(28590 + (int)charset);
                        break;
                }
				string name = e.GetString(buffer, 8 + (iDev*devInfoLen), 22);
                int nullIndex = name.IndexOf('\0');
				//trim nulls
				if(nullIndex > -1)
				{
					name = name.Substring(0, nullIndex);
				}
#if NETCF
                // PPC doesn't fill the charset field! :-(  We'll attempt
                // to detect the Unicode encoding, but not the ISO-8859-X ones:
                // as their strings will display at least partially -- dropping
                // the high chars, but also because those encodings are not
                // really supported by CF anyway.
                if (Environment.OSVersion.Platform == PlatformID.WinCE) {
                    // This assert is valid, but very annoying when running the 
                    // unit-tests so we'll remove it for now.
                    // System.Diagnostics.Debug.Assert(charset == 0, "should charset==0 as field unset on CE");
                    try {
                        e = Encoding.Unicode;
                        string nameGuessUnicode = e.GetString(buffer, 8 + (iDev * devInfoLen), 22);
                        //trim nulls
                        int nullIndexGU = nameGuessUnicode.IndexOf('\0');
                        if (nullIndexGU > -1) {
                            nameGuessUnicode = nameGuessUnicode.Substring(0, nullIndexGU);
                        }
                        // If more sense was made of the bytes as Unicode, then return
                        // that string.
                        // e.g. a unicode string "abc" is 61-00-62-00-63-00 which
                        // ASCII will see as "A" and Unicode as "ABC"
                        if (nameGuessUnicode.Length > name.Length) {
                            name = nameGuessUnicode;
                        }
                    } catch { }
                }
#endif

                idia[iDev] = new IrDADeviceInfo(devid, name, hints, charset);			
				
			}

			return idia;
        }
        #endregion

        #region Remote Machine Name

        /// <summary>
		/// Gets the name of the peer device using the specified socket.
		/// </summary>
        /// <param name="irdaSocket">A connected IrDA <c>Socket</c>.</param>
		/// <returns>The name of the remote device.</returns>
        /// -
        /// <remarks>
        /// This finds the name of the device to which the socket is connection, 
        /// an exception will occur if the socket is not connected.
        /// </remarks>
        /// -
        /// <exception cref="T:System.ArgumentNullException">
        /// <c>s</c> is null (<c>Nothing</c> in Visual Basic).
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The remote device is not present in the list of discovered devices.
        /// </exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// The socket is not connected.
        /// </exception>
        public static string GetRemoteMachineName(Socket irdaSocket)
		{
			if(irdaSocket == null)
			{
                throw new ArgumentNullException("irdaSocket", "GetRemoteMachineName requires a valid Socket");
			}
            if (!irdaSocket.Connected)
                throw new InvalidOperationException ("The socket must be connected to a device to get the remote machine name.");
			//get remote endpoint
			IrDAEndPoint ep = (IrDAEndPoint)irdaSocket.RemoteEndPoint;
            IrDAAddress a = ep.Address;

			//lookup devices and search for match
			IrDADeviceInfo[] idia = DiscoverDevices(10, irdaSocket);

			foreach(IrDADeviceInfo idi in idia)
			{
                if (a == idi.DeviceAddress)
                {
                    return idi.DeviceName;
                }
			}

            // See unit-test "CheckExceptionThrownByGetRemoteMachineName".
            throw ExceptionFactory.ArgumentOutOfRangeException(null, "No matching device discovered.");
		}

		/// <summary>
		/// Gets the name of the peer device participating in the communication.
		/// </summary>
        /// -
        /// <remarks>
        /// This finds the name of the device to which the client is connection, 
        /// an exception will occur if the socket is not connected.
        /// </remarks>
        /// -
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// If the remote device is not found in the discovery cache.
        /// </exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// The socket is not connected.
        /// </exception>
        public string RemoteMachineName
		{
			get
			{
                EnsureNotDisposed();
				return GetRemoteMachineName(this.clientSocket);
			}
        }
        #endregion

        #region Connect


        /// <overloads>
        /// Forms a connection to the specified peer service.
        /// </overloads>
        /// --
        /// <summary>
        /// Forms a connection to the specified endpoint.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The endpoint specifies both the peer device and service name 
        /// to connect to.  If only one device is expected to be in range, or 
        /// an arbitrary peer device is suitable, then one can use 
        /// <see cref="M:InTheHand.Net.Sockets.IrDAClient.Connect(System.String)"/> instead.
        /// </para>
        /// </remarks>
        /// <param name="remoteEP">
        /// An <see cref="IrDAEndPoint"/> initialised with the address of the peer
        /// device and the service name to connect to.
        /// </param>
#if CODE_ANALYSIS
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")
            /* FxCop wanted us to use parameter type EndPoint! */]
#endif
        public void Connect(IrDAEndPoint remoteEP)
		{
            if (cleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (remoteEP == null)
            {
                throw new ArgumentNullException("remoteEP");
            }

            clientSocket.Connect(remoteEP);
            active = true;
		}

		/// <summary>
        /// Forms a connection to the specified service on an arbitrary peer.
		/// </summary>
        /// <remarks>
        /// As noted the connection will be made to an arbitrary peer.  This is 
        /// most useful when it is expected that there will be only one device in 
        /// range &#x2014; as is often the case.  If a connection is to be made to
        /// a particular remote peer, then use 
        /// <see cref="M:InTheHand.Net.Sockets.IrDAClient.Connect(InTheHand.Net.IrDAEndPoint)"/>.
        /// </remarks>
        /// <param name="service">The Service Name to connect to eg "<c>OBEX</c>".
        /// In the very uncommon case where a connection is to be made to a 
        /// specific LSAP-SEL (port number), then use 
        /// the form "<c>LSAP-SELn</c>", where n is an integer.</param>
        /// -
        /// <exception cref="T:System.InvalidOperationException">
        /// No peer IrDA device was found.  The exception has message &#x201C;No device&#x201D;.
        /// </exception>
        /// <exception cref="T:System.Net.Sockets.SocketException">
        /// A connection could not be formed.  See the exception message or 
        /// <see cref="P:System.Net.Sockets.SocketException.SocketErrorCode"/> 
        /// (or <see cref="P:System.Net.Sockets.SocketException.ErrorCode"/> on NETCF) 
        /// for what error occurred.
        /// </exception>
        public void Connect(string service)
		{
			IrDADeviceInfo[] devs = this.DiscoverDevices(1);
			if(devs.Length > 0)
			{
				IrDAEndPoint ep = new IrDAEndPoint(devs[0].DeviceAddress, service);
				Connect(ep);
			}
			else
			{
				throw new InvalidOperationException("No device");
			}
        }

        #region Begin Connect
        /// <overloads>
        /// Begins an asynchronous request for a remote host connection.
        /// </overloads>
        /// -
        /// <summary>
        /// Begins an asynchronous request for a remote host connection.
        /// The remote host is specified by an endpoint. 
        /// </summary>
        /// -
        /// <param name="remoteEP">
        /// An <see cref="IrDAEndPoint"/> initialised with the address of the peer
        /// device and the service name to connect to.
        /// </param>
        /// <param name="requestCallback">An AsyncCallback delegate that references the method to invoke when the operation is complete.</param>
        /// <param name="state">A user-defined object that contains information about the connect operation.
        /// This object is passed to the requestCallback delegate when the operation is complete.</param>
        /// -
        /// <returns>An <see cref="T:System.IAsyncResult"/> that represents the 
        /// asynchronous connect, which could still be pending.
        /// </returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "1#Callback")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public IAsyncResult BeginConnect(IrDAEndPoint remoteEP, AsyncCallback requestCallback, object state)
        {
            return this.Client.BeginConnect(remoteEP, requestCallback, state);
        }

        /// <summary>
        /// Begins an asynchronous request for a remote host connection.
        /// The remote host is specified by a service name (string). 
        /// </summary>
        /// -
        /// <param name="service">The service name of the remote host.</param>
        /// <param name="requestCallback">An AsyncCallback delegate that references the method to invoke when the operation is complete.</param>
        /// <param name="state">A user-defined object that contains information about the connect operation.
        /// This object is passed to the requestCallback delegate when the operation is complete.</param>
        /// -
        /// <returns>An <see cref="T:System.IAsyncResult"/> that represents the 
        /// asynchronous connect, which could still be pending.
        /// </returns>
        /// -
        /// <remarks>
        /// <para>
        /// See <see cref="M:InTheHand.Net.Sockets.IrDAClient.Connect(System.String)"/> 
        /// for the errors that can occur.
        /// </para>
        /// </remarks>
#if CODE_ANALYSIS
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "1#Callback")]
#endif
        public IAsyncResult BeginConnect(string service, AsyncCallback requestCallback, object state)
        {
            IrDADeviceInfo[] devs = this.DiscoverDevices(1);
            if (devs.Length > 0)
            {
                IrDAEndPoint ep = new IrDAEndPoint(devs[0].DeviceAddress, service);
                return BeginConnect(ep, requestCallback, state);
            }
            else
            {
                throw new InvalidOperationException("No remote device");
            }  
        }
        #endregion

        #region End Connect
        /// <summary>
        /// Asynchronously accepts an incoming connection attempt.
        /// </summary>
        /// <param name="asyncResult">An <see cref="IAsyncResult"/> object returned
        /// by a call to <see cref="M:InTheHand.Net.Sockets.IrDAClient.BeginConnect(InTheHand.Net.IrDAEndPoint,System.AsyncCallback,System.Object)"/>
        /// / <see cref="M:InTheHand.Net.Sockets.IrDAClient.BeginConnect(System.String,System.AsyncCallback,System.Object)"/>.
        /// </param>
        public void EndConnect(IAsyncResult asyncResult)
        {
            this.Client.EndConnect(asyncResult);
            this.active = true;
        }

#if FX4
        public System.Threading.Tasks.Task ConnectAsync(IrDAEndPoint remoteEP, object state)
        {
            return System.Threading.Tasks.Task.Factory.FromAsync(
                BeginConnect, EndConnect,
                remoteEP, state);
        }
#endif
        #endregion

        #endregion

        #region Close
        /// <summary>
        /// Closes the <see cref="IrDAClient"/> and the underlying connection.
        /// </summary>
        /// -
        /// <remarks>The two XxxxxClient classes produced by Microsoft (TcpClient, 
        /// and IrDAClient in the NETCF) have various documented behaviours and various
        /// actual behaviours for close/dispose/finalize on the various platforms. :-(
        /// The current TcpClient implementation is that 
        /// Close/Dispose closes the connection by closing the underlying socket and/or
        /// NetworkStream, and finalization doesn't close either.  This is the behaviour
        /// we use for the here (for <see cref="T:InTheHand.Net.Sockets.BluetoothClient"/>,
        /// <see cref="T:InTheHand.Net.Sockets.IrDAClient"/>).  (The documentation in MSDN for 
        /// <see cref="T:System.Net.Sockets.TcpClient"/> is still wrong by-the-way,
        /// see <see href="https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=158480">
        /// Microsoft feedback #158480</see>).
        /// </remarks>
        public void Close()
        {
            Dispose();
        }
        #endregion

        #region Get Stream

        private NetworkStream dataStream;

        /// <summary>
        /// Returns the <see cref="T:System.Net.Sockets.NetworkStream"/> used to send and receive data.
        /// </summary>
        /// -
        /// <returns>The underlying <c>NetworkStream</c>.</returns>
        /// -
        /// <remarks>
        /// <para>
        /// <c>GetStream</c> returns a <c>NetworkStream</c> 
        /// that you can use to send and receive data. The <c>NetworkStream</c> class 
        /// inherits from the <see cref="T:System.IO.Stream"/> class, which provides a 
        /// rich collection of methods and properties used to facilitate network communications.
        /// </para>
        /// <para>You must call the <see cref="M:InTheHand.Net.Sockets.IrDAClient.Connect(InTheHand.Net.IrDAEndPoint)"/> 
        /// method, or one of its overloads, first, or 
        /// the <c>GetStream</c> method will throw an <c>InvalidOperationException</c>.
        /// After you have obtained the <c>NetworkStream</c>, call the 
        /// <see cref="M:System.Net.Sockets.NetworkStream.Write(System.Byte[],System.Int32,System.Int32)"/>
        /// method to send data to the remote host.
        /// Call the <see cref="M:System.Net.Sockets.NetworkStream.Read(System.Byte[],System.Int32,System.Int32)"/> 
        /// method to receive data arriving from the remote host.
        /// Both of these methods block until the specified operation is performed.
        /// You can avoid blocking on a read operation by checking the 
        /// <see cref="P:System.Net.Sockets.NetworkStream.DataAvailable"/> property.
        /// A <c>true</c> value means that data has arrived from the remote host and
        /// is available for reading. In this case, <c>Read</c> is 
        /// guaranteed to complete immediately.
        /// If the remote host has shutdown its connection, <c>Read</c> will 
        /// immediately return with zero bytes.
        /// </para>
        /// <note>
        /// Closing the <c>NetworkStream</c> closes the connection.  
        /// Similarly Closing, Disposing, or the finalization of the <c>IrDAClient</c> 
        /// Disposes the <c>NetworkStream</c>.
        /// This is new behaviour post 2.0.60828.0.
        /// <!-- [dodgy?]TcpClient documentation:
        /// You must close the NetworkStream when you are through sending and
        /// receiving data. Closing TcpClient does not release the NetworkStream.-->
        /// </note>
        /// <note>
        /// If you receive a SocketException, use SocketException.ErrorCode to obtain
        /// the specific error code. After you have obtained this code, you can refer
        /// to the Windows Sockets version 2 API error code documentation in MSDN
        /// for a detailed description of the error.
        /// </note>
        /// </remarks>
        /// -
        /// <exception cref="T:System.InvalidOperationException">
        /// The <see cref="IrDAClient"/> is not connected to a remote host.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">
        /// The <c>IrDAClient</c> has been closed.
        /// </exception>
#if CODE_ANALYSIS
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
#endif
        public NetworkStream GetStream()
        {
            if (cleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (!this.Client.Connected)
            {
                throw new InvalidOperationException("The operation is not allowed on non-connected sockets.");
            }

            if (dataStream == null)
            {
                dataStream = new NetworkStream(this.Client, true);
            }

            return dataStream;
        }
        #endregion

        #region IDisposable Members

		/// <summary>
        /// Releases the unmanaged resources used by the <see cref="IrDAClient"/> and optionally releases the managed resources.
		/// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
            if(!cleanedUp)
            {
                if (disposing)
                {
                    IDisposable idStream = dataStream;
                    if (idStream != null)
                    {
                        //dispose the stream which will also close the socket
                        idStream.Dispose();
                    }
                    else
                    {
                        // Changed from the property to using the field reference
                        // to make it clear to FxCop the we're disposing the socket
                        // in that field.
                        if (this.clientSocket != null)
                        {
                            this.clientSocket.Close();
                            this.clientSocket = null;
                        }
                    }
                }

                cleanedUp = true;
            }
        }

        private void EnsureNotDisposed()
        {
            Debug.Assert(cleanedUp == (clientSocket == null), "always consistent!! ("
                + cleanedUp + " != " + (clientSocket == null) + ")");
            if (cleanedUp || (clientSocket == null))
                throw new ObjectDisposedException("IrDAClient");
        }

        /// <summary>
        /// Closes the <see cref="IrDAClient"/> and the underlying connection.
        /// </summary>
        /// -
        /// <seealso cref="M:InTheHand.Net.Sockets.IrDAClient.Close"/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // This seems unnecessary, the only effect it has is to set this.cleanedUp = true;
        // which is unimportant as any accesses will access the socket which will itself
        // likely be undergoing finalization and thus later accesses will fail.
        ///// <summary>
        ///// Frees resources used by the <see cref="IrDAClient"/> class, 
        ///// but doesn't affect the underlying connection.
        ///// </summary>
        //~IrDAClient()
        //{
        //    Dispose(false);
        //}

		#endregion

    }
}
#endif
