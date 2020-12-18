using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace InTheHand.Net.Bluetooth.Factory
{
#pragma warning disable 1591
    /// <exclude/>
    public abstract class DecoratorNetworkStream : System.Net.Sockets.NetworkStream
    {
        readonly Stream m_child;

        [SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly",
            Justification = "NetworkStream needs Finalization but we don't.")]
        protected DecoratorNetworkStream(Stream child)
            : base(GetAConnectedSocket(), false)
        {
            GC.SuppressFinalize(this); // NetworkStream needs Finalization but we don't.
            if (child == null) throw new ArgumentNullException("child");
            if (!child.CanRead || !child.CanWrite)
                throw new ArgumentException("Child stream must be connected.");
            m_child = child;
        }

        //----
        internal static System.Net.Sockets.Socket GetAConnectedSocket()
        {
            Socket s = SocketPair.GetConnectedSocket();
            Debug.Assert(s != null);
            Debug.Assert(s.Connected);
            return s;
        }

        sealed class SocketPair
        {
            Socket m_cli;
            [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields",
                Justification = "m_svr is there to stop the Socket's Finalization.")]
            Socket m_svr;
            static SocketPair m_SocketPair;

            //--------
            internal static Socket GetConnectedSocket()
            {
                // No need for locking etc here, as it's ok to make one or more (not 
                // many hopefully however!)  The socket (is meant!) to be only used on 
                // initialising the base NetworkStream, so it doesn't matter if the 
                // SocketPair is finalized either.  Better to create only one, hence 
                // why we cache it.
                // Careful of a race between accessing it, it becoming null, and the
                // Finalizer running, so keep a reference locally.
                SocketPair sp = m_SocketPair;
                if (sp == null || !sp.Alive)
                    m_SocketPair = sp = SocketPair.Create();
                return sp.m_cli;
            }

            //--------
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
            internal static SocketPair Create()
            {
                try {
                    return Create(AddressFamily.InterNetworkV6);
                } catch {
                    return Create(AddressFamily.InterNetwork);
                }
            }

            internal static SocketPair Create(AddressFamily af)
            {
                return new SocketPair(af);
            }

            //--------
            private SocketPair(AddressFamily af)
            {
                using (Socket lstnr = new Socket(af, SocketType.Stream, ProtocolType.Unspecified)) {
                    lstnr.Bind(new IPEndPoint(
                        af == AddressFamily.InterNetworkV6 ? IPAddress.IPv6Loopback : IPAddress.Loopback, 0));
                    lstnr.Listen(1);
                    EndPoint svrEp = lstnr.LocalEndPoint;
                    m_cli = new Socket(svrEp.AddressFamily, lstnr.SocketType, lstnr.ProtocolType);
                    m_cli.Connect(svrEp);
                    m_svr = lstnr.Accept();
                }
            }

            //--------
            private bool Alive
            {
                get
                {
                    return m_cli != null // just for safety, shouldn't occur
                        && m_cli.Connected;
                }
            }
        }//class2

        //----
        public abstract override bool DataAvailable { get; }

        public override bool CanRead
        {
            get
            {
                if (m_child == null) {
                    //#if !ANDROID
                    Debug.Assert(PlatformVerification.IsMonoRuntime);
                    //#endif
                    return true; // NS on Mono calls this virtual property from its c'tor!
                }
                return m_child.CanRead;
            }
        }

        public override bool CanSeek
        {
            get { return m_child.CanSeek; }
        }

        public override bool CanWrite
        {
            get
            {
                if (m_child == null) {
                    //#if !ANDROID
                    Debug.Assert(PlatformVerification.IsMonoRuntime);
                    //#endif
                    return true; // NS on Mono calls this virtual property from its c'tor!
                }
                return m_child.CanWrite;
            }
        }

        public override void Flush()
        {
            m_child.Flush();
        }

        public override long Length
        {
            get { return m_child.Length; }
        }

        public override long Position
        {
            get { return m_child.Position; }
            set { m_child.Position = value; }
        }

        public override bool CanTimeout
        {
            get { return m_child.CanTimeout; }
        }

        public override int ReadTimeout
        {
            get { return m_child.ReadTimeout; }
            set { m_child.ReadTimeout = value; }
        }

        public override int WriteTimeout
        {
            get { return m_child.WriteTimeout; }
            set { m_child.WriteTimeout = value; }
        }

        [DebuggerStepThrough]
        public override int Read(byte[] buffer, int offset, int count)
        {
            return m_child.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return m_child.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            m_child.SetLength(value);
        }

        [DebuggerStepThrough]
        public override void Write(byte[] buffer, int offset, int count)
        {
            m_child.Write(buffer, offset, count);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return m_child.EndRead(asyncResult);
        }

        [DebuggerStepThrough]
        public override void EndWrite(IAsyncResult asyncResult)
        {
            m_child.EndWrite(asyncResult);
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return m_child.BeginRead(buffer, offset, count, callback, state);
        }

        [DebuggerStepThrough]
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return m_child.BeginWrite(buffer, offset, count, callback, state);
        }

        protected override void Dispose(bool disposing)
        {
            try {
                if (disposing)
                    m_child.Close();
            } finally {
                base.Dispose(disposing);
            }
        }

    }//class
}
