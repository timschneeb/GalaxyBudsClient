using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace InTheHand.Net.Bluetooth.Factory
{
    internal class BluetoothRfcommStreamConnector : BluetoothConnector
    {
        NetworkStream m_netStrm;
#if DEBUG
        internal
#endif
        readonly CommonRfcommStream m_conn;

        //----
        internal BluetoothRfcommStreamConnector(IUsesBluetoothConnectorImplementsServiceLookup parent, CommonRfcommStream conn)
            : base(parent)
        {
            m_conn = conn;
        }

        //----
        public System.Net.Sockets.NetworkStream GetStream()
        {
            GetStream2(); // Check validity (but a sub-type, so hard to use here).
            if (m_netStrm == null)
                m_netStrm = new RfcommDecoratorNetworkStream(m_conn);
            return m_netStrm;
        }

        public System.IO.Stream GetStream2()
        {
            //if (cleanedUp) {
            //    throw new ObjectDisposedException(base.GetType().FullName);
            //}
            if (!Connected) {
                throw new InvalidOperationException("The operation is not allowed on non-connected sockets.");
            }
            return m_conn;
        }

        public LingerOption LingerState
        {
            get { return m_conn.LingerState; }
            set { m_conn.LingerState = value; }
        }

        //----
        public bool Connected
        {
            get { return m_conn.Connected; }
        }

        public void Dispose()
        {
            m_disposed = true;
            m_conn.Close();
            //
            // Abort the Connect (usually the SDP lookup) if we're still in that phase
            Connect_SetAsCompleted_CompletedSyncFalse(null, new ObjectDisposedException("BluetoothClient"));
        }

        //----
        protected override IAsyncResult ConnBeginConnect(BluetoothEndPoint remoteEP, AsyncCallback requestCallback, object state)
        {
            return m_conn.BeginConnect(remoteEP, requestCallback, state);
        }

        protected override void ConnEndConnect(IAsyncResult asyncResult)
        {
            m_conn.EndConnect(asyncResult);
        }

        //----
        public int Available
        {
            get { return m_conn.AmountInReadBuffers; }
        }

        //--
        public BluetoothEndPoint RemoteEndPoint
        {
            get
            {
                if (m_remoteEndPoint == null) {
                    BluetoothAddress addr = m_conn.RemoteAddress;
                    Debug.Assert(addr != null, "port.IsConnected should have returned the remote address!");
                    // Don't know the remote port unfortunately so just use 0/-1.
                    if (_remotePort.HasValue)
                        m_remoteEndPoint = new BluetoothEndPoint(addr, BluetoothService.Empty, _remotePort.Value);
                    else
                        m_remoteEndPoint = new BluetoothEndPoint(addr, BluetoothService.Empty);
                }
                return m_remoteEndPoint;
            }
        }

    }//class
}
