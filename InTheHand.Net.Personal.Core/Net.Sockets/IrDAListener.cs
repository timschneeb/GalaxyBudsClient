// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Sockets.IrDAListener
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
	/// Places a socket in a listening state to monitor infrared connections from a specified service or network address.
	/// </summary>
    /// <remarks>This class monitors a service by specifying a service name or a network address.
    /// The listener does not listen until you call one of the <see cref="M:InTheHand.Net.Sockets.IrDAListener.Start"/>
    /// methods.</remarks>
    /// <seealso cref="T:System.Net.Sockets.IrDAListener"/>
#if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
#endif
    public class IrDAListener
	{
		private IrDAEndPoint serverEP;

        #region Constructor
        /// <summary>
		/// Initializes a new instance of the <see cref="IrDAListener"/> class.
		/// </summary>
		/// <param name="ep">The network address to monitor for making a connection.</param>
		public IrDAListener(IrDAEndPoint ep)
		{
            initialize();
            serverEP = ep;
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="IrDAListener"/> class.
		/// </summary>
		/// <param name="service">The name of the service to listen for.</param>
		public IrDAListener(string service)
		{
            initialize();
			serverEP = new IrDAEndPoint(IrDAAddress.None, service);
        }

        private void initialize()
        {
            active = false;

            try
            {
                serverSocket = new Socket(AddressFamily.Irda, SocketType.Stream, ProtocolType.IP);
            }
            catch (SocketException /*se*/)
            {
                //added for platforms where the legacy CE specific address family constant is used
                serverSocket = new Socket(AddressFamily32.Irda, SocketType.Stream, ProtocolType.IP);
            }
        }
        #endregion

        #region Server
        private Socket serverSocket;
		/// <summary>
		/// Gets the underlying network <see cref="Socket"/>.
		/// </summary>
		public Socket Server
		{
			get
			{
				return serverSocket;
			}
        }
        #endregion

        #region Active

        private bool active = false;

        /// <summary>
		/// Gets a value that indicates whether the <see cref="IrDAListener"/> is actively listening for client connections.
		/// </summary>
		public bool Active
		{
			get
			{
				return this.active;
			}
        }
        #endregion

        #region Local Endpoint
        /// <summary>
		/// Gets an <see cref="IrDAEndPoint"/> representing the local device.
		/// </summary>
		public IrDAEndPoint LocalEndpoint
		{
			get
			{
				if(serverSocket!=null)
				{
                    return (IrDAEndPoint)serverSocket.LocalEndPoint;
				}
				return serverEP;
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
                serverSocket.Bind(serverEP);
                serverSocket.Listen(backlog);
				active=true;
			}
        }
        #endregion

        #region Stop
        /// <summary>
		/// Stops the socket from monitoring connections.
		/// </summary>
		public void Stop()
		{
            if (serverSocket != null)
            {
                serverSocket.Close();
                serverSocket = null;
            }

            initialize();
        }
        #endregion

        #region Accept
        /// <summary>
		/// Creates a new socket for a connection.
		/// </summary>
		/// <returns>A socket.</returns>
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
		/// <returns>An <see cref="IrDAClient"/> object.</returns>
		public IrDAClient AcceptIrDAClient()
		{
			Socket s = this.AcceptSocket();
			return new IrDAClient(s);
        }

        #region Async Socket
        /// <summary>
        /// Begins an asynchronous operation to accept an incoming connection attempt.
        /// </summary>
        /// -
        /// <param name="callback">An <see cref="AsyncCallback"/> delegate that references the method to invoke when the operation is complete.</param>
        /// <param name="state">A user-defined object containing information about the accept operation.
        /// This object is passed to the callback delegate when the operation is complete.</param>
        /// -
        /// <returns>An <see cref="IAsyncResult"/> that references the asynchronous creation of the <see cref="Socket"/>.</returns>
        /// -
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="Socket"/> has been closed.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId
= "0#callback")]
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId 
= "0#callback")]
        public IAsyncResult BeginAcceptIrDAClient(AsyncCallback callback, object state)
        {
            if (!active)
            {
                throw new InvalidOperationException("Not listening. You must call the Start() method before calling this method.");
            }

            return serverSocket.BeginAccept(callback, state);
        }

        /// <summary>
        /// Asynchronously accepts an incoming connection attempt and creates a new <see cref="IrDAClient"/> to handle remote host communication.
        /// </summary>
        /// <param name="asyncResult">An <see cref="IAsyncResult"/> returned by a call to the <see cref="BeginAcceptIrDAClient"/> method.</param>
        /// <returns>An <see cref="IrDAClient"/>.</returns>
        public IrDAClient EndAcceptIrDAClient(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }

            Socket s = serverSocket.EndAccept(asyncResult);
            return new IrDAClient(s);
        }

#if FX4
        public System.Threading.Tasks.Task<IrDAClient> AcceptIrDAClientAsync(object state)
        {
            return System.Threading.Tasks.Task.Factory.FromAsync<IrDAClient>(
                BeginAcceptIrDAClient, EndAcceptIrDAClient,
                state);
        }
#endif
        #endregion

        #endregion

        #region Pending
        /// <summary>
		/// Determines if a connection is pending.
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
    }

	
}
#endif
