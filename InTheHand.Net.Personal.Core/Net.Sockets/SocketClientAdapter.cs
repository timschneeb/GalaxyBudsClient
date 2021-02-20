using System;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace InTheHand.Net.Sockets
{
    /// <summary>
    /// An adapter that provides a <see cref="T:System.Net.Sockets.Socket">System.Net.Sockets.Socket</see>-like
    /// interface to <see cref="T:InTheHand.Net.Sockets.BluetoothClient"/> etc.
    /// </summary>
    /// -
    /// <remarks>
    /// <para>Required as  on Widcomm/Broadcom <see cref="T:InTheHand.Net.Sockets.BluetoothClient"/>
    /// does not support getting a <see cref="T:System.Net.Sockets.Socket"/> from
    /// the <see cref="T:InTheHand.Net.Sockets.BluetoothClient.Client"/> property.
    /// Motivated by upgrading of <see cref="T:InTheHand.Net.ObexListener"/> to
    /// be usable on Widcomm.
    /// </para>
    /// <para>Also adapts <see cref="T:InTheHand.Net.Sockets.IrDAClient"/>, and
    /// <see cref="T:System.Net.Sockets.TcpClient"/>.
    /// </para>
    /// </remarks>
    internal sealed class SocketClientAdapter : SocketStreamIOAdapter
    {
        readonly BluetoothClient cli1B;
        readonly Socket sIT;

        public SocketClientAdapter(BluetoothClient cli)
            : base(cli.GetStream())
        {
            cli1B = cli;
        }

#if !NO_IRDA
        public SocketClientAdapter(IrDAClient cli)
            : base(cli.GetStream())
        {
            sIT = cli.Client;
        }
#endif

        public SocketClientAdapter(TcpClient cli)
            : base(cli.GetStream())
        {
            sIT = cli.Client;
        }

        //----
        public override EndPoint LocalEndPoint
        {
            get
            {
                if (cli1B != null)
                    // Not ideal!
                    return new BluetoothEndPoint(BluetoothAddress.None, Guid.Empty);
                else
                    return sIT.LocalEndPoint;
            }
        }

        public override EndPoint RemoteEndPoint
        {
            get
            {
                if (cli1B != null)
                    return cli1B.RemoteEndPoint;
                else
                    return sIT.RemoteEndPoint;
            }
        }
        //
        public override int Available
        {
            get
            {
                if (cli1B != null)
                    return cli1B.Available;
                else
                    return sIT.Available;
            }
        }

    }
}
