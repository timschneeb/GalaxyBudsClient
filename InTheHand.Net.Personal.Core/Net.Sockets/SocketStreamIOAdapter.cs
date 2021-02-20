using System;
using System.IO;
using System.Net.Sockets; // e.g. SocketFlags

namespace InTheHand.Net.Sockets
{
    abstract class SocketStreamIOAdapter : SocketAdapter
    {
        readonly Stream m_strm;

        protected SocketStreamIOAdapter(Stream strm)
        {
            if (strm == null)
                throw new ArgumentNullException("strm");
            if (!strm.CanRead || !strm.CanWrite)
                throw new ArgumentException("Stream is closed.");
            m_strm = strm;
        }

        //
        public override int Receive(byte[] buffer, int size, SocketFlags socketFlags)
        {
            return Receive(buffer, 0, size, socketFlags);
        }

        public override int Receive(byte[] buffer, int offset, int size, SocketFlags socketFlags)
        {
            CheckSocketFlags(socketFlags);
            return m_strm.Read(buffer, offset, size);
        }

        public override int Send(byte[] buffer)
        {
            m_strm.Write(buffer, 0, buffer.Length);
            return buffer.Length;
        }

        //
        public override void Close()
        {
            Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                m_strm.Close();
            }
        }

    }
}
