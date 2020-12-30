// 32feet.NET - Personal Area Networking for .NET
//
// Copyright (c) 2008-2010 In The Hand Ltd, All rights reserved.
// Copyright (c) 2008-2010 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Diagnostics.CodeAnalysis;

namespace InTheHand.Net.Bluetooth.Factory
{
    /// <exclude/>
    public abstract class CommonBluetoothClient : CommonDiscoveryBluetoothClient, IUsesBluetoothConnectorImplementsServiceLookup
    {
        // !!!
#pragma warning disable 1591 // Missing XML comment for publicly visible type or member '
        readonly BluetoothFactory _fcty;
        readonly BluetoothRfcommStreamConnector m_conn;

        //--------------
        [CLSCompliant(false)]
        protected CommonBluetoothClient(BluetoothFactory factory, CommonRfcommStream conn)
        {
            Debug.Assert(factory != null, "NULL factory");
            _fcty = factory;
            m_conn = new BluetoothRfcommStreamConnector(this, conn);
        }

        //--------------
#if DEBUG
        internal CommonRfcommStream Testing_GetConn() { return m_conn.m_conn; }
#endif

        public override System.Net.Sockets.NetworkStream GetStream()
        {
            return m_conn.GetStream();
        }

        public override LingerOption LingerState
        {
            get { return m_conn.LingerState; }
            set { m_conn.LingerState = value; }
        }

        //--------------------------------------------------------------
        public override bool Connected
        {
            get { return m_conn.Connected; }
        }

        protected override void Dispose(bool disposing)
        {
            try {
                // Client shouldn't close the connection when it's being
                // Finalized.  It is allowed for the consumer just to keep
                // a reference to the Stream and discard the BtCli.
                if (disposing) {
                    m_conn.Dispose();
                }
            } finally {
                base.Dispose(disposing);
            }
        }

        //--
        public sealed override void Connect(BluetoothEndPoint remoteEP)
        {
            BeforeConnectAttempt(remoteEP.Address);
            try {
                m_conn.Connect(remoteEP);
            } finally {
                AfterConnectAttempt();
            }
        }

        public sealed override IAsyncResult BeginConnect(BluetoothEndPoint remoteEP, AsyncCallback requestCallback, object state)
        {
            BeforeConnectAttempt(remoteEP.Address);
            return m_conn.BeginConnect(remoteEP, requestCallback, state);
        }

        public sealed override void EndConnect(IAsyncResult asyncResult)
        {
            try {
                m_conn.EndConnect(asyncResult);
            } finally {
                AfterConnectAttempt();
            }
        }

        protected virtual void BeforeConnectAttempt(BluetoothAddress target)
        { }
        protected virtual void AfterConnectAttempt()
        { }

        //--
        /// <summary>
        /// When overidden, initiates 
        /// lookup the SDP record with the give Service Class Id
        /// to find the RFCOMM port number (SCN) that the server is listening on.
        /// The process returns a list of port numbers.
        /// </summary>
        /// <param name="address">The remote device.
        /// </param>
        /// <param name="serviceGuid">The Service Class Id.
        /// </param>
        /// <param name="asyncCallback">callback</param>
        /// <param name="state">state</param>
        /// <returns>IAsyncResult</returns>
        [SuppressMessage("Microsoft.Security", "CA2119:SealMethodsThatSatisfyPrivateInterfaces")]
        public abstract IAsyncResult BeginServiceDiscovery(
            BluetoothAddress address, Guid serviceGuid,
            AsyncCallback asyncCallback, Object state);

        /// <summary>
        /// When overidden, 
        /// completes the SDP Record to port number lookup process
        /// </summary>
        /// -
        /// <param name="ar">IAsyncResult from <see cref="BeginServiceDiscovery"/>.
        /// </param>
        /// -
        /// <remarks>
        /// <para>There must be at least one entry in the result list for each
        /// Service Record found for the specified Service Class Id.  This
        /// allows us to know if no records were found, or that records were
        /// found but none of them were for RFCOMM.
        /// If a particular record does not have a RFCOMM port then -1 (negative
        /// one should be added to the list for it).
        /// </para>
        /// <para>The process may throw an exception if an error occurs, e.g.
        /// the remote device did not respond.
        /// </para>
        /// </remarks>
        /// -
        /// <returns>A <see cref="T:System.Collections.Generic.List{System.Int32}"/>
        /// with at least one entry for each Service Record
        /// found for the specified Service Class Id, the item being -1 if the
        /// record has no port. is .
        /// </returns>
        [SuppressMessage("Microsoft.Security", "CA2119:SealMethodsThatSatisfyPrivateInterfaces")]
        public abstract List<int> EndServiceDiscovery(IAsyncResult ar);

        //--------------------------------------------------------------
        public override int Available
        {
            get { return m_conn.Available; }
        }

        //--------------------------------------------------------------
        public override System.Net.Sockets.Socket Client
        {
            get { throw new NotSupportedException("This stack does not use Sockets."); }
            set { throw new NotSupportedException("This stack does not use Sockets."); }
        }

        //--------------------------------------------------------------
        public override bool Authenticate
        {
            get { return false; }
            set { throw new NotImplementedException("The method or operation is not implemented."); }
        }

        public override bool Encrypt
        {
            get { return false; }
            set { throw new NotImplementedException("The method or operation is not implemented."); }
        }

        public override BluetoothEndPoint RemoteEndPoint
        {
            get { return m_conn.RemoteEndPoint; }
        }

        public override string GetRemoteMachineName(BluetoothAddress device)
        {
            // This is what we do on Win32.  Good enough??
            IBluetoothDeviceInfo bdi = _fcty.DoGetBluetoothDeviceInfo(device);
            return bdi.DeviceName;
        }

        public override Guid LinkKey
        {
            get { throw new NotImplementedException("The method or operation is not implemented."); }
        }

        public override LinkPolicy LinkPolicy
        {
            get { throw new NotImplementedException("The method or operation is not implemented."); }
        }

        public override string RemoteMachineName
        {
            get { return GetRemoteMachineName(RemoteEndPoint.Address); }
        }

    }
}