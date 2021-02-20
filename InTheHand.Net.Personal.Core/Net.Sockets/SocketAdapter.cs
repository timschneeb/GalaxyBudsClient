using System;
using System.Net;
using System.Net.Sockets;

namespace InTheHand.Net.Sockets
{
    // Could be public...?
    /// <summary>
    /// Provide a <see cref="T:System.Net.Sockets.Socket">System.Net.Sockets.Socket</see>-like
    /// interace to another connection type e.g. a <see cref="T:InTheHand.Net.Sockets.BluetoothClient"/>
    /// </summary>
    /// -
    /// <remarks>
    /// <para>See class <see cref="T:InTheHand.Net.Sockets.SocketClientAdapter"/>
    /// for an implementation that adapts <see cref="T:InTheHand.Net.Sockets.BluetoothClient"/>
    /// etc to the <see cref="T:System.Net.Sockets.Socket">Socket</see>-like interface.
    /// That is required as on Widcomm/Broadcom <see cref="T:InTheHand.Net.Sockets.BluetoothClient"/>
    /// does not support getting a <see cref="T:System.Net.Sockets.Socket"/> from
    /// the <see cref="T:InTheHand.Net.Sockets.BluetoothClient.Client"/> property.
    /// Motivated by upgrading of <see cref="T:InTheHand.Net.ObexListener"/> to
    /// be usable on Widcomm.
    /// </para>
    /// </remarks>
    internal abstract class SocketAdapter : IDisposable
    {
        public abstract EndPoint LocalEndPoint { get;}
        public abstract EndPoint RemoteEndPoint { get;}
        public abstract int Available { get;}
        //
        public abstract int Receive(byte[] buffer, int size, SocketFlags socketFlags);
        public abstract int Receive(byte[] buffer, int offset, int size, SocketFlags socketFlags);
        public abstract int Send(byte[] buffer);
        public abstract void Close();

        //--
        protected static void CheckSocketFlags(SocketFlags socketFlags)
        {
            if (socketFlags != SocketFlags.None)
                throw new ArgumentException("Only SocketFlags.None is supported.");
        }

        //--
        public void Dispose()
        {
            Dispose(true);
        }

        protected abstract void Dispose(bool disposing);

    }
}
