using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using InTheHand.Net.Bluetooth.Factory;

namespace InTheHand.Net.Sockets
{
    /// <summary>
    /// Provides client connections to a remote Bluetooth L2CAP service.
    /// </summary>
    /// -
    /// <remarks>
    /// <para>For RFCOMM connections use <see cref="T:InTheHand.Net.Sockets.BluetoothClient"/>.
    /// </para>
    /// <para>The normal usage is o create an instance, connect with 
    /// <see cref="M:InTheHand.Net.Sockets.L2CapClient.Connect(InTheHand.Net.BluetoothEndPoint)"/>
    /// or <see cref="M:InTheHand.Net.Sockets.L2CapClient.BeginConnect(InTheHand.Net.BluetoothEndPoint,System.AsyncCallback,System.Object)"/>,
    /// and if successful one then calls <see cref="M:InTheHand.Net.Sockets.L2CapClient.GetStream"/>
    /// to send and receive data.
    /// </para>
    /// <para>See the <see cref="M:InTheHand.Net.Sockets.L2CapClient.Connect(InTheHand.Net.BluetoothEndPoint)"/>
    /// method for more information
    /// on specifying the remote service to connect to.
    /// </para>
    /// </remarks>
    public class L2CapClient
    {
        readonly IL2CapClient m_impl; // HACK ?should be IL2CapClient?

        #region Constructor
#if NETCF
        static L2CapClient()
        {
            InTheHand.Net.PlatformVerification.ThrowException();
        }
#endif

        internal L2CapClient(IL2CapClient impl)
        {
            m_impl = impl;
        }

        internal L2CapClient(IBluetoothClient impl)
        {
            var impl2 = (IL2CapClient)impl;
            m_impl = impl2;
        }

        /// <summary>
        /// Creates a new instance of <see cref="T:InTheHand.Net.Sockets.L2CapClient"/>.
        /// </summary>
        public L2CapClient()
        //    : this(BluetoothFactory.Factory)
        {
            // HACK
            foreach (var f in BluetoothFactory.Factories) {
                try {
                    var cli = f.DoGetL2CapClient();
                    m_impl = cli;
                    return;
                } catch (NotImplementedException) { }
            }//for
            throw new NotSupportedException("L2CapClient");
            //
            //            if (PlatformVerification.IsMonoRuntime) {
            //#if NETCF
            //                throw new RankException("IsMonoRuntime on NETCF!!");
            //#elif BlueZ
            //                m_impl = new InTheHand.Net.Bluetooth.BlueZ.BluezL2CapClient(null);
            //#endif
            //            } else {
            //                m_impl = InTheHand.Net.Bluetooth.Widcomm.WidcommL2CapClient.Create();
            //            }
        }

        //internal L2CapClient(BluetoothFactory factory)
        //    : this(factory.DoGetBluetoothClient())
        //{
        //}
        #endregion

        #region Close
        /// <summary>
        /// Closes the <see cref="T:InTheHand.Net.Sockets.L2CapClient"/> and the underlying connection.
        /// </summary>
        /// -
        /// <seealso cref="M:InTheHand.Net.Sockets.L2CapClient.Dispose"/>
        [DebuggerStepThrough]
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// Closes the <see cref="T:InTheHand.Net.Sockets.L2CapClient"/> and the underlying connection.
        /// </summary>
        /// -
        /// <seealso cref="M:InTheHand.Net.Sockets.L2CapClient.Close"/>
        [DebuggerStepThrough]
        public void Dispose()
        {
            m_impl.Dispose();
        }
        #endregion

        #region Connect
        /// <summary>
        /// Connects to a remote Bluetooth L2CAP service
        /// using the specified remote endpoint.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>The <see cref="T:InTheHand.Net.BluetoothEndPoint"/> must
        /// have the <see cref="P:InTheHand.Net.BluetoothEndPoint.Address"/>
        /// set, and either the <see cref="P:InTheHand.Net.BluetoothEndPoint.Service"/>
        /// or <see cref="P:InTheHand.Net.BluetoothEndPoint.Port"/> properties
        /// set.
        /// The port is the L2CAP PSM number, and if set a connection will be
        /// made to that PSM and the Service Class Id ignored.
        /// Note that only certain PSM values are valid.  See 
        /// <see cref="T:InTheHand.Net.Sockets.L2CapListener"/> for more
        /// information.
        /// </para>
        /// </remarks>
        /// -
        /// <param name="remoteEP">The <see cref="T:InTheHand.Net.BluetoothEndPoint"/>
        /// to which you intend to connect. See the remarks for usage.
        /// </param>
        public void Connect(BluetoothEndPoint remoteEP)
        {
            if (remoteEP == null) {
                throw new ArgumentNullException("remoteEP");
            }
            m_impl.Connect(remoteEP);
        }

        /// <summary>
        /// Begins an asynchronous request for a remote host connection.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>See 
        /// <see cref="M:InTheHand.Net.Sockets.L2CapClient.Connect(InTheHand.Net.BluetoothEndPoint)"/>
        /// for more information.
        /// </para>
        /// </remarks>
        /// -
        /// <param name="remoteEP">The <see cref="T:InTheHand.Net.BluetoothEndPoint"/>
        /// to which you intend to connect.
        /// See 
        /// or <see cref="M:InTheHand.Net.Sockets.L2CapClient.Connect(InTheHand.Net.BluetoothEndPoint)"/>,
        /// for more information.
        /// </param>
        /// <param name="requestCallback">An <see cref="T:System.AsyncCallback"/>
        /// delegate that references the method to invoke when the operation is
        /// complete.
        /// </param>
        /// <param name="state">A user-defined object that contains information
        /// about the connect operation. This object is passed to the
        /// <paramref name="requestCallback"/> delegate when the operation is
        /// complete.
        /// </param>
        /// -
        /// <returns>An <see cref="T:System.IAsyncResult"/> object that
        /// references the asynchronous connection,
        /// which may still be pending.
        /// </returns>
        [DebuggerStepThrough]
        public IAsyncResult BeginConnect(BluetoothEndPoint remoteEP, AsyncCallback requestCallback, object state)
        {
            if (remoteEP == null) {
                throw new ArgumentNullException("remoteEP");
            }
            return m_impl.BeginConnect(remoteEP, requestCallback, state);
        }

        /// <summary>
        /// Asynchronously accepts an incoming connection attempt.
        /// </summary>
        /// -
        /// <param name="asyncResult">An <see cref="T:System.IAsyncResult"/>
        /// object returned by a call to 
        /// or <see cref="M:InTheHand.Net.Sockets.L2CapClient.BeginConnect(InTheHand.Net.BluetoothEndPoint,System.AsyncCallback,System.Object)"/>,
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

        /// <summary>
        /// Returns the <see cref="T:System.IO.Stream"/> used to send and
        /// receive data.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>Note it is NOT a <see cref="T:System.Net.Sockets.NetworkStream"/>.
        /// That type handles SOCK_STREAM connections, whereas L2CAP uses
        /// SOCK_SEQPACKET.
        /// Different Stream subclasses may be returned by different platforms.
        /// </para>
        /// </remarks>
        /// -
        /// <returns>The <see cref="T:System.IO.Stream"/> used to send and
        /// receive data.
        /// </returns>
        [DebuggerStepThrough]
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public Stream GetStream()
        {
            return m_impl.GetStream();
        }

        #region Remote info
        /// <summary>
        /// Get the remote endpoint.
        /// </summary>
        /// -
        /// <value>
        /// The <see cref="T:InTheHand.Net.BluetoothEndPoint"/> with which the 
        /// <see cref="T:InTheHand.Net.Sockets.L2CapClient"/> is communicating.
        /// </value>
        public BluetoothEndPoint RemoteEndPoint
        {
            [DebuggerStepThrough]
            get { return m_impl.RemoteEndPoint; }
        }

        ///// <summary>
        ///// Gets the name of the remote device.
        ///// </summary>
        //[Obsolete("Do we want to support this one??")]
        //public string RemoteMachineName
        //{
        //    [DebuggerStepThrough]
        //    get { return m_impl.RemoteMachineName; }
        //}
        #endregion


        /// <summary>
        /// Get the MTU................
        /// </summary>
        /// <returns>int</returns>
        public int GetMtu()
        {
            return m_impl.GetMtu();
        }

    }
}
